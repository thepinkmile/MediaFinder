using System.Windows;
using System.Windows.Controls;

namespace MediaFinder.Views.Export;

/// <summary>
/// Interaction logic for MediaPlayerView.xaml
/// </summary>
public partial class MediaPlayerView : UserControl
{
    public MediaPlayerView()
    {
        InitializeComponent();
    }

    // Play the media.
    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        // The Play method will begin the media if it is not currently active or
        // resume media if it is paused. This has no effect if the media is
        // already running.
        fileVideoViewer.Play();
        mediaPlayButton.IsEnabled = false;
    }

    // Pause the media.
    void PauseButton_Click(object sender, RoutedEventArgs args)
    {
        // The Pause method pauses the media if it is currently running.
        // The Play method can be used to resume.
        fileVideoViewer.Pause();
        mediaPlayButton.IsEnabled = true;
    }

    // Stop the media.
    void StopButton_Click(object sender, RoutedEventArgs args)
    {
        // The Stop method stops and resets the media to be played from
        // the beginning.
        fileVideoViewer.Stop();
        mediaPlayButton.IsEnabled = true;
    }

    // When loaded start and pause playback
    private void FileVideoViewer_Loaded(object sender, RoutedEventArgs e)
    {
        fileVideoViewer.Play();
        fileVideoViewer.Pause();
        mediaPlayButton.IsEnabled = true;
    }

    // When the media playback is finished. reset the media to the start.
    private void FileVideoViewer_MediaEnded(object sender, RoutedEventArgs e)
    {
        // Reset the position and loop forever
        fileVideoViewer.Position = TimeSpan.Zero;
    }

    public void Play()
    {
        if (fileVideoViewer.IsLoaded)
        {
            fileVideoViewer.Play();
            mediaPlayButton.IsEnabled = false;
        }
    }

    public void Stop()
    {
        if (fileVideoViewer.IsLoaded)
        {
            fileVideoViewer.Pause();
            mediaPlayButton.IsEnabled = true;
        }
    }
}
