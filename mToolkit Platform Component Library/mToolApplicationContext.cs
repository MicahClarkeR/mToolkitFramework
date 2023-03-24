using System.Reflection;
using System.Runtime.Loader;

namespace mToolkitPlatformComponentLibrary
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
