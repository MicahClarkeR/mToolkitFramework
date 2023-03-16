using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using mToolkitPlatformComponentLibrary;
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
    public abstract class mTool : IDisposable
    {
        public readonly ILog Log;
        public mToolConfig Config;
        public ToolInfo Info;
        public DirectoryInfo ParentDirectory;
        public string GUID;
        public string Name { get { return Info.Name; } }
        public string InternalName { get { return Info.InternalName; } }
        public string Author { get { return Info.Author; } }
        public string Version { get { return Info.Version; } }
        public string Description { get { return Info.Description; } }

        public mTool(string guid, string directory)
        {
            GUID = guid;
            ParentDirectory = new DirectoryInfo(directory);
            Log = LogManager.GetLogger(GetToolType());
            Info = GetInfo();

            XNamespace log4netNs = "http://logging.apache.org/log4net/schemas/log4net.xsd";

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

            Config = new mToolConfig(this);
        }

        public string GetLog()
        {
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
            IAppender[] appenders = LogManager.GetRepository().GetAppenders();
            FileAppender appender = appenders.FirstOrDefault(a => a.Name == $"LogFileAppender") as FileAppender;
            Process.Start("notepad.exe", appender.File);
        }

        public UserControl Create()
        {
            UserControl control = CreateUI();

            return control;
        }

        public string GetToolDirectory()
        {
            return $"{ParentDirectory.FullName}";
        }


        public abstract void Initialise();
        public abstract UserControl CreateUI();

        public virtual void Focused()
        {
            Log.Info("Tool focused.");
        }

        public virtual void Unfocused()
        {
            Log.Info("Tool unfocused.");
        }

        protected abstract Type GetToolType();

        internal class DumpVariable
        {
            public readonly string Name, Value;

            public DumpVariable(string name, string? value)
            {
                Name = name;
                Value = value ?? string.Empty;
            }
        }

        internal void DumpTool(string fatal, params DumpVariable[] diagnostics)
        {
            string now = DateTime.Now.ToString();
            XElement root = new XElement("tooldump",
                                new XAttribute("Tool", Name),
                                new XAttribute("Created", now),
                                new XAttribute("Version", Version));

            foreach (DumpVariable v in diagnostics)
            {
                root.Add(new XElement(v.Name, v.Value));
            }

            string dumpFile = $"{GetToolDirectory()}/Dumps/{Name}{now}.xml";

            // Create a new XML writer for the specified file path
            using (XmlWriter writer = XmlWriter.Create(dumpFile, new XmlWriterSettings() { Indent = true }))
            {
                // Write the XElement to the XML writer
                root.WriteTo(writer);
            }

            Log.Fatal(fatal);
            Log.Fatal($"Diagnostic information has been dumped to: {dumpFile}.");
        }

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

        protected abstract ToolInfo GetInfo();

        private bool disposed = false;

        // Implement IDisposable.
        public void Dispose()
        {
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
                    Config = null;
                    Info = null;
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