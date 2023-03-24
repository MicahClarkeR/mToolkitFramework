namespace mToolkitPlatformComponentLibrary.Pipelines
{
    /// <summary>
    /// Manages a collection of pipelines and their messages.
    /// </summary>
    public sealed class mPipelineManager
    {
        /// <summary>
        /// A dictionary of registered pipelines.
        /// </summary>
        private Dictionary<string, mPipelineInterface> Pipelines = new Dictionary<string, mPipelineInterface>();

        /// <summary>
        /// Registers a pipeline with the specified name.
        /// </summary>
        /// <param name="name">The name of the pipeline.</param>
        /// <param name="pipeline">The pipeline to register.</param>
        public void RegisterPipeline(string name, mPipelineInterface pipeline)
        {
            name = name.ToLower();

            // If the pipeline with the given name doesn't exist, add it to the dictionary
            if (!Pipelines.ContainsKey(name))
                Pipelines.Add(name, pipeline);
        }

        /// <summary>
        /// Sends a message to the pipeline with the specified name.
        /// </summary>
        /// <typeparam name="T">The type of message to send.</typeparam>
        /// <param name="name">The name of the pipeline to send the message to.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>true if the message was sent successfully; otherwise, false.</returns>
        public bool SendMessage<T>(string name, mPipeMessage message)
        {
            name = name.ToLower();

            // If the pipeline with the given name doesn't exist, return false
            if (!Pipelines.ContainsKey(name))
                return false;

            // Get the pipeline with the given name
            mPipelineInterface suspect = Pipelines[name];

            // If the pipeline doesn't accept messages of type T, return false
            if (suspect.Accepts() != typeof(T))
                return false;

            // Send the message to the pipeline
            suspect.SendMessage(message);

            return true;
        }
    }
}
