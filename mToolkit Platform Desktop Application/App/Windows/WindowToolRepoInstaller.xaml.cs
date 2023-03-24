using mToolkitPlatform.Desktop;
using mToolkitPlatformComponentLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace mToolkitPlatformDesktopLauncher.App.Windows
{
    /// <summary>
    /// Interaction logic for WindowToolRepoInstaller.xaml
    /// </summary>
    public partial class WindowToolRepoInstaller : Window
    {
        private string[] zips = new string[0];
        public WindowToolRepoInstaller()
        {
            InitializeComponent();
            LoadFromRepo();
        }

        private void LoadFromRepo()
        {
            zips = ToolRepository.GetRepoItems();

            foreach (string item in zips)
                RepoListBox.Items.Add(Path.GetFileNameWithoutExtension(item));
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            string selected = zips[RepoListBox.SelectedIndex];

            try
            {
                ToolRepository.InstallTool(selected, ToolFolderInput.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Installing Tool", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.Close();

            if((bool) AutoRestart.IsChecked)
            {
                mFrameworkDesktop.Restart();
            }
        }

        private void RepoListBox_Selected(object sender, RoutedEventArgs e)
        {
            InstallButton.IsEnabled = true;
            ToolFolderInput.IsEnabled = true;
            ToolFolderInput.Text = Path.GetFileNameWithoutExtension(zips[RepoListBox.SelectedIndex]);
        }
    }
}
