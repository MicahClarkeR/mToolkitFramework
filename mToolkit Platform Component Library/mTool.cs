using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using mToolkitPlatformComponentLibrary;
using mToolkitPlatformComponentLibrary.Workspace;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;

namespace mToolkitPlatformComponentLibrary
{
    /// <summary>
    /// Represents a tool that can be used within an application.
    /// </summary>
    public abstract class mTool : IDisposable
    {
        // Static field used to store the root path of the framework.
        public static string FrameworkRootPath;

        // Instance fields.
        public string CurrentFrameworkRootPath;
        public readonly ILog CurrentLog;
        public readonly mToolConfig CurrentConfig;
        public readonly mToolWorkspace CurrentWorkspace;
        public readonly ToolInfo CurrentInfo;
        public readonly DirectoryInfo CurrentParentDirectory;
        public string GUID;

        /// <summary>
        /// Gets the name of the tool.
        /// </summary>
        public string Name { get { return CurrentInfo.Name; } }

        /// <summary>
        /// Gets the internal name of the tool.
        /// </summary>
        public string InternalName { get { return CurrentInfo.InternalName; } }

        /// <summary>
        /// Gets the author of the tool.
        /// </summary>
        public string Author { get { return CurrentInfo.Author; } }

        /// <summary>
        /// Gets the version of the tool.
        /// </summary>
        public string Version { get { return CurrentInfo.Version; } }

        /// <summary>
        /// Gets the description of the tool.
        /// </summary>
        public string Description { get { return CurrentInfo.Description; } }

        /// <summary>
        /// Initializes a new instance of the mTool class.
        /// </summary>
        /// <param name="guid">The GUID of the tool.</param>
        /// <param name="directory">The directory where the tool is located.</param>
        /// <param name="config">The configuration for the tool.</param>
        public mTool(string guid, string directory, mToolConfig? config = null)
        {
            // Initialize fields.
            GUID = guid;
            CurrentParentDirectory = new DirectoryInfo(directory);
            CurrentFrameworkRootPath = FrameworkRootPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            CurrentLog = LogManager.GetLogger(GetToolType());
            CurrentInfo = GetInfo();
            CurrentWorkspace = new mToolWorkspace(this);

            // Create log.
            CreateLog();

            // Create configuration if none was provided.
            if (config == null)
                config = mToolConfig.Create(this);

            CurrentConfig = config;
        }

        /// <summary>
        /// Sets the configuration for the tool.
        /// </summary>
        /// <param name="config">The configuration for the tool.</param>
        public void SetConfig(mToolConfig config)
        { }

        /// <summary>
        /// Creates the log for the tool.
        /// </summary>
        private void CreateLog()
        {
            // Namespace used for log4net.
            XNamespace log4netNs = "http://logging.apache.org/log4net/schemas/log4net.xsd";

            // Create the log4net configuration.
            XElement log4netConfig = new XElement(log4netNs + "log4net",
                new XElement(log4netNs + "appender",
                    new XAttribute("name", $"LogFileAppender"),
                    new XAttribute("type", "log4net.Appender.FileAppender"),
                    new XElement(log4netNs + "file",
                        new XAttribute("value", $"{GetToolDirectory()}\\LogFile.txt")
                    ),
                    new XElement(log4netNs + "appendToFile",
                        new XAttribute("value", "true")
                    ),
                    new XElement(log4netNs + "lockingModel",
                        new XAttribute("type", "log4net.Appender.FileAppender+MinimalLock")
                    ),
                    new XElement(log4netNs + "layout",
                        new XAttribute("type", "log4net.Layout.PatternLayout"),
                        new XElement(log4netNs + "conversionPattern",
                            new XAttribute("value", "%date %level %logger - %message%newline")
                        )
                    )
                ),
                new XElement(log4netNs + "root",
                    new XElement(log4netNs + "level",
                        new XAttribute("value", "DEBUG")
                    ),
                    new XElement(log4netNs + "appender-ref",
                        new XAttribute("ref", "LogFileAppender")
                    )
                )
            );

            // Convert the XElement to an XmlDocument
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(log4netConfig.ToString());

            // Convert the XmlDocument to an XmlElement
            XmlElement rootElement = doc.DocumentElement;

            // Initialize log4net with the configuration file
            XmlConfigurator.Configure(rootElement);
        }

