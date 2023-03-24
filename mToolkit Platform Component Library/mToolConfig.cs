using Microsoft.Win32;
using System.Data;
using System.Security.Permissions;
using System.Security.Policy;
using System.Xml;
using System.Xml.Linq;
using static mToolkitPlatformComponentLibrary.mToolConfig;

namespace mToolkitPlatformComponentLibrary
{
    public class mToolConfig : mSegmentHandler
    {
        private XElement? Variables = null;

        public static mToolConfig? Create(mTool owner)
        {
            // Get the path of the directory containing the executing assembly.
            string configDirectory = owner.GetToolDirectory();

            // Attempt to get the tool configuration file.
            GetToolConfig(configDirectory, out XElement? config);

            // If the tool configuration file was not found, create a new one.
            if (config == null)
            {
                config = new XElement("tool",
                            new XAttribute("InternalName", owner.InternalName),
                            new XElement("variables"));
            }

            mToolConfig toolConfig = new mToolConfig(configDirectory, config, new XElement("segments"));

            if (configDirectory != null)
            {
                return toolConfig;
            }

            return null;
        }

        public mToolConfig(string configDirectory, XElement root, XElement segments, mConfigSaver? saver = null) : base(root, segments)
        {
            // Get the <variables> element from the tool configuration file, creating it if necessary.
            XElement variables = Owner.Element("variables") ?? new XElement("variables");

            // Set the Variables property to the <variables> element.
            Variables = variables;

            if (saver == null)
                saver = new mToolConfigSaver(configDirectory, Owner);

            Saver = saver;
            Saver?.SaveConfig();
        }

        /// <summary>
        /// Gets the value associated with the specified key from the <see cref="Variables"/> XML element.
        /// </summary>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key,
        /// or an empty string if the key was not found.</param>
        /// <returns>true if the key was found; otherwise, false.</returns>
        public bool Get(string key, out string value)
        {
            return Get(key, out value, null);
        }

        /// <summary>
        /// Gets the value associated with the specified key from the <see cref="Variables"/> XML element.
        /// </summary>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key,
        /// or an empty string if the key was not found.</param>
        /// <returns>true if the key was found; otherwise, false.</returns>
        public bool Get(string key, out string value, string? defaultValue)
        {
            // Select the variable element that matches the specified key.
            XElement? variable = Variables?.Elements()
                .FirstOrDefault(v => v.Attribute("name")?.Value == key);

            if (variable != null)
            {
                value = variable.Attributes().FirstOrDefault(v => v.Name == "value")?.Value;
                return true;
            }

            value = defaultValue ?? string.Empty;
            return false;
        }

        /// <summary>
        /// Updates a variable in the ToolConfig object, or creates a new variable if it doesn't exist.
        /// </summary>
        /// <param name="key">The name of the variable to update or create.</param>
        /// <param name="value">The value to set for the variable.</param>
        public bool Put(string key, string value)
        {
            // Check if Variables is null
            if (Variables == null)
            {
                // If it is, throw an exception
                throw new InvalidOperationException("Variables XElement is null.");
            }

            // Look for an XElement with a "name" attribute equal to the key
            XElement variable = Variables.Elements().FirstOrDefault(v => v.Attribute("name")?.Value == key);

            // If a matching XElement was found, update its value attribute to the new value
            if (variable != null)
            {
                variable.Attribute("value")?.SetValue(value);
            }
            else
            {
                // If a matching XElement was not found, create a new XElement with the given key and value attributes
                variable = new XElement("variable", new XAttribute("name", key), new XAttribute("value", value));
                Variables.Add(variable);
            }

            // Save the changes to the config file
            Saver?.SaveConfig();

            return true;
        }

        /// <summary>
        /// Searches the specified directory and its subdirectories for a tool configuration file named "tool.config",
        /// and returns the contents of the file as an XElement object if it is found.
        /// </summary>
        /// <param name="directory">The directory to search for the tool configuration file.</param>
        /// <param name="config">When this method returns, contains the XElement object representing the tool configuration file,
        /// or null if the file was not found.</param>
        /// <returns>true if the tool configuration file was found; otherwise, false.</returns>
        private static void GetToolConfig(string directory, out XElement? config)
        {

            // Initialize the output parameter.
            config = null;

            // Get a list of files in the tool directory.
            string[] files = Directory.GetFiles(directory);

            // Search for the tool configuration file in the list of files.
            XElement? toolConfig = files
                .Select(f =>
                {
                    FileInfo fi = new FileInfo(f);

                    // Check if the file extension is "config" and the file name is "tool".
                    if (fi.Name == "tool.config")
                    {
                        // Read the contents of the tool configuration file and parse it as an XElement object.
                        string contents = File.ReadAllText(f);
                        XElement result = XElement.Parse(contents);

                        return result;
                    }

                    return null;
                })
                .FirstOrDefault(x => x != null);

            // If the tool configuration file was found, set the output parameter and exit the method.
            if (toolConfig != null)
            {
                config = toolConfig;
                return;
            }
        }


