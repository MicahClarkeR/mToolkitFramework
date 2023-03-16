using mToolkitPlatformComponentLibrary;
using mToolkitPlatformDesktopLauncher.App;
using mToolkitPlatformDesktopLauncher.App.Windows;
using System;
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
        private static MainWindow? Current = null;

        public MainWindow()
        {
            Current = this;
            InitializeComponent();

            Toolkit.Tools.AddCreationCallback((s, m) => AddTool(m));
            Toolkit.LoadTools(this);

            if (ToolsTabControl.Items.Count == 0)
            {
                UpdateContextMenu(null, null);
            }
        }

        /// <summary>
        /// Adds a new tool to the framework's main window.
        /// </summary>
        /// <param name="tool">The mTool instance to be added.</param>
        private void AddTool(mTool tool)
        {
            tool.Log.Debug("Adding mTool to the framework Main Window.");
            UserControl control = Toolkit.Tools.UIs[tool.GUID];

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

            tool.Log.Debug("Context menu created.");
        }

        private TabItem CreateTabItem(mTool tool, UserControl control)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            grid.Children.Add(control);
            Grid.SetRow(control, 0);
            Grid.SetColumn(control, 0);
            Grid.SetRowSpan(control, 1);
            Grid.SetColumnSpan(control, 1);

            tool.Log.Debug("Created and set in Main Window grid.");

            return new TabItem
            {
                Header = tool.ParentDirectory.Name,
                Content = grid
            };
        }

        private void UpdateMainWindow(TabItem tab)
        {
            Title = $"{tab.Header} - mTool Framework";
        }

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
            toolSubmenu.Items.Add(CreateMenuItem("Restart mToolkit Platform", (sender, e) => mToolkitPlatform.Desktop.App.Restart()));
            toolSubmenu.Items.Add(CreateMenuItem("Install Tool From Repo", (sender, e) => new WindowToolRepoInstaller().ShowDialog()));

            if (tool != null)
            {
                toolSubmenu.Items.Add(new Separator());
                // toolSubmenu.Items.Add(CreateMenuItem("Unload Tool", (sender, e) => UnloadTool(tool)));
                toolSubmenu.Items.Add(CreateMenuItem("View Tool Log", (sender, e) => tool.OpenLog()));
            }

            ContextMenu.Items.Add(toolSubmenu);
        }

        private void UnloadTool(mTool tool)
        {
            TabItem owner = (TabItem)ToolsTabControl.SelectedItem;

            if (owner != null)
            {
                Grid grid = (Grid)owner.Content;
                UserControl ui = Toolkit.Tools.UIs[tool.GUID];

                grid.Children.Remove(ui);
                ContextMenu.Items.Clear();

                ui.ContextMenu = null;
                ContextMenu = null;
                owner.ContextMenu = null;

                Toolkit.UnloadTool(tool);
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