        public string GetLog()
        {
            // Get the file appender for the log file
            FileAppender appender = LogManager.GetRepository().GetAppenders().FirstOrDefault(a => a.Name == $"LogFileAppender") as FileAppender;

            // Read the contents of the log file
            var logContents = string.Empty;
            using (var reader = new StreamReader(appender.File, Encoding.UTF8))
            {
                logContents = reader.ReadToEnd();
            }
            return logContents;
        }

        public void OpenLog()
        {
            // Get all appenders
            IAppender[] appenders = LogManager.GetRepository().GetAppenders();
            // Find the file appender for the log file
            FileAppender appender = appenders.FirstOrDefault(a => a.Name == $"LogFileAppender") as FileAppender;
            // Open the log file in Notepad
            Process.Start("notepad.exe", appender.File);
        }

        public UserControl Create()
        {
            // Create the user interface control for the tool
            UserControl control = CreateUI();
            return control;
        }

        public string GetToolDirectory()
        {
            // Return the full name of the tool's parent directory
            return $"{CurrentParentDirectory.FullName}";
        }

        /// <summary>
        /// Called to initialize the tool.
        /// </summary>
        public abstract void Initialise();

        /// <summary>
        /// Called to create the user interface for the tool.
        /// </summary>
        /// <returns>The user interface control for the tool.</returns>
        public abstract UserControl CreateUI();

        public virtual void Focused()
        {
            // Log that the tool has been focused
            CurrentLog.Info("Tool focused.");
        }

        public virtual void Unfocused()
        {
            // Log that the tool has been unfocused
            CurrentLog.Info("Tool unfocused.");
        }

        /// <summary>
        /// Gets the type of the tool.
        /// </summary>
        /// <returns>The type of the tool.</returns>
        protected abstract Type GetToolType();

        /// <summary>
        /// A class representing a variable to be included in a tool dump.
        /// </summary>
        internal class DumpVariable
        {
            public readonly string Name, Value;

            public DumpVariable(string name, string? value)
            {
                Name = name;
                Value = value ?? string.Empty;
            }
        }

        // This method creates an XML file containing diagnostic information about the tool.
        // The XML file is saved in the "Dumps" subdirectory under the tool's directory.
        internal void DumpTool(string fatal, params DumpVariable[] diagnostics)
        {
            // Get the current date and time
            string now = DateTime.Now.ToString();

            // Create the root XElement for the XML file
            XElement root = new XElement("tooldump",
                                new XAttribute("Tool", Name),
                                new XAttribute("Created", now),
                                new XAttribute("Version", Version));

            // Add each diagnostic variable as a child XElement
            foreach (DumpVariable v in diagnostics)
            {
                root.Add(new XElement(v.Name, v.Value));
            }

            // Construct the file path for the XML file
            string dumpFile = $"{GetToolDirectory()}/Dumps/{Name}{now}.xml";

            // Create a new XML writer for the specified file path
            using (XmlWriter writer = XmlWriter.Create(dumpFile, new XmlWriterSettings() { Indent = true }))
            {
                // Write the XElement to the XML writer
                root.WriteTo(writer);
            }

            // Log the fatal error and the location of the diagnostic information
            CurrentLog.Fatal(fatal);
            CurrentLog.Fatal($"Diagnostic information has been dumped to: {dumpFile}.");
        }

        // This class represents information about a tool, including its name, internal name, author, description, and version.
        public class ToolInfo
        {
            public readonly string Name;
            public readonly string InternalName;
            public readonly string Author;
            public readonly string Description;
            public readonly string Version;

            public ToolInfo(string name, string internalName, string author, string description, string version)
            {
                Name = name;
                InternalName = internalName;
                Author = author;
                Description = description;
                Version = version;
            }
        }

        // This method returns information about the tool.
        protected abstract ToolInfo GetInfo();

        private bool disposed = false;

        // This method is called when the tool is about to be closed.
        protected virtual void ToolClosing()
        { }

        // Implement IDisposable.
        public void Dispose()
        {
            ToolClosing();
            CurrentWorkspace.Clear();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Release unmanaged resources.
                disposed = true;
            }
        }

        // Destructor
        ~mTool()
        {
            Dispose(false);
        }
    }
}