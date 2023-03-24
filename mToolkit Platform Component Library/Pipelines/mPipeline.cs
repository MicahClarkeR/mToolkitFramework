using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace mToolkitPlatformComponentLibrary.Pipelines
{
    /// <summary>
    /// Represents a generic pipeline that processes messages of type T.
    /// </summary>
    /// <typeparam name="T">The type of messages this pipeline processes.</typeparam>
    public abstract class mPipeline<T> : mPipelineInterface
    {
        /// <summary>
        /// The name of this pipeline.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="mPipeline{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the pipeline.</param>
        public mPipeline(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Sends a message to this pipeline.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void SendMessage(mPipeMessage message)
        {
            AcceptMessage(message);
        }

        /// <summary>
        /// Accepts a message of type <see cref="mPipeMessage"/> for processing.
        /// </summary>
        /// <param name="message">The message to accept.</param>
        protected abstract void AcceptMessage(mPipeMessage message);

        Type mPipelineInterface.Accepts()
        {
            return typeof(T);
        }
    }

    /// <summary>
    /// Defines the interface for a pipeline that can process messages of a particular type.
    /// </summary>
    public interface mPipelineInterface
    {
        /// <summary>
        /// Gets the type of messages this pipeline can process.
        /// </summary>
        /// <returns>The type of messages this pipeline can process.</returns>
        Type Accepts();

        /// <summary>
        /// Sends a message to this pipeline.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void SendMessage(mPipeMessage message);
    }
}
