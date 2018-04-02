using System;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AniView.Classes;

namespace AniView.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        #region Variables
        /// <summary>
        /// The main window
        /// </summary>
        private readonly MainWindow _mainWindow;
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Initialize a new SettingsWindow object
        /// </summary>
        /// <param name="mw">The main window of AniView</param>
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
                ChbAutoStartAnimation.IsChecked = Properties.Settings.Default.AutoStart;
                ChbAutoSizeWindow.IsChecked = Properties.Settings.Default.AutoSizeWindow;
                ChbFullScreen.IsChecked = Properties.Settings.Default.FullScreen;
                ChbDragDrop.IsChecked = Properties.Settings.Default.DragDrop;
                ChbArrowKeys.IsChecked = Properties.Settings.Default.ArrowKeys;
                ChbFileTitle.IsChecked = Properties.Settings.Default.ShowFileTitle;
                ChbStatusBar.IsChecked = Properties.Settings.Default.StatusBar;

                if (Properties.Settings.Default.Topmost)
                {
                    Topmost = true;
                    ChbTopMost.IsChecked = true;
                }
                else
                {
                    Topmost = false;
                    ChbTopMost.IsChecked = false;
                }

                if (Properties.Settings.Default.RepeatBehaviour > 3)
                {
                    CboRepeat.SelectedIndex = 4;
                    TxtCustomRepeat.Value = Properties.Settings.Default.RepeatBehaviour;
                }
                else
                {
                    CboRepeat.SelectedIndex = Properties.Settings.Default.RepeatBehaviour;
                }

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
                else if (Properties.Settings.Default.ImageFormat.Equals(ImageFormat.Gif))
                {
                    CboFormat.SelectedIndex = 4;
                }

                if (Properties.Settings.Default.WindowDragging)
                {
                    ChbWindowDragging.IsChecked = true;
                    // Prevent duplicate handlers
                    MouseDown -= OnMouseDown;
                    MouseDown += OnMouseDown;
                }
                else
                {
                    ChbWindowDragging.IsChecked = false;
                    MouseDown -= OnMouseDown;
                }

                CboStyle.SelectedValue = Properties.Settings.Default.VisualStyle;
                CpMetroBrush.Color = Properties.Settings.Default.MetroColor;
                IntBorderThickness.Value = Properties.Settings.Default.BorderThickness;
                SldOpacity.Value = Properties.Settings.Default.WindowOpacity * 100;
                SldWindowResize.Value = Properties.Settings.Default.WindowResizeBorder;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the Window should be dragged
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The MouseButtonEventArgs</param>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        /// <summary>
        /// Reset all settings
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.Save();

                LoadSettings();

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

        /// <summary>
        /// Save the currently selected settings
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ChbAutoUpdate.IsChecked != null) Properties.Settings.Default.AutoUpdate = ChbAutoUpdate.IsChecked.Value;
                if (ChbAutoStartAnimation.IsChecked != null) Properties.Settings.Default.AutoStart = ChbAutoStartAnimation.IsChecked.Value;
                if (ChbAutoSizeWindow.IsChecked != null) Properties.Settings.Default.AutoSizeWindow = ChbAutoSizeWindow.IsChecked.Value;
                if (ChbFullScreen.IsChecked != null) Properties.Settings.Default.FullScreen = ChbFullScreen.IsChecked.Value;
                if (ChbDragDrop.IsChecked != null) Properties.Settings.Default.DragDrop = ChbDragDrop.IsChecked.Value;
                if (ChbArrowKeys.IsChecked != null) Properties.Settings.Default.ArrowKeys = ChbArrowKeys.IsChecked.Value;
                if (ChbFileTitle.IsChecked != null) Properties.Settings.Default.ShowFileTitle = ChbFileTitle.IsChecked.Value;
                if (ChbWindowDragging.IsChecked != null) Properties.Settings.Default.WindowDragging = ChbWindowDragging.IsChecked.Value;
                if (ChbStatusBar.IsChecked != null) Properties.Settings.Default.StatusBar = ChbStatusBar.IsChecked.Value;
                if (ChbTopMost.IsChecked != null) Properties.Settings.Default.Topmost = ChbTopMost.IsChecked.Value;

                if (CboRepeat.SelectedIndex == 4)
                {
                    if (TxtCustomRepeat.Value != null) Properties.Settings.Default.RepeatBehaviour = (int) TxtCustomRepeat.Value;
                }
                else
                {
                    Properties.Settings.Default.RepeatBehaviour = CboRepeat.SelectedIndex;
                }

                switch (CboFormat.SelectedIndex)
                {
                    default:
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
                    case 4:
                        Properties.Settings.Default.ImageFormat = ImageFormat.Gif;
                        break;
                }

                Properties.Settings.Default.VisualStyle = CboStyle.Text;

                Properties.Settings.Default.MetroColor = CpMetroBrush.Color;
                if (IntBorderThickness.Value != null) Properties.Settings.Default.BorderThickness = (int)IntBorderThickness.Value;
                Properties.Settings.Default.WindowOpacity = SldOpacity.Value / 100;
                Properties.Settings.Default.WindowResizeBorder = SldWindowResize.Value;

                Properties.Settings.Default.Save();

                _mainWindow.LoadAnimationBehaviour();
                _mainWindow.ChangeVisualStyle();
                _mainWindow.LoadSettings();

                ChangeVisualStyle();
                LoadSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// This method will be called when the selection of CboRepeat has changed
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The selection changed event arguments</param>
        private void CboRepeat_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TxtCustomRepeat.IsEnabled = ((ComboBox) sender).SelectedIndex == 4;
        }

        /// <summary>
        /// Method that is called when the opacity of the window should change dynamically
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedPropertyChangedEventArgs</param>
        private void SldOpacity_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Opacity = SldOpacity.Value / 100;
        }

        /// <summary>
        /// Method that is called when the ResizeBorderThickness of the window should change dynamically
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedPropertyChangedEventArgs</param>
        private void SldWindowResize_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ResizeBorderThickness = new Thickness(SldWindowResize.Value);
        }
    }
}
