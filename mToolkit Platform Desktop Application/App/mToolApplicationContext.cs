using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace mToolkitPlatformDesktopLauncher.App
{
    public class mToolApplicationContext : AssemblyLoadContext
    {
        public mToolApplicationContext() : base(isCollectible: true)
        {
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }
    }
}
