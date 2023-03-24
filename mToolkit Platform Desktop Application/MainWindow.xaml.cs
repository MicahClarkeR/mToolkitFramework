using mToolkitPlatform.Desktop;
using mToolkitPlatformComponentLibrary;
using mToolkitPlatformDesktopLauncher.App;
using mToolkitPlatformDesktopLauncher.App.Windows;
using mToolkitPlatformDesktopLauncher.Pipelines;
using mToolkitPlatformDesktopLauncher.Properties;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace mToolkitPlatformDesktopLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Singleton instance of MainWindow.
        /// </summary>
        private static MainWindow? Current = null;

        /// <summary>
        /// Static constructor for MainWindow. Registers the "statusbar" pipeline.
        /// </summary>
        static MainWindow()
        {
            mFrameworkApplication.Pipelines.RegisterPipeline("statusbar", new StatusbarPipeline());
        }

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            Current = this;
            InitializeComponent();

            mToolkit.Tools.AddCreationCallback((s, m) => AddTool(m));
            mToolkit.LoadTools(this);

            if (ToolsTabControl.Items.Count == 0)
            {
                UpdateContextMenu(null, null);
            }

            this.Left = Settings.Default.WindowLeft;
            this.Top = Settings.Default.WindowTop;
            this.Width = Settings.Default.WindowWidth;
            this.Height = Settings.Default.WindowHeight;
        }

        /// <summary>
        /// Handles the Window Closing event. Saves the window position and size, and unloads the tools.
        /// </summary>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Save window position and size to the settings
            Settings.Default.WindowLeft = this.Left;
            Settings.Default.WindowTop = this.Top;
            Settings.Default.WindowWidth = this.Width;
            Settings.Default.WindowHeight = this.Height;

            // Save the settings
            Settings.Default.Save();
            mToolkit.UnloadTools();
        }

        /// <summary>
        /// Adds a new tool to the framework's main window.
        /// </summary>
        /// <param name="tool">The mTool instance to be added.</param>
        private void AddTool(mTool tool)
        {
            tool.CurrentLog.Debug("Adding mTool to the framework Main Window.");
            UserControl control = mToolkit.Tools.UIs[tool.GUID];

            TabItem newTab = CreateTabItem(tool, control);
            ToolsTabControl.Items.Add(newTab);

            newTab.GotFocus += (s, e) =>
            {
                UpdateMainWindow(newTab);
                UpdateContextMenu(control, tool);
            };

            // Initialize the main window title and context menu if this is the first tool.
            if (ToolsTabControl.Items.Count == 1)
            {
                ToolsTabControl.Dispatcher.BeginInvoke(() =>
                {
                    Thread.Sleep(10);
                    UpdateMainWindow(newTab);
                    UpdateContextMenu(control, tool);
                });
            }

            tool.CurrentLog.Debug("Context menu created.");
        }

        /// <summary>
        /// Creates a TabItem with the specified tool and control.
        /// </summary>
        private TabItem CreateTabItem(mTool tool, UserControl control)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            control.HorizontalAlignment = HorizontalAlignment.Stretch;
            control.VerticalAlignment = VerticalAlignment.Stretch;

            grid.Children.Add(control);

            tool.CurrentLog.Debug("Created and set in Main Window grid.");

            return new TabItem
            {
                Header = tool.CurrentParentDirectory.Name,
                Content = grid
            };
        }

        /// <summary>
        /// Updates the main window title based on the current tab.
        /// </summary>
        private void UpdateMainWindow(TabItem tab)
        {
            Title = $"{tab.Header} - mTool Framework";
        }

        /// <summary>
        /// Updates the context menu based on the control and tool provided.
        /// </summary>
        /// <param name="control">The UserControl instance associated with the tool.</param>
        /// <param name="tool">The mTool instance.</param>
        private void UpdateContextMenu(UserControl? control, mTool? tool)
        {
            ContextMenu = new ContextMenu();

            if (control?.ContextMenu != null)
            {
                foreach (MenuItem item in control.ContextMenu.Items)
                {
                    ContextMenu.Items.Add(item);
                }

                ContextMenu.Items.Add(new Separator());
            }

            MenuItem toolSubmenu = new MenuItem()
            {
                Header = "Framework Menu"
            };
            toolSubmenu.Items.Add(CreateMenuItem("Restart mToolkit Platform", (sender, e) => mFrameworkDesktop.Restart()));
            toolSubmenu.Items.Add(new Separator());
            toolSubmenu.Items.Add(CreateMenuItem("Install Tool From Repo...", (sender, e) => new WindowToolRepoInstaller().ShowDialog()));

            if (tool != null)
            {
                toolSubmenu.Items.Add(new Separator());
                // toolSubmenu.Items.Add(CreateMenuItem("Unload Tool", (sender, e) => UnloadTool(tool)));
                toolSubmenu.Items.Add(CreateMenuItem("View Tool Log", (sender, e) => tool.OpenLog()));
            }

            ContextMenu.Items.Add(toolSubmenu);
        }

        /// <summary>
        /// Unloads the specified tool from the main window.
        /// </summary>
        /// <param name="tool">The mTool instance to be unloaded.</param>
        private void UnloadTool(mTool tool)
        {
            TabItem owner = (TabItem)ToolsTabControl.SelectedItem;

            if (owner != null)
            {
                Grid grid = (Grid)owner.Content;
                UserControl ui = mToolkit.Tools.UIs[tool.GUID];

                grid.Children.Remove(ui);
                ContextMenu.Items.Clear();

                ui.ContextMenu = null;
                ContextMenu = null;
                owner.ContextMenu = null;

                mToolkit.UnloadTool(tool);
                ToolsTabControl.Items.Remove(owner);

                if (ToolsTabControl.Items.Count > 0)
                {
                    TabItem next = (TabItem)ToolsTabControl.Items[0];
                    next.Focus();
                }

                GC.Collect();
                owner.Content = grid;
            }
        }

        /// <summary>
        /// Creates a MenuItem with the specified header and click event callback.
        /// </summary>
        /// <param name="header">The header text for the menu item.</param>
        /// <param name="callback">The RoutedEventHandler to be invoked when the menu item is clicked.</param>
        /// <returns>A new MenuItem instance.</returns>
        private MenuItem CreateMenuItem(string header, RoutedEventHandler callback)
        {
            MenuItem item = new MenuItem
            {
                Header = header
            };
            item.Click += callback;
            return item;
        }
    }
}
