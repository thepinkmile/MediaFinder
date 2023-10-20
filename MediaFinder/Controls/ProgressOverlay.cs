using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using CommunityToolkit.Mvvm.ComponentModel;

namespace MediaFinder_v2.Controls;

/// <summary>
/// A control for displaying some kind of progress indication over the complete user interface while a long running operation is in progress.
/// </summary>
[ObservableObject]
public partial class ProgressOverlay : ContentControl
{
    public static readonly DependencyProperty OverlayBackgroundProperty = DependencyProperty.Register(
        nameof(OverlayBackground), typeof(Brush), typeof(ProgressOverlay), new FrameworkPropertyMetadata(Panel.BackgroundProperty.DefaultMetadata.DefaultValue));

    [Bindable(true), Category("Appearance")]
    public Brush OverlayBackground
    {
        get
        {
            return (Brush)GetValue(OverlayBackgroundProperty);
        }

        set
        {
            SetValue(OverlayBackgroundProperty, value);
        }
    }

    /// <summary>
    /// A <see cref="ICommand"/> to enable cancellation.
    /// </summary>
    public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register(
        nameof(CancelCommand), typeof(ICommand), typeof(ProgressOverlay), new FrameworkPropertyMetadata(null));

    /// <summary>
    /// A <see cref="ICommand"/> to enable cancellation.
    /// </summary>
    [Bindable(true), Category("Action")]
    public ICommand? CancelCommand
    {
        get
        {
            return (ICommand?)GetValue(CancelCommandProperty);
        }

        set
        {
            SetValue(CancelCommandProperty, value);
        }
    }

    /// <summary>
    /// True, to switch the control into busy state and make it visible in the UI's foreground.
    /// </summary>
    public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
        nameof(IsBusy), typeof(bool), typeof(ProgressOverlay), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// True, to switch the control into busy state and make it visible in the UI's foreground.
    /// </summary>
    [Bindable(true), Category("Action")]
    public bool IsBusy
    {
        get
        {
            return (bool)GetValue(IsBusyProperty);
        }

        set
        {
            SetValue(IsBusyProperty, value);
        }
    }

    /// <summary>
    /// The progress in percentage of the operation causing the busy state.
    /// </summary>
    public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
        nameof(Progress), typeof(int), typeof(ProgressOverlay), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// The progress in percentage of the operation causing the busy state.
    /// </summary>
    [Bindable(true), Category("Status")]
    public int Progress
    {
        get
        {
            return (int)GetValue(ProgressProperty);
        }

        set
        {
            SetValue(ProgressProperty, value);
        }
    }

    /// <summary>
    /// The status message to be displayed instead of percentage value.
    /// </summary>
    public static readonly DependencyProperty StatusMessageProperty = DependencyProperty.Register(
        nameof(StatusMessage), typeof(string), typeof(ProgressOverlay), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// The status message to be displayed instead of percentage value.
    /// </summary>
    [Bindable(true), Category("Status")]
    public string StatusMessage
    {
        get
        {
            return (string)GetValue(StatusMessageProperty);
        }

        set
        {
            SetValue(StatusMessageProperty, value);
        }
    }

    /// <summary>
    /// Creates a new <see cref="ProgressOverlay" />.
    /// </summary>
    public ProgressOverlay() : base() { }
}
