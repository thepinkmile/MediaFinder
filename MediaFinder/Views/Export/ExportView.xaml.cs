using System.Windows.Controls;

namespace MediaFinder.Views.Export
{
    /// <summary>
    /// Interaction logic for ViewResults.xaml
    /// </summary>
    public partial class ExportView : UserControl
    {
        public ExportView()
        {
            InitializeComponent();
        }

        private void DrawerHost_DrawerOpened(object sender, MaterialDesignThemes.Wpf.DrawerOpenedEventArgs e)
        {
            mediaPlayer.Play();
        }

        private void DrawerHost_DrawerClosing(object sender, MaterialDesignThemes.Wpf.DrawerClosingEventArgs e)
        {
            mediaPlayer.Stop();
        }
    }
}
