using System.Drawing;
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
        }
    }
}
