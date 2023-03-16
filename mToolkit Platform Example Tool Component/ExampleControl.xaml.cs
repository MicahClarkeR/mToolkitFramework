using mToolkitPlatformComponentLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mToolkitPlatformExampleToolComponent
{
    /// <summary>
    /// Interaction logic for ExampleControl.xaml
    /// </summary>
    public partial class ExampleControl : UserControl
    {
        internal ExampleTool? Owner;

        internal ExampleControl(ExampleTool owner) : base()
        {
            Owner = owner;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Label.Content = "I have changed!";
            Owner?.Log.Info("Changing label text.");
        }

        internal void Close()
        {
            Owner = null;
        }
    }
}
