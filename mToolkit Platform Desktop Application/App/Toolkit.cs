using Microsoft.Win32;
using mToolkitPlatformComponentLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Xml.Linq;

namespace mToolkitPlatformDesktopLauncher.App
{
    internal class Toolkit
    {
        public static readonly CallbackToolDictionary<string, mTool> Tools = new CallbackToolDictionary<string, mTool>();
        private static readonly Dictionary<string, mToolApplicationContext> ToolDLLContexts = new Dictionary<string, mToolApplicationContext>();
        public static readonly string ToolDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Tools");

        /// <summary>
        /// Searches the Tools directory for subdirectories containing a tool.config file and a DLL with the same name as the InternalName attribute in the tool.config file.
        /// For each subdirectory found, loads the DLL and creates an instance of the Tool class defined in the DLL.
        /// </summary>
        /// <param name="window">The main window of the application.</param>
        public static void LoadTools(Window window)
        {
            // Construct the path to the Tools directory

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
                            Tools.Add(tool.GUID, tool);
                    }
                    catch(Exception ex)
                    {
                        tool?.Log.Error(ex);
                        MessageBox.Show("Critical error encountered when creating mTool UI, please check log.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private static mTool? CreateToolFromDLL(string dllFile)
        {
            using (var stream = File.OpenRead(dllFile))
            {
                mToolApplicationContext context = new mToolApplicationContext();
                Assembly assembly = context.LoadFromStream(stream);

                // Find the first type that is a subclass of mTool
                Type toolType = assembly.GetTypes().FirstOrDefault(type => type.IsSubclassOf(typeof(mTool)));

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
                        ToolDLLContexts.Add(tool.GUID, context);
                        return tool;
                    }
                }
            }

            return null;
        }

        public static void UnloadTool(mTool tool)
        {
            mToolApplicationContext? contexts = ToolDLLContexts[tool.GUID];

            if(contexts != null)
            {
                contexts.Unload();
                ToolDLLContexts.Remove(tool.GUID);
                Tools.Remove(tool.GUID);
                tool.Dispose();
            }
        }

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
                    tool?.Log.Error(ex);
                    MessageBox.Show("Critical error encountered when creating mTool UI, please check log.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }

            return newTool;
        }

        internal class CallbackToolDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TValue : mTool
        {
            private List<Action<TKey, mTool>> CreationCallbacks = new List<Action<TKey, mTool>>();
            public readonly Dictionary<string, UserControl> UIs = new Dictionary<string, UserControl>();

            public new void Add(TKey key, TValue value)
            {
                value.Log.Debug($"Adding to Toolkit dictionary.");
                base.Add(key, value);

                value.Log.Debug($"Registering and creating UI.");
                // Add a new UI to the UIs dictionary
                UIs.Add(value.GUID, value.Create());

                value.Log.Debug($"Cycling through post-creation callbacks.");
                CreationCallbacks.ForEach((e) => e(key, value));
            }

            public new void Remove(TKey key)
            {
                mTool tool = this[key];
                tool.Log.Debug($"Removing from Toolkit dictionary.");
                base.Remove(key);

                tool.Log.Debug($"De-registering UI.");
                CreationCallbacks.Clear();

                // Add a new UI to the UIs dictionary
                UIs.Remove(tool.GUID);
            }

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
