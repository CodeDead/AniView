using System;
using System.Drawing.Imaging;
using System.Windows;
using AniView.Classes;

namespace AniView.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        #region Variables
        private readonly MainWindow _mainWindow;
        #endregion

        public SettingsWindow(MainWindow mw)
        {
            _mainWindow = mw;
            InitializeComponent();
            LoadSettings();

            ChangeVisualStyle();
        }

        /// <summary>
        /// Change the visual style of the controls, depending on the settings.
        /// </summary>
        private void ChangeVisualStyle()
        {
            StyleManager.ChangeStyle(this);
        }

        /// <summary>
        /// Load the settings
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                ChbAutoUpdate.IsChecked = Properties.Settings.Default.AutoUpdate;
                ChbFullScreen.IsChecked = Properties.Settings.Default.FullScreen;
                ChbDragDrop.IsChecked = Properties.Settings.Default.DragDrop;
                ChbArrowKeys.IsChecked = Properties.Settings.Default.ArrowKeys;
                ChbAutoStartAnimation.IsChecked = Properties.Settings.Default.AutoStart;
                CboRepeat.SelectedIndex = Properties.Settings.Default.RepeatBehaviour;

                if (Properties.Settings.Default.ImageFormat.Equals(ImageFormat.Png))
                {
                    CboFormat.SelectedIndex = 0;
                }
                else if (Properties.Settings.Default.ImageFormat.Equals(ImageFormat.Bmp))
                {
                    CboFormat.SelectedIndex = 1;
                }
                else if (Properties.Settings.Default.ImageFormat.Equals(ImageFormat.Jpeg))
                {
                    CboFormat.SelectedIndex = 2;
                }
                else if (Properties.Settings.Default.ImageFormat.Equals(ImageFormat.Tiff))
                {
                    CboFormat.SelectedIndex = 3;
                }

                CboStyle.SelectedValue = Properties.Settings.Default.VisualStyle;
                CpMetroBrush.Color = Properties.Settings.Default.MetroColor;
                IntBorderThickness.Value = Properties.Settings.Default.BorderThickness;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure that you want to reset all settings?", "AniView", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.Save();

                LoadSettings();

                _mainWindow.LoadSettings();
                _mainWindow.ChangeVisualStyle();
                ChangeVisualStyle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ChbAutoUpdate.IsChecked != null) Properties.Settings.Default.AutoUpdate = ChbAutoUpdate.IsChecked.Value;
                if (ChbFullScreen.IsChecked != null) Properties.Settings.Default.FullScreen = ChbFullScreen.IsChecked.Value;
                if (ChbDragDrop.IsChecked != null) Properties.Settings.Default.DragDrop = ChbDragDrop.IsChecked.Value;
                if (ChbArrowKeys.IsChecked != null) Properties.Settings.Default.ArrowKeys = ChbArrowKeys.IsChecked.Value;
                if (ChbAutoStartAnimation.IsChecked != null) Properties.Settings.Default.AutoStart = ChbAutoStartAnimation.IsChecked.Value;
                Properties.Settings.Default.RepeatBehaviour = CboRepeat.SelectedIndex;

                switch (CboFormat.SelectedIndex)
                {
                    case 0:
                        Properties.Settings.Default.ImageFormat = ImageFormat.Png;
                        break;
                    case 1:
                        Properties.Settings.Default.ImageFormat = ImageFormat.Bmp;
                        break;
                    case 2:
                        Properties.Settings.Default.ImageFormat = ImageFormat.Jpeg;
                        break;
                    case 3:
                        Properties.Settings.Default.ImageFormat = ImageFormat.Tiff;
                        break;
                }

                Properties.Settings.Default.VisualStyle = CboStyle.Text;

                Properties.Settings.Default.MetroColor = CpMetroBrush.Color;
                if (IntBorderThickness.Value != null) Properties.Settings.Default.BorderThickness = (int)IntBorderThickness.Value;

                Properties.Settings.Default.Save();

                _mainWindow.LoadAnimationBehaviour();
                _mainWindow.ChangeVisualStyle();
                _mainWindow.LoadSettings();
                ChangeVisualStyle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
