using System;
using System.Diagnostics;
using System.Windows;
using AniView.Classes;

namespace AniView.Views
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();
            ChangeVisualStyle();
        }

        /// <summary>
        /// Change the visual style of the controls, depending on the settings.
        /// </summary>
        private void ChangeVisualStyle()
        {
            StyleManager.ChangeStyle(this);
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnLicense_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("gpl.pdf");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCodeDead_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("http://codedead.com/");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
