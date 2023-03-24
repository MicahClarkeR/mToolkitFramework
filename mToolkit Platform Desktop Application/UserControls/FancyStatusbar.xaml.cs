using mToolkitPlatformDesktopLauncher.Pipelines;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace mToolkitPlatformDesktopLauncher.UserControls
{
    /// <summary>
    /// Represents a custom fancy status bar control.
    /// </summary>
    public partial class FancyStatusbar : UserControl
    {
        private string? _additional;
        private int _updated;

        /// <summary>
        /// Initializes a new instance of the FancyStatusbar class.
        /// </summary>
        public FancyStatusbar()
        {
            InitializeComponent();
            StatusbarPipeline.Current = this;
            StatusText.Text = string.Empty;
            _updated = DateTime.Now.Millisecond;
        }

        /// <summary>
        /// Updates the status bar text, color, and optional additional information.
        /// </summary>
        /// <param name="text">The status text to display.</param>
        /// <param name="additional">Optional additional information.</param>
        /// <param name="type">Optional status type (e.g. "error", "success").</param>
        /// <param name="timing">Optional time in milliseconds to display the status before resetting.</param>
        public void Update(string text, string? additional, string? type, int? timing = -1)
        {
            StatusText.Text = text;
            _additional = additional;
            SetForegroundColor(type);

            int now = DateTime.Now.Millisecond;
            _updated = now;

            if (timing != -1 && timing != null)
            {
                ResetStatusTextAfterDelay(now, timing.Value);
            }
        }

        private void SetForegroundColor(string? type)
        {
            StatusText.Foreground = Brushes.Black;

            switch (type?.ToLower())
            {
                case "error":
                    StatusText.Foreground = Brushes.Red;
                    break;
                case "success":
                    StatusText.Foreground = Brushes.Green;
                    break;
            }
        }

        private async void ResetStatusTextAfterDelay(int now, int delay)
        {
            await Task.Delay(delay);

            if (_updated == now)
            {
                StatusText.Dispatcher.Invoke(() => StatusText.Text = "");
            }
        }

        /// <summary>
        /// Handles the MouseDoubleClick event on the status bar.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The MouseButtonEventArgs for the event.</param>
        private void StatusBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_additional != null)
            {
                MessageBox.Show(_additional);
            }
        }
    }
}
