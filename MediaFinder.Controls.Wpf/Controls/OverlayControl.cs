using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MediaFinder.Controls.Wpf.Controls;

public partial class OverlayControl : ContentControl
{
    /// <summary>
    /// Creates a new <see cref="OverlayControl" />.
    /// </summary>
    public OverlayControl() : base() { }

    static OverlayControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(OverlayControl), new FrameworkPropertyMetadata(typeof(OverlayControl)));
    }

    #region OverlayBackground

    /// <summary>
    /// The <see cref="Brush"/> to use for the overlay background.
    /// </summary>
    public static readonly DependencyProperty OverlayBackgroundProperty = DependencyProperty.Register(
        nameof(OverlayBackground), typeof(Brush), typeof(OverlayControl), new FrameworkPropertyMetadata(Panel.BackgroundProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.Inherits));

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

    #region IsBusy

    /// <summary>
    /// True, to switch the control into busy state and make it visible in the UI's foreground.
    /// </summary>
    public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
        nameof(IsBusy), typeof(bool), typeof(OverlayControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

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
}
