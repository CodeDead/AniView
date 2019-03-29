using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AniView.Classes;
using Syncfusion.Windows.Shared;

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
                    // Prevent duplicate handlers
                    MouseDown -= OnMouseDown;
                    MouseDown += OnMouseDown;
                }
                else
                {
                    MouseDown -= OnMouseDown;
                }
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
        /// Method that is called when the opacity of the window should change dynamically
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedPropertyChangedEventArgs</param>
        private void SldOpacity_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Opacity = ((Slider)sender).Value / 100;
        }

        /// <summary>
        /// Method that is called when the ResizeBorderThickness of the window should change dynamically
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedPropertyChangedEventArgs</param>
        private void SldWindowResize_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ResizeBorderThickness = new Thickness(((Slider)sender).Value);
        }

        /// <summary>
        /// Method that is called when the BorderThickness of the window should change dynamically
        /// </summary>
        /// <param name="d">The DependencyObject</param>
        /// <param name="e">The DependencyPropertyChangedEventArgs</param>
        private void BorderThickness_OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            long? value = ((IntegerTextBox) d).Value;
            if (value != null) BorderThickness = new Thickness(value.Value);
        }

        /// <summary>
        /// Method that is called when the Window is closing
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The CancelEventArgs</param>
        private void SettingsWindow_OnClosing(object sender, CancelEventArgs e)
        {
            try
            {
                Properties.Settings.Default.Reload();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
