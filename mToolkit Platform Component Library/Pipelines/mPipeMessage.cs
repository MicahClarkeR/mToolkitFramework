using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mToolkitPlatformComponentLibrary.Pipelines
{
    /// <summary>
    /// Represents a message that can be passed through a pipeline.
    /// </summary>
    public class mPipeMessage
    {
        /// <summary>
        /// The message object.
        /// </summary>
        public object Message { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="mPipeMessage"/> class.
        /// </summary>
        /// <param name="message">The message object.</param>
        public mPipeMessage(object message)
        {
            Message = message;
        }
    }
}
