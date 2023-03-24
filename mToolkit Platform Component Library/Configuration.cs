using System.Windows;

namespace mToolkitPlatformComponentLibrary
{
    internal class Configuration
    {
        public static void Startup(Window owner)
        {
            mToolkit.LoadTools(owner);
        }
    }
}
