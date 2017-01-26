using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace AniView.Classes
{
    internal static class ImageExtractor
    {
        /// <summary>
        /// Extract all frames of an animated image
        /// </summary>
        /// <param name="path">The full path that points to an animated image</param>
        /// <param name="savePath">The full path where the frames should be saved</param>
        /// <returns>An integer</returns>
        internal static async Task<int> ExtractFrames(string path, string savePath)
        {
            await Task.Run(() =>
            {
                using (Image img = Image.FromFile(path))
                {
                    Image[] frames = GetFrames(img);
                    for (int i = 0; i < frames.Length; i++)
                    {
                        frames[i].Save(savePath + "\\" + i + ".png", ImageFormat.Png);
                    }
                    return 0;
                }
            });
            return 0;
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
                frames[i] = ((Image)originalImg.Clone());
            }
            return frames;
        }
    }
}
