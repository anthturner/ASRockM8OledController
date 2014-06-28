using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LibOLEDController.Images
{
    public class OledRgbImage : OledImage
    {
        private PixelColor[,] PixelMatrix { get; set; }

        public OledRgbImage(OledController controller, BitmapSource source)
            : base(controller)
        {
            PixelMatrix = OledImage.GetPixels(source);
        }

        public OledRgbImage(OledController controller, Bitmap source)
            : base(controller)
        {
            PixelMatrix = OledImage.GetPixels(source);
        }

        internal override void Draw()
        {
            byte startX = 0;
            byte startY = 0;
            var data = new List<byte>();
            for (int y = 0; y < Controller.DisplayHeight; y++)
                for (int x = 0; x < Controller.DisplayWidth; x++)
                {
                    if (data.Count >= Controller.MaxLength)
                    {
                        WriteSingleRgbFrame(startX, startY, data.ToArray());
                        data.Clear();
                        startX = (byte)x;
                        startY = (byte)y;
                    }

                    data.AddRange(PixelMatrix[x,y].AsRgb565());
                }

            if (data.Count > 0)
                WriteSingleRgbFrame(startX, startY, data.ToArray());
        }
    }
}
