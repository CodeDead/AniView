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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Local Variables
        private List<string> _images = new List<string>();
        private int _current;
        private Animator _animator;
        private bool _repeatForever;
        private bool _completed;
        private RepeatBehavior _repeatBehavior;
        private bool _isDownloading;
        private int _downloadProgress;
        private bool _isDownloadProgressIndeterminate;
        private string _currentPath = "";
        private readonly UpdateManager.UpdateManager _updateManager;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Setting Variables
        private bool _showFileTitle;
        private bool _arrowKeysEnabled;
        private bool _autoSizeWindow;
        private bool _autoStartAnimation;
        #endregion

        public MainWindow()
        {
            _updateManager = new UpdateManager.UpdateManager(Assembly.GetExecutingAssembly().GetName().Version, "https://codedead.com/Software/AniView/update.xml", "AniView");

            InitializeComponent();
            ChangeVisualStyle();
            LoadAnimationBehaviour();

            LoadArguments();
            AutoUpdate();
            LoadSettings();
        }

        #region Properties
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

        internal bool Completed
        {
            get => _completed;
            set
            {
                _completed = value;
                OnPropertyChanged("Completed");
            }
        }

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

        internal bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                _isDownloading = value;
                OnPropertyChanged("IsDownloading");
            }
        }

        internal int DownloadProgress
        {
            get => _downloadProgress;
            set
            {
                _downloadProgress = value;
                OnPropertyChanged("DownloadProgress");
            }
        }

        internal bool IsDownloadProgressIndeterminate
        {
            get => _isDownloadProgressIndeterminate;
            set
            {
                _isDownloadProgressIndeterminate = value;
                OnPropertyChanged("IsDownloadProgressIndeterminate");
            }
        }

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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Change the visual style of the controls, depending on the settings.
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
        /// Load the path of all GIF images into a list and determine the current position
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
            ImgPause.Source = _autoStartAnimation ? new BitmapImage(new Uri("/AniView;component/Resources/Images/pin.png", UriKind.Relative)) : new BitmapImage(new Uri("/AniView;component/Resources/Images/replay.png", UriKind.Relative));
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

            LblSize.Content = new FileInfo(path).Length + " bytes";
        }

        /// <summary>
        /// Unload the current image and reset all variables to their default values
        /// </summary>
        private void UnloadImage()
        {
            AnimationBehavior.SetSourceUri(ImgView, null);
            ImgView.Visibility = Visibility.Collapsed;
            _animator = null;
            _currentPath = "";
            _current = 0;
            _images = new List<string>();
            Title = "AniView";
            LblSize.Content = "";
            LblDimensions.Content = "";
            SldFrame.Value = 0;
        }

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

        private void BtnLeft_Click(object sender, RoutedEventArgs e)
        {
            MoveLeft();
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (_animator == null) return;
            if (_animator.IsPaused)
            {
                _animator.Play();
                ImgPause.Source = new BitmapImage(new Uri("/AniView;component/Resources/Images/pin.png", UriKind.Relative));
            }
            else
            {
                _animator.Pause();
                ImgPause.Source = new BitmapImage(new Uri("/AniView;component/Resources/Images/replay.png", UriKind.Relative));
            }
        }

        private void CurrentFrameChanged(object sender, EventArgs e)
        {
            if (_animator != null && PgbLoading.Visibility == Visibility.Collapsed)
            {
                SldFrame.Value = _animator.CurrentFrameIndex;
            }
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {
            if (!RepeatForever)
            {
                _animator.Pause();
                ImgPause.Source = new BitmapImage(new Uri("/AniView;component/Resources/Images/replay.png", UriKind.Relative));
            }
            Completed = true;
        }

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

            if (_animator != null)
            {
                _animator.CurrentFrameChanged += CurrentFrameChanged;
                _animator.AnimationCompleted += AnimationCompleted;
                SldFrame.Value = 0;
                SldFrame.Maximum = _animator.FrameCount - 1;
                LblDimensions.Content = ImgView.Source.Width + " x " + ImgView.Source.Height;
            }
        }

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

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "GIF Images (*.gif)|*.gif" };

            if (ofd.ShowDialog(this) == true)
            {
                LoadImage(ofd.FileName);
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

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

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(this).ShowDialog();
        }

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

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            _updateManager.CheckForUpdate(true, true);
        }

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

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

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

        private void BtnFullScreen_OnClick(object sender, RoutedEventArgs e)
        {
            ImgView.Stretch = BtnFullScreen.IsChecked ? Stretch.Fill : Stretch.None;
        }

        private void GridMain_OnDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null) return;
            if (!File.Exists(files[0])) return;

            LoadImage(files[0]);
        }

        private void GridMain_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!_arrowKeysEnabled) return;
            if (e.Key == Key.Left)
            {
                MoveLeft();
            }
            if (e.Key == Key.Right)
            {
                MoveRight();
            }
        }

        private void GridMain_OnLoaded(object sender, RoutedEventArgs e)
        {
            GridMain.Focus();
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            UnloadImage();
        }

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
