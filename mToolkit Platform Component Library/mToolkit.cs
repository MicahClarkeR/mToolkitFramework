﻿using System.Reflection;
using System.Runtime.Loader;
using System.Windows;
using System.Windows.Controls;

namespace mToolkitPlatformComponentLibrary
{
    /// <summary>
    /// A class that provides functionality for loading, unloading, and managing mTool instances.
    /// </summary>
    public class mToolkit
    {
        public static readonly CallbackToolDictionary<string, mTool> Tools = new CallbackToolDictionary<string, mTool>();
        private static readonly Dictionary<string, AssemblyLoadContext> ToolDLLContexts = new Dictionary<string, AssemblyLoadContext>();
        public static readonly string ToolDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Tools");

        /// <summary>
        /// Searches the Tools directory for subdirectories containing a tool.config file and a DLL with the same name as the InternalName attribute in the tool.config file.
        /// For each subdirectory found, loads the DLL and creates an instance of the Tool class defined in the DLL.
        /// </summary>
        /// <param name="window">The main window of the application.</param>
        public static void LoadTools(Window window)
        {
            // Check if the directory is null or empty
            if (string.IsNullOrEmpty(ToolDirectory))
                return;

            // Create the Tools directory if it does not exist
            if (!Directory.Exists(ToolDirectory))
            {
                Directory.CreateDirectory(ToolDirectory);
            }

            // Get the subdirectories of the Tools directory
            string[] toolDirectories = Directory.GetDirectories(ToolDirectory);

            // Loop through each subdirectory
            foreach (string toolDirectory in toolDirectories)
            {
                // Get the DLL files in the subdirectory
                string[] dllFiles = Directory.GetFiles(toolDirectory, "*.dll");

                foreach (string dllFile in dllFiles)
                {
                    mTool? tool = CreateToolFromDLL(dllFile);

                    try
                    {
                        if (tool != null)
                        {
                            tool.GUID = new FileInfo(dllFile).Directory.FullName;
                            Tools.Add(tool.GUID, tool);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        tool?.DumpTool(ex.Message);
                        tool?.CurrentLog.Error(ex);
                        MessageBox.Show("Critical error encountered when creating mTool UI, please check log.\n" + dllFile, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Creates an mTool instance from the specified DLL file.
        /// </summary>
        /// <param name="dllFile">The path to the DLL file.</param>
        /// <returns>An mTool instance if successful, or null if there was an error.</returns>
        private static mTool? CreateToolFromDLL(string dllFile)
        {
            try
            {
                Type toolType = null;
                try
                {
                    // Load the assembly using the default assembly loading context
                    Assembly assembly = Assembly.LoadFrom(dllFile);

                    // Find the first type that is a subclass of mTool
                    toolType = assembly.GetTypes().FirstOrDefault(type => type.IsSubclassOf(typeof(mTool)));
                }
                catch (Exception ex) { }

                if (toolType != null)
                {
                    // Get the Tool class constructor that takes a Window, a string, a string, a string, a string, and a string as parameters
                    ConstructorInfo[] constructors = toolType.GetConstructors();
                    ConstructorInfo? constructor = constructors[0];

                    // Check if the constructor is not null
                    if (constructor != null)
                    {
                        // Create a new instance of the Tool class and cast it to the Tool type
                        string guid = Guid.NewGuid().ToString();
                        mTool tool = (mTool)constructor.Invoke(new object[] { guid, Path.GetDirectoryName(dllFile) });
                        return tool;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }

        /// <summary>
        /// Unloads all loaded mTools.
        /// </summary>
        public static void UnloadTools()
        {
            foreach (mTool tool in Tools.Values.ToArray())
            {
                Tools.Remove(tool.GUID);
                // tool.CurrentWorkspace.Clear();
            }
        }

        /// <summary>
        /// Unloads a specific mTool.
        /// </summary>
        /// <param name="tool">The mTool to unload.</param>
        public static void UnloadTool(mTool tool)
        {
            AssemblyLoadContext? contexts = ToolDLLContexts[tool.GUID];

            if (contexts != null)
            {
                // contexts.Unload();
                ToolDLLContexts.Remove(tool.GUID);
                Tools.Remove(tool.GUID);
                tool.Dispose();
            }
        }

        /// <summary>
        /// Reloads an mTool instance.
        /// </summary>
        /// <param name="tool">The mTool instance to reload.</param>
        /// <returns>The reloaded mTool instance, or null if there was an error.</returns>
        public static mTool? Reload(mTool tool)
        {
            string path = null;
            foreach (string key in Tools.Keys)
            {
                if (tool == Tools[key])
                {
                    path = key;
                    break;
                }
            }

            if (path == null)
                return null;

            mTool? newTool = CreateToolFromDLL(path);

            if (newTool != null)
            {
                try
                {
                    Tools.Update(path, newTool);
                }
                catch (Exception ex)
                {
                    newTool.CurrentLog.Error(ex);
                    MessageBox.Show("Critical error encountered when creating mTool UI, please check log.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }

            return newTool;
        }

        /// <summary>
        /// A custom dictionary class that allows for registering creation callbacks when mTool instances are added or removed.
        /// </summary>
        public class CallbackToolDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TValue : mTool
        {
            private List<Action<TKey, mTool>> CreationCallbacks = new List<Action<TKey, mTool>>();
            public readonly Dictionary<string, UserControl> UIs = new Dictionary<string, UserControl>();

            public new void Add(TKey key, TValue value)
            {
                value.CurrentLog.Debug($"Adding to Toolkit dictionary.");
                base.Add(key, value);

                value.CurrentLog.Debug($"Registering and creating UI.");
                // Add a new UI to the UIs dictionary
                UIs.Add(value.GUID, value.Create());

                value.CurrentLog.Debug($"Cycling through post-creation callbacks.");
                CreationCallbacks.ForEach((e) => e(key, value));
            }

            public new void Remove(TKey key)
            {
                mTool tool = this[key];
                tool.CurrentLog.Debug($"Removing from Toolkit dictionary.");
                base.Remove(key);

                tool.CurrentLog.Debug($"De-registering UI.");
                CreationCallbacks.Clear();

                // Remove the UI from the UIs dictionary
                UIs.Remove(tool.GUID);
            }

            /// <summary>
            /// Adds a creation callback to be called when mTool instances are added or removed.
            /// </summary>
            /// <param name="callback">The callback to add.</param>
            public void Update(TKey key, TValue value)
            {
                mTool old = this[key];
                base.Remove(key);
                UIs.Remove(old.GUID);
                UIs.Add(value.GUID, value.Create());
                base.Add(key, value);
            }

            public void AddCreationCallback(Action<TKey, mTool> callback)
            {
                CreationCallbacks.Add(callback);
            }
        }
    }
}
