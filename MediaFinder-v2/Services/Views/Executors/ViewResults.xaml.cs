using System.Windows;
using System.Windows.Controls;

namespace MediaFinder_v2.Views.Executors
{
    /// <summary>
    /// Interaction logic for ViewResults.xaml
    /// </summary>
    public partial class ViewResults : UserControl
    {
        public ViewResults()
        {
            InitializeComponent();
        }

        #region Media Element Controls

        // Play the media.
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            // The Play method will begin the media if it is not currently active or
            // resume media if it is paused. This has no effect if the media is
            // already running.
            fileVideoViewer.Play();
        }

        // Pause the media.
        void PauseButton_Click(object sender, RoutedEventArgs args)
        {
            // The Pause method pauses the media if it is currently running.
            // The Play method can be used to resume.
            fileVideoViewer.Pause();
        }

        // Stop the media.
        void StopButton_Click(object sender, RoutedEventArgs args)
        {
            // The Stop method stops and resets the media to be played from
            // the beginning.
            fileVideoViewer.Stop();
        }

        // When loaded start and pause playback
        private void FileVideoViewer_Loaded(object sender, RoutedEventArgs e)
        {
            fileVideoViewer.Play();
            fileVideoViewer.Pause();
        }

        // When the media playback is finished. reset the media to the start.
        private void FileVideoViewer_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Reset the position and loop forever
            fileVideoViewer.Position = TimeSpan.Zero;
        }
        private void DrawerHost_DrawerOpened(object sender, MaterialDesignThemes.Wpf.DrawerOpenedEventArgs e)
        {
            if (fileVideoViewer.IsLoaded)
            {
                fileVideoViewer.Play();
            }
        }

        private void DrawerHost_DrawerClosing(object sender, MaterialDesignThemes.Wpf.DrawerClosingEventArgs e)
        {
            fileVideoViewer.Pause();
        }

        #endregion

    }
}