        /// <summary>
        /// Creates a segment of the specified type using the provided key.
        /// </summary>
        /// <typeparam name="T">The type of the segment, must inherit from mToolConfigSegment.</typeparam>
        /// <param name="key">The key used to identify the segment.</param>
        /// <returns>The created segment, or null if not found.</returns>
        public T? CreateSegment<T>(string key) where T : mToolConfigSegment
        {
            SetSegments(Segments);
            return CreateSegment<T>(key, Owner);
        }

        /// <summary>
        /// Handles saving the configuration as an XML file.
        /// </summary>
        internal class mToolConfigSaver : mConfigSaver
        {
            private string ConfigDirectory;
            private XElement Root;

            /// <summary>
            /// Initializes a new instance of the mToolConfigSaver class.
            /// </summary>
            /// <param name="configDirectory">The directory where the configuration will be saved.</param>
            /// <param name="root">The XElement representing the configuration.</param>
            internal mToolConfigSaver(string configDirectory, XElement root)
            {
                ConfigDirectory = configDirectory;
                Root = root;
            }

            /// <summary>
            /// Saves the current configuration as an XML file to the ConfigDirectory, if it is not null.
            /// </summary>
            public void SaveConfig()
            {
                if (ConfigDirectory != null && Root != null)
                {
                    // Create a new XML writer for the specified file path
                    using (XmlWriter writer = XmlWriter.Create($"{ConfigDirectory}\\tool.config", new XmlWriterSettings() { Indent = true }))
                    {
                        // Write the XElement to the XML writer
                        Root.WriteTo(writer);
                    }
                }
            }
        }

        /// <summary>
        /// Represents a segment in the mTool configuration.
        /// </summary>
        public abstract class mToolConfigSegment
        {
            protected readonly XElement Owner;
            protected readonly mToolConfig Config;

            /// <summary>
            /// Initializes a new instance of the mToolConfigSegment class.
            /// </summary>
            /// <param name="segment">The XElement representing the segment.</param>
            /// <param name="config">The mToolConfig instance containing this segment.</param>
            public mToolConfigSegment(XElement segment, mToolConfig config)
            {
                Owner = segment;
                Config = config;
            }

            /// <summary>
            /// Retrieves the value associated with the specified key.
            /// </summary>
            /// <param name="key">The key for the value to retrieve.</param>
            /// <returns>The value associated with the specified key, or null if not found.</returns>
            public string? Get(string key)
            {
                foreach (XElement child in Owner.Elements())
                {
                    if (child.Name.LocalName.ToLower() == "value" && child.Attribute("name")?.Value == key)
                    {
                        return child.Value;
                    }
                }

                return null;
            }

            /// <summary>
            /// Retrieves a value of the specified type associated with the specified key.
            /// </summary>
            /// <typeparam name="T">The type of the value, must inherit from mToolConfigSegmentValue.</typeparam>
            /// <param name="key">The key for the value to retrieve.</param>
            /// <returns>The value associated with the specified key, or null if not found.</returns>
            public mToolConfigSegmentValue? Get<T>(string key) where T : mToolConfigSegmentValue
            {
                foreach (XElement child in Owner.Elements())
                {
                    if (child.Name.LocalName.ToLower() == "value" && child.Attribute("name")?.Value == key)
                    {
                        return Activator.CreateInstance(typeof(T), child) as T;
                    }
                }

                return null;
            }

            /// <summary>
            /// Retrieves the sub-segment of type T that corresponds to the given key.
            /// </summary>
            /// <typeparam name="T">The type of the sub-segment.</typeparam>
            /// <param name="key">The key corresponding to the sub-segment to retrieve.</param>
            /// <returns>The sub-segment of type T that corresponds to the given key, or null if no sub-segment with the given key was found.</returns>
            public mToolConfigSegment? GetSubSegment<T>(string key) where T : mToolConfigSegment
            {
                return GetSubSegment<T>(this, key);
            }

