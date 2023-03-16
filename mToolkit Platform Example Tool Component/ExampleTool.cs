using mToolkitPlatformComponentLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace mToolkitPlatformExampleToolComponent
{
    internal class ExampleTool : mTool
    {
        private ExampleControl? UI = null;

        public ExampleTool(string guid, string directory) : base(guid, directory)
        {
            
        }

        public override UserControl CreateUI()
        {
            UI ??= new ExampleControl(this);
            return UI;
        }

        public override void Initialise()
        {
        }

        protected override ToolInfo GetInfo()
        {
            return new ToolInfo("Example Tool",
                                "mToolkitPlatformExampleToolComponent",
                                "Micah", "1.0", "This is an example Tool.");
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            UI?.Close();
            UI = null;
        }

        protected override Type GetToolType()
        {
            return typeof(ExampleTool);
        }
    }
}
