using System.Data;
using System.Xml;
using System.Xml.Linq;

namespace mToolkitPlatformComponentLibrary
{
    public class mToolConfig
    {
        private readonly string? ConfigDirectory;
        private readonly mTool Owner;
        private readonly XElement? Config = null;
        private readonly XElement? Variables = null;

        internal mToolConfig(mTool owner)
        {
            Owner = owner;

            // Get the path of the directory containing the executing assembly.
            ConfigDirectory = Owner.GetToolDirectory();

            if (ConfigDirectory != null)
            {
                // Attempt to get the tool configuration file.
                GetToolConfig(ConfigDirectory, out XElement? config);

                // If the tool configuration file was not found, create a new one.
                if (config == null)
                {
                    config = new XElement("tool",
                                new XAttribute("InternalName", Owner.InternalName),
                                new XElement("variables"));
                }

                // Get the <variables> element from the tool configuration file, creating it if necessary.
                XElement variables = config.Element("variables") ?? new XElement("variables");
                Config = config;

                // Set the Variables property to the <variables> element.
                Variables = variables;
                SaveConfig();
            }
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
            // Select the variable element that matches the specified key.
            XElement? variable = Variables?.Elements()
                .FirstOrDefault(v => v.Attribute("name")?.Value == key);

            if (variable != null)
            {
                value = variable.Attributes().FirstOrDefault(v => v.Name == "value")?.Value;
                return true;
            }

            value = string.Empty;
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
            SaveConfig();

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
        private void GetToolConfig(string directory, out XElement? config)
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
        /// Saves the current configuration as an XML file to the ConfigDirectory, if it is not null.
        /// </summary>
        private void SaveConfig()
        {
            if (ConfigDirectory != null && Config != null)
            {
                // Create a new XML writer for the specified file path
                using (XmlWriter writer = XmlWriter.Create($"{ConfigDirectory}\\tool.config", new XmlWriterSettings() { Indent = true }))
                {
                    // Write the XElement to the XML writer
                    Config.WriteTo(writer);
                }
            }
        }
    }
}
