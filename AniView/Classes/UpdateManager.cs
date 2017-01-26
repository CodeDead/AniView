using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Xml.Serialization;

namespace AniView.Classes
{
    /// <summary>
    /// The UpdateManager has the ability to check for software updates
    /// </summary>
    internal class UpdateManager
    {

        #region Variables
        private readonly string _updateUrl;
        private Update _update;
        #endregion

        /// <summary>
        /// Initiate a new UpdateManager object
        /// </summary>
        /// <param name="updateUrl">The URL where your XML update file is located</param>
        internal UpdateManager(string updateUrl)
        {
            _updateUrl = updateUrl;
            _update = new Update();
        }

        /// <summary>
        /// Check if there are updates available
        /// </summary>
        /// <param name="showErrors">Show a notification if an error occured</param>
        /// <param name="showNoUpdates">Show a notification if no updates are available</param>
        internal async void CheckForUpdate(bool showErrors, bool showNoUpdates)
        {
            try
            {
                WebClient wc = new WebClient();
                string xml = await wc.DownloadStringTaskAsync(_updateUrl);

                XmlSerializer serializer = new XmlSerializer(_update.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.Write(xml);
                    writer.Flush();
                    stream.Position = 0;
                    _update = (Update)serializer.Deserialize(stream);
                    writer.Dispose();
                }

                if (_update.CheckForUpdate())
                {
                    if (MessageBox.Show("Version " + _update.GetUpdateVersion() + " is now available!" + Environment.NewLine + "Would you like to download this version?", "AniView", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(_update.UpdateUrl);
                    }
                }
                else
                {
                    if (showNoUpdates)
                    {
                        MessageBox.Show("No updates are currently available", "AniView", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                if (showErrors)
                {
                    MessageBox.Show(ex.Message, "AniView", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
