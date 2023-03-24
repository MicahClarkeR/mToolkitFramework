using mToolkitPlatformComponentLibrary.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mToolkitPlatformComponentLibrary
{
    public static class mFrameworkApplication
    {
        public static mPipelineManager Pipelines { get; private set; } = new mPipelineManager();
    }
}