            /// <summary>
            /// Retrieves the sub-segment of type T that corresponds to the given key.
            /// </summary>
            /// <typeparam name="T">The type of the sub-segment.</typeparam>
            /// <param name="caller">The parent segment.</param>
            /// <param name="key">The key corresponding to the sub-segment to retrieve.</param>
            /// <returns>The sub-segment of type T that corresponds to the given key, or null if no sub-segment with the given key was found.</returns>
            private static mToolConfigSegment? GetSubSegment<T>(mToolConfigSegment caller, string key) where T : mToolConfigSegment
            {
                foreach (XElement child in caller.Owner.Elements())
                {
                    if (child.Name.LocalName.ToLower() == "segment" && child.Attribute("name")?.Value == key)
                    {
                        return Activator.CreateInstance(typeof(T), child) as T;
                    }
                }

                return null;
            }

            /// <summary>
            /// Represents a value in a configuration segment.
            /// </summary>
            public abstract class mToolConfigSegmentValue
            {
                /// <summary>
                /// The underlying XElement that contains the value.
                /// </summary>
                protected readonly XElement Value;

                /// <summary>
                /// Creates a new instance of the mToolConfigSegmentValue class.
                /// </summary>
                /// <param name="value">The underlying XElement that contains the value.</param>
                public mToolConfigSegmentValue(XElement value)
                {
                    Value = value;
                }

                /// <summary>
                /// Gets the value as type T.
                /// </summary>
                /// <typeparam name="T">The type to convert the value to.</typeparam>
                /// <returns>The value as type T.</returns>
                public abstract T Get<T>();
            }

        }
    }

    public class mSegmentHandler
    {
        public XElement Segments;
        public XElement? Owner;
        public mConfigSaver? Saver;

        public mSegmentHandler(XElement root, XElement segments)
        {
            SetOwner(root);
            SetSegments(segments);
        }

        /// <summary>
        /// Sets the XML element that this segment handler is associated with.
        /// </summary>
        /// <param name="owner">The XML element to set as the owner.</param>
        public void SetOwner(XElement owner)
        {
            this.Owner = owner;
        }

        /// <summary>
        /// Sets the XML element that contains the segments.
        /// </summary>
        /// <param name="segments">The XML element that contains the segments.</param>
        public void SetSegments(XElement segments)
        {
            if (Owner?.Elements("segments") != null)
                segments = Owner?.Element("segments");
            else
                Owner?.Add(segments);

            this.Segments = segments;
        }

        /// <summary>
        /// Gets a segment of the specified type with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the segment to get.</typeparam>
        /// <param name="key">The key of the segment to get.</param>
        /// <returns>The segment with the specified key, or null if it doesn't exist.</returns>
        public T? GetSegment<T>(string key) where T : mToolConfigSegment
        {
            if (Segments != null)
            {
                foreach (XElement child in Segments.Elements())
                {
                    if (child.Name.LocalName.ToLower() == typeof(T).Name.ToLower() && child.Attribute("name")?.Value == key)
                    {
                        return Activator.CreateInstance(typeof(T), child, this) as T;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a segment of the specified type with the specified key, if it doesn't already exist.
        /// </summary>
        /// <typeparam name="T">The type of the segment to create.</typeparam>
        /// <param name="key">The key of the segment to create.</param>
        /// <param name="element">The XML element to add the segment to.</param>
        /// <returns>The created segment, or null if it couldn't be created.</returns>
        public T? CreateSegment<T>(string key, XElement? element) where T : mToolConfigSegment
        {
            if (element.Element("segments") == null)
            {
                Segments = new XElement("segments");
                element.Add(Segments);
                Saver?.SaveConfig();
            }

            T? existing = GetSegment<T>(key);

            if (existing != null)
            {
                return existing;
            }

            XElement element2 = new XElement(typeof(T).Name.ToLower(), new XAttribute("name", key));
            T? segment = Activator.CreateInstance(typeof(T), element2, this) as T;

            if (segment != null)
            {
                Segments.Add(element2);
                Saver?.SaveConfig();
                return segment;
            }

            return null;
        }
    }

    /// <summary>
    /// Interface for classes that can save configuration data.
    /// </summary>
    public interface mConfigSaver
    {
        /// <summary>
        /// Saves the configuration data.
        /// </summary>
        public void SaveConfig();
    }
}
