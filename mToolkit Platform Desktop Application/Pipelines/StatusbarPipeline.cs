using mToolkitPlatformComponentLibrary.Pipelines;
using mToolkitPlatformDesktopLauncher.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace mToolkitPlatformDesktopLauncher.Pipelines
{
    /// <summary>
    /// Represents a pipeline for processing status bar messages.
    /// </summary>
    public class StatusbarPipeline : mPipeline<XElement>
    {
        /// <summary>
        /// Gets or sets the current FancyStatusbar instance.
        /// </summary>
        public static FancyStatusbar Current { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusbarPipeline"/> class.
        /// </summary>
        public StatusbarPipeline() : base("Statusbar")
        {
        }

        /// <summary>
        /// Processes an incoming message for the pipeline.
        /// </summary>
        /// <param name="message">The message to process.</param>
        protected override void AcceptMessage(mPipeMessage message)
        {
            XElement messageElement = (XElement)message.Message;

            string text = messageElement.Element("text")?.Value ?? string.Empty;
            string additional = messageElement.Element("additional")?.Value ?? string.Empty;
            string type = messageElement.Element("type")?.Value ?? string.Empty;
            string timing = messageElement.Element("timing")?.Value ?? string.Empty;

            if (!string.IsNullOrEmpty(text) && Current != null)
            {
                int timingValue = string.IsNullOrEmpty(timing) ? -1 : int.Parse(timing);
                Current.Update(text, additional, type, timingValue);
            }
        }
    }
}
