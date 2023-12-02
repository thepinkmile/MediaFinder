using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MediaFinder.Controls.Wpf.Controls;

/// <summary>
/// A control for displaying some kind of progress/status indication over the complete user interface while a long running operation is in progress.
/// </summary>
public partial class StatusOverlay : ContentControl
{
    /// <summary>
    /// Creates a new <see cref="StatusOverlay" />.
    /// </summary>
    public StatusOverlay() : base() { }

    #region OverlayBackground

    /// <summary>
    /// The <see cref="Brush"/> to use for the overlay background.
    /// </summary>
    public static readonly DependencyProperty OverlayBackgroundProperty = DependencyProperty.Register(
        nameof(OverlayBackground), typeof(Brush), typeof(ProgressOverlay), new FrameworkPropertyMetadata(Panel.BackgroundProperty.DefaultMetadata.DefaultValue));

    /// <summary>
    /// The <see cref="Brush"/> to use for the overlay background.
    /// </summary>
    [Bindable(true), Category("Appearance")]
    public Brush? OverlayBackground
    {
        get
        {
            return (Brush?)GetValue(OverlayBackgroundProperty);
        }

        set
        {
            SetValue(OverlayBackgroundProperty, value);
        }
    }

    #endregion

    #region CancelCommand

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

    #endregion

    #region IsBusy

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

    #endregion

    #region Progress

    /// <summary>
    /// The progress in percentage of the operation causing the busy state.
    /// </summary>
    public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
        nameof(Progress), typeof(int), typeof(ProgressOverlay), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// The progress in percentage of the operation causing the busy state.
    /// </summary>
    [Bindable(true), Category("Status"), Range(-1,100, MaximumIsExclusive = false, MinimumIsExclusive = false)]
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

    #endregion

    #region StatusOverlayType

    /// <summary>
    /// The status message to be displayed instead of percentage value.
    /// </summary>
    public static readonly DependencyProperty OverlayTypeProperty = DependencyProperty.Register(
        nameof(OverlayType), typeof(StatusOverlayType), typeof(ProgressOverlay), new FrameworkPropertyMetadata(StatusOverlayType.Circular, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// The status message to be displayed instead of percentage value.
    /// </summary>
    [Bindable(true), Category("Status")]
    public StatusOverlayType? OverlayType
    {
        get
        {
            return (StatusOverlayType?)GetValue(OverlayTypeProperty);
        }

        set
        {
            SetValue(OverlayTypeProperty, value);
        }
    }

    #endregion
}
