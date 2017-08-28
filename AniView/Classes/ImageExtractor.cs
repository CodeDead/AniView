using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

namespace AniView.Classes
{
    /// <summary>
    /// This class contains methods and functions to manipulate an image
    /// </summary>
    internal static class ImageExtractor
    {
        /// <summary>
        /// Extract all frames of an animated image
        /// </summary>
        /// <param name="path">The full path that points to an animated image</param>
        /// <param name="savePath">The full path where the frames should be saved</param>
        /// <param name="format">The image format in which the frames should be exported</param>
        /// <returns>An integer</returns>
        internal static async Task<int> ExtractFrames(string path, string savePath, ImageFormat format)
        {
            await Task.Run(() =>
            {
                using (Image img = Image.FromFile(path))
                {
                    Image[] frames = GetFrames(img);
                    string ext = FileExtensionFromEncoder(format);
                    for (int i = 0; i < frames.Length; i++)
                    {
                        frames[i].Save(savePath + "\\" + i + ext, format);
                    }
                    return 0;
                }
            });
            return 0;
        }

        /// <summary>
        /// Retrieve the file extension from an ImageFormat
        /// </summary>
        /// <param name="format">The ImageFormat</param>
        /// <returns>The file extension from an ImageFormat</returns>
        private static string FileExtensionFromEncoder(ImageFormat format)
        {
            try
            {
                return ImageCodecInfo.GetImageEncoders()
                        .First(x => x.FormatID == format.Guid)
                        .FilenameExtension
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .First()
                        .Trim('*')
                        .ToLower();
            }
            catch (Exception)
            {
                return ".unknown";
            }
        }

        /// <summary>
        /// Get all the frames of an animated image
        /// </summary>
        /// <param name="originalImg">The animated image</param>
        /// <returns>All the frames of an animated image</returns>
        private static Image[] GetFrames(Image originalImg)
        {
            int numberOfFrames = originalImg.GetFrameCount(FrameDimension.Time);
            Image[] frames = new Image[numberOfFrames];

            for (int i = 0; i < numberOfFrames; i++)
            {
                originalImg.SelectActiveFrame(FrameDimension.Time, i);
                frames[i] = (Image)originalImg.Clone();
            }
            return frames;
        }
    }
}
