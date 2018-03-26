using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using AniView.Classes;
using XamlAnimatedGif;
using Application = System.Windows.Application;
using DataFormats = System.Windows.DataFormats;
using DownloadProgressEventArgs = XamlAnimatedGif.DownloadProgressEventArgs;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Path = System.IO.Path;

namespace AniView.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Local Variables
        /// <summary>
        /// A list of obtained paths that lead to displayable images
        /// </summary>
        private List<string> _images = new List<string>();
        /// <summary>
        /// The index of the currently selected image in the image list
        /// </summary>
        private int _current;
        /// <summary>
        /// The animation behaviour for the XamlAnimatedGif control
        /// </summary>
        private Animator _animator;
        /// <summary>
        /// A boolean to indicate whether or not the image playlist should be repeated indefinitely
        /// </summary>
        private bool _repeatForever;
        /// <summary>
        /// A boolean to indicate whether the operation has been completed
        /// </summary>
        private bool _completed;
        /// <summary>
        /// The repeat behaviour for the XamlAnimatedGif control
        /// </summary>
        private RepeatBehavior _repeatBehavior;
        /// <summary>
        /// A boolean to indicate whether a download is in progress or not
        /// </summary>
        private bool _isDownloading;
        /// <summary>
        /// An integer to indicate the current download progress
        /// </summary>
        private int _downloadProgress;
        /// <summary>
        /// A boolean to indicate whether the download progress is indeterminate or not
        /// </summary>
        private bool _isDownloadProgressIndeterminate;
        /// <summary>
        /// The path of the image that is currently being displayed
        /// </summary>
        private string _currentPath = "";
        /// <summary>
        /// The UpdateManager that will indicate whether an application update is available or not
        /// </summary>
        private readonly UpdateManager.Classes.UpdateManager _updateManager;
        /// <summary>
        /// The Property changed event handler for the XamlAnimatedGif control
        /// </summary>
        internal event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Setting Variables
        /// <summary>
        /// A boolean to indicate whether the file path should be displayed in the title of the window
        /// </summary>
        private bool _showFileTitle;
        /// <summary>
        /// A boolean to indicate whether or not navigating AniView with arrow keys is enabled
        /// </summary>
        private bool _arrowKeysEnabled;
        /// <summary>
        /// A boolean to indicate whether the window should automatically resize itself
        /// </summary>
        private bool _autoSizeWindow;
        /// <summary>
        /// A boolean to indicate whether the animation behaviour should start automatically
        /// </summary>
        private bool _autoStartAnimation;
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Initialize a new MainWindow object
        /// </summary>
        public MainWindow()
        {
            _updateManager = new UpdateManager.Classes.UpdateManager(Assembly.GetExecutingAssembly().GetName().Version, "https://codedead.com/Software/AniView/update.xml", "AniView", "Information", "Cancel", "Download", "No new version is currently available.");

            InitializeComponent();
            ChangeVisualStyle();
            LoadAnimationBehaviour();

            LoadArguments();
            AutoUpdate();
            LoadSettings();
        }

        #region Properties
        /// <summary>
        /// A property to indicate whether the image playlist should be repeated indefinitely
        /// </summary>
        internal bool RepeatForever
        {
            get => _repeatForever;
            set
            {
                _repeatForever = value;
                OnPropertyChanged("RepeatForever");
                if (value)
                {
                    RepeatBehavior = RepeatBehavior.Forever;
                }
            }
        }

        /// <summary>
        /// A property to indicate whether the operation has completed or not
        /// </summary>
        internal bool Completed
        {
            get => _completed;
            set
            {
                _completed = value;
                OnPropertyChanged("Completed");
            }
        }

        /// <summary>
        /// A property that indicates the current repeat behaviour
        /// </summary>
        internal RepeatBehavior RepeatBehavior
        {
            get => _repeatBehavior;
            set
            {
                _repeatBehavior = value;
                OnPropertyChanged("RepeatBehavior");
                Completed = false;
            }
        }

        /// <summary>
        /// A property to indicate whether a download operation is currently being executed or not
        /// </summary>
        internal bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                _isDownloading = value;
                OnPropertyChanged("IsDownloading");
            }
        }

        /// <summary>
        /// A property to indicate the current download progress
        /// </summary>
        internal int DownloadProgress
        {
            get => _downloadProgress;
            set
            {
                _downloadProgress = value;
                OnPropertyChanged("DownloadProgress");
            }
        }

        /// <summary>
        /// A property to indicate whether the download progress is indeterminate or not
        /// </summary>
        internal bool IsDownloadProgressIndeterminate
        {
            get => _isDownloadProgressIndeterminate;
            set
            {
                _isDownloadProgressIndeterminate = value;
                OnPropertyChanged("IsDownloadProgressIndeterminate");
            }
        }

        /// <summary>
        /// This method will be called when a property has changed
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        /// <summary>
        /// Load the animation behaviour for XamlAnimatedGif
        /// </summary>
        internal void LoadAnimationBehaviour()
        {
            try
            {
                AnimationBehavior.SetRepeatBehavior(ImgView, Properties.Settings.Default.RepeatBehaviour == 0 ? RepeatBehavior.Forever : new RepeatBehavior(Properties.Settings.Default.RepeatBehaviour));
                _autoStartAnimation = Properties.Settings.Default.AutoStart;
                AnimationBehavior.SetAutoStart(ImgView, _autoStartAnimation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Automatically check for updates
        /// </summary>
        private void AutoUpdate()
        {
            try
            {
                if (Properties.Settings.Default.AutoUpdate)
                {
                    _updateManager.CheckForUpdate(false, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Load all relevant settings
        /// </summary>
        internal void LoadSettings()
        {
            try
            {
                BtnFullScreen.IsChecked = Properties.Settings.Default.FullScreen;
                GridMain.AllowDrop = Properties.Settings.Default.DragDrop;
                _arrowKeysEnabled = Properties.Settings.Default.ArrowKeys;
                _autoSizeWindow = Properties.Settings.Default.AutoSizeWindow;
                _showFileTitle = Properties.Settings.Default.ShowFileTitle;

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

                StbInfo.Visibility = Properties.Settings.Default.StatusBar ? Visibility.Visible : Visibility.Collapsed;
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
        /// Change the visual style of an object, depending on the settings.
        /// </summary>
        internal void ChangeVisualStyle()
        {
            StyleManager.ChangeStyle(this);
        }

        /// <summary>
        /// Load startup arguments in order to load the image into the GUI
        /// </summary>
        private void LoadArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length <= 1) return;
            if (!File.Exists(args[1])) return;

            LoadImage(args[1]);
        }

        /// <summary>
        /// Load the path of all GIF images into a list and determine the position of the current image in the list
        /// </summary>
        private void LoadImage(string path)
        {
            if (path == null) return;
            if (!File.Exists(path)) return;

            PgbLoading.Visibility = Visibility.Visible;
            ImgView.Visibility = Visibility.Collapsed;
            SldFrame.Value = 0;
            SldFrame.Maximum = 100;

            try
            {
                BitmapImage bitmap = new BitmapImage();
                using (FileStream stream = File.OpenRead(path))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show(ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AnimationBehavior.SetSourceUri(ImgView, new Uri(path));
            _images = new List<string>();
            ImgPause.Source = _autoStartAnimation ? new BitmapImage(new Uri("/AniView;component/Resources/Images/pause.png", UriKind.Relative)) : new BitmapImage(new Uri("/AniView;component/Resources/Images/replay.png", UriKind.Relative));
            if (_autoSizeWindow)
            {
                SizeToContent = SizeToContent.WidthAndHeight;
            }

            string pathName = Path.GetDirectoryName(path);
            if (pathName != null)
            {
                foreach (string s in Directory.GetFiles(pathName, "*.gif", SearchOption.TopDirectoryOnly))
                {
                    if (!_images.Contains(s))
                    {
                        _images.Add(s);
                    }
                }
            }
            for (int i = 0; i < _images.Count; i++)
            {
                if (_images[i] == path)
                {
                    _current = i;
                }
            }
            _currentPath = path;

            Title = "AniView";
            if (_showFileTitle)
            {
                Title += " - " + _currentPath;
            }

            LblSize.Content = (new FileInfo(path).Length / 1024f / 1024f).ToString("F2") + " MB";
        }

        /// <summary>
        /// Unload the current image and reset all variables to their default values
        /// </summary>
        private void UnloadImage()
        {
            AnimationBehavior.SetSourceUri(ImgView, null);

            ImgView.Visibility = Visibility.Collapsed;
            PgbLoading.Visibility = Visibility.Collapsed;

            _animator = null;
            _currentPath = "";
            _current = 0;
            _images = new List<string>();
            Title = "AniView";
            LblSize.Content = "";
            LblDimensions.Content = "";
            SldFrame.Value = 0;
        }

        /// <summary>
        /// Open the next image
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnRight_Click(object sender, RoutedEventArgs e)
        {
            MoveRight();
        }

        /// <summary>
        /// Load the image that is located to the left of the current image position
        /// </summary>
        private void MoveRight()
        {
            if (_images.Count == 0) return;
            _current++;
            if (_current > _images.Count - 1)
            {
                _current = 0;
            }
            LoadImage(_images[_current]);
        }

        /// <summary>
        /// Load the image that is located to the right of the current image position
        /// </summary>
        private void MoveLeft()
        {
            if (_images.Count == 0) return;
            _current--;
            if (_current < 0)
            {
                _current = _images.Count - 1;
            }
            LoadImage(_images[_current]);
        }

        /// <summary>
        /// Open the previous image
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnLeft_Click(object sender, RoutedEventArgs e)
        {
            MoveLeft();
        }

        /// <summary>
        /// Pause the current image
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (_animator == null) return;
            if (_animator.IsPaused)
            {
                _animator.Play();
                ImgPause.Source = new BitmapImage(new Uri("/AniView;component/Resources/Images/pause.png", UriKind.Relative));
            }
            else
            {
                _animator.Pause();
                ImgPause.Source = new BitmapImage(new Uri("/AniView;component/Resources/Images/replay.png", UriKind.Relative));
            }
        }

        /// <summary>
        /// This method will be called when the current frame of the image has changed
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The event arguments</param>
        private void CurrentFrameChanged(object sender, EventArgs e)
        {
            if (_animator != null && PgbLoading.Visibility == Visibility.Collapsed)
            {
                SldFrame.Value = _animator.CurrentFrameIndex;
            }
        }

        /// <summary>
        /// This method will be called when the animation of the image has completed
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The event arguments</param>
        private void AnimationCompleted(object sender, EventArgs e)
        {
            if (!RepeatForever)
            {
                _animator.Pause();
                ImgPause.Source = new BitmapImage(new Uri("/AniView;component/Resources/Images/replay.png", UriKind.Relative));
            }
            Completed = true;
        }

        /// <summary>
        /// This method will be called when the progress of a download operation has changed
        /// </summary>
        /// <param name="d">The dependency object</param>
        /// <param name="e">The download progress event arguments</param>
        private void AnimationBehavior_OnDownloadProgress(DependencyObject d, DownloadProgressEventArgs e)
        {
            IsDownloading = true;
            if (e.Progress >= 0)
            {
                DownloadProgress = e.Progress;
                IsDownloadProgressIndeterminate = false;
            }
            else
            {
                IsDownloadProgressIndeterminate = true;
            }
        }

        /// <summary>
        /// This method will be called when the animation behaviour has loaded
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void AnimationBehavior_OnLoaded(object sender, RoutedEventArgs e)
        {
            PgbLoading.Visibility = Visibility.Collapsed;
            ImgView.Visibility = Visibility.Visible;

            if (_animator != null)
            {
                _animator.CurrentFrameChanged -= CurrentFrameChanged;
                _animator.AnimationCompleted -= AnimationCompleted;
            }

            _animator = AnimationBehavior.GetAnimator(ImgView);

            if (_animator == null) return;
            _animator.CurrentFrameChanged += CurrentFrameChanged;
            _animator.AnimationCompleted += AnimationCompleted;
            SldFrame.Value = 0;
            SldFrame.Maximum = _animator.FrameCount - 1;
            LblDimensions.Content = ImgView.Source.Width + " x " + ImgView.Source.Height;
        }

        /// <summary>
        /// This method will be called when an error occurs within the animation behaviour
        /// </summary>
        /// <param name="d">The dependency object</param>
        /// <param name="e">The animation error event arguments</param>
        private void AnimationBehavior_OnError(DependencyObject d, AnimationErrorEventArgs e)
        {
            PgbLoading.Visibility = Visibility.Collapsed;
            ImgView.Visibility = Visibility.Collapsed;

            if (e.Kind == AnimationErrorKind.Loading)
            {
                IsDownloading = false;
            }

            MessageBox.Show($"An error occurred ({e.Kind}): {e.Exception}", "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Open an image
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "GIF Images (*.gif)|*.gif" };

            if (ofd.ShowDialog(this) == true)
            {
                LoadImage(ofd.FileName);
            }
        }

        /// <summary>
        /// Exit the application
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        /// <summary>
        /// Edit an image using the default image editor
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(_currentPath)) return;
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(_currentPath) { Verb = "edit" };
                Process.Start(startInfo);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "AniView", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Open a new SettingsWindow
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(this).ShowDialog();
        }

        /// <summary>
        /// Open the help documentation, if applicable
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + "\\help.pdf");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Check for application updates
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            _updateManager.CheckForUpdate(true, true);
        }

        /// <summary>
        /// Open the CodeDead website
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnCodeDead_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://codedead.com/");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Open the file containing the license for AniView
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnLicense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + "\\gpl.pdf");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Open a new AboutWindow
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

        /// <summary>
        /// Export an image and all of its frames
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private async void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPath.Length == 0) return;
            if (!File.Exists(_currentPath)) return;

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            try
            {
                await ImageExtractor.ExtractFrames(_currentPath, fbd.SelectedPath, Properties.Settings.Default.ImageFormat);
                MessageBox.Show("All frames have been extracted!", "AniView", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Open the properties of a file using the default file properties window
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnProperties_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentPath.Length == 0) return;
            if (!File.Exists(_currentPath)) return;
            try
            {
                NativeMethods.ShowFileProperties(_currentPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Enable or disable the stretching of an image
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnFullScreen_OnClick(object sender, RoutedEventArgs e)
        {
            ImgView.Stretch = BtnFullScreen.IsChecked ? Stretch.Fill : Stretch.None;
        }

        /// <summary>
        /// Allow drag and drop of an image on the MainWindow
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The drag event arguments</param>
        private void GridMain_OnDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null) return;
            if (!File.Exists(files[0])) return;

            LoadImage(files[0]);
        }

        /// <summary>
        /// Open the previous or next image if arrow key navigation is enabled
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The key event arguments</param>
        private void GridMain_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!_arrowKeysEnabled) return;
            if (e.Key == Key.Left)
            {
                MoveLeft();
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                MoveRight();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Method that will be called when the GridMain object has loaded
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void GridMain_OnLoaded(object sender, RoutedEventArgs e)
        {
            GridMain.Focus();
        }

        /// <summary>
        /// Close an image
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            UnloadImage();
        }

        /// <summary>
        /// Open the donation page
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnDonate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("https://codedead.com/?page_id=302");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
