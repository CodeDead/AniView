using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using AniView.Classes;
using XamlAnimatedGif;
using Application = System.Windows.Application;
using DataFormats = System.Windows.DataFormats;
using DownloadProgressEventArgs = XamlAnimatedGif.DownloadProgressEventArgs;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Path = System.IO.Path;

namespace AniView.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Variables
        private List<string> _images = new List<string>();
        private int _current;
        private Animator _animator;
        private bool _useDefaultRepeatBehavior = true;
        private bool _repeatForever;
        private bool _useSpecificRepeatCount;
        private int _repeatCount = 3;
        private bool _completed;
        private RepeatBehavior _repeatBehavior;
        private bool _autoStart = true;
        private bool _isDownloading;
        private int _downloadProgress;
        private bool _isDownloadProgressIndeterminate;
        private string _currentPath = "";
        private readonly UpdateManager _updateManager;
        #endregion

        public MainWindow()
        {
            _updateManager = new UpdateManager("http://codedead.com/Software/AniView/update.xml");

            InitializeComponent();
            ChangeVisualStyle();
            LoadRepeatBehaviour();

            LoadArguments();
            AutoUpdate();
            LoadSettings();
        }

        internal void LoadRepeatBehaviour()
        {
            try
            {
                AnimationBehavior.SetRepeatBehavior(ImgView, Properties.Settings.Default.RepeatBehaviour == 0 ? RepeatBehavior.Forever : new RepeatBehavior(Properties.Settings.Default.RepeatBehaviour));
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

            try
            {
                BitmapImage bitmap = new BitmapImage();
                FileStream stream = File.OpenRead(path);
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                stream.Close();
                stream.Dispose();
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show(ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AnimationBehavior.SetSourceUri(ImgView, new Uri(path));
            _images = new List<string>();
            ImgPause.Source = new BitmapImage(new Uri("/AniView;component/Resources/Images/pin.png", UriKind.Relative));
            SizeToContent = SizeToContent.WidthAndHeight;

            foreach (string s in Directory.GetFiles(Path.GetDirectoryName(path), "*.gif", SearchOption.TopDirectoryOnly))
            {
                if (!_images.Contains(s))
                {
                    _images.Add(s);
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

            Title = "AniView - " + _currentPath;
            LblSize.Content = new FileInfo(path).Length + " bytes";
        }

        private void BtnRight_Click(object sender, RoutedEventArgs e)
        {
            if (_images.Count == 0) return;
            _current++;
            if (_current > _images.Count - 1)
            {
                _current = 0;
            }
            LoadImage(_images[_current]);
        }

        private void BtnLeft_Click(object sender, RoutedEventArgs e)
        {
            if (_images.Count == 0) return;
            _current--;
            if (_current < 0)
            {
                _current = _images.Count - 1;
            }
            LoadImage(_images[_current]);
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
            if (_animator != null)
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

        public bool UseDefaultRepeatBehavior
        {
            get { return _useDefaultRepeatBehavior; }
            set
            {
                _useDefaultRepeatBehavior = value;
                OnPropertyChanged("UseDefaultRepeatBehavior");
                if (value)
                {
                    RepeatBehavior = default(RepeatBehavior);
                }
            }
        }

        public bool RepeatForever
        {
            get { return _repeatForever; }
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

        public bool UseSpecificRepeatCount
        {
            get { return _useSpecificRepeatCount; }
            set
            {
                _useSpecificRepeatCount = value;
                OnPropertyChanged("UseSpecificRepeatCount");
                if (value)
                {
                    RepeatBehavior = new RepeatBehavior(RepeatCount);
                }
            }
        }

        public int RepeatCount
        {
            get { return _repeatCount; }
            set
            {
                _repeatCount = value;
                OnPropertyChanged("RepeatCount");
                if (UseSpecificRepeatCount)
                {
                    RepeatBehavior = new RepeatBehavior(value);
                }
            }
        }

        public bool Completed
        {
            get { return _completed; }
            set
            {
                _completed = value;
                OnPropertyChanged("Completed");
            }
        }

        public RepeatBehavior RepeatBehavior
        {
            get { return _repeatBehavior; }
            set
            {
                _repeatBehavior = value;
                OnPropertyChanged("RepeatBehavior");
                Completed = false;
            }
        }

        public bool AutoStart
        {
            get { return _autoStart; }
            set
            {
                _autoStart = value;
                OnPropertyChanged("AutoStart");
            }
        }

        public bool IsDownloading
        {
            get { return _isDownloading; }
            set
            {
                _isDownloading = value;
                OnPropertyChanged("IsDownloading");
            }
        }

        public int DownloadProgress
        {
            get { return _downloadProgress; }
            set
            {
                _downloadProgress = value;
                OnPropertyChanged("DownloadProgress");
            }
        }

        public bool IsDownloadProgressIndeterminate
        {
            get { return _isDownloadProgressIndeterminate; }
            set
            {
                _isDownloadProgressIndeterminate = value;
                OnPropertyChanged("IsDownloadProgressIndeterminate");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AnimationBehavior_OnLoaded(object sender, RoutedEventArgs e)
        {
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
                Process.Start("http://codedead.com/");
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
    }
}
