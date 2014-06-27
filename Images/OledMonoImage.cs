using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace LibOLEDController.Images
{
    public class OledMonoImage : OledImage
    {
        public enum DitheringMethod
        {
            FloydSteinberg,
            None
        };

        private bool[,] PixelMatrix { get; set; }

        internal OledMonoImage(OledController controller, DitheringMethod dithering, BitmapSource source) : base(controller)
        {
            BitmapSource updatedImage = null;
            switch (dithering)
            {
                case DitheringMethod.FloydSteinberg:
                    updatedImage = OledImage.GetFloydSteinbergDithered(source);
                    break;
                default:
                case DitheringMethod.None:
                    updatedImage = source;
                    break;
            }

            PixelMatrix = OledImage.GetPixelsBW(updatedImage);
        }

        internal OledMonoImage(OledController controller, DitheringMethod dithering, Bitmap source)
            : base(controller)
        {
            Bitmap updatedImage = null;
            switch (dithering)
            {
                case DitheringMethod.FloydSteinberg:
                    updatedImage = OledImage.GetFloydSteinbergDithered(source);
                    break;
                case DitheringMethod.None:
                    updatedImage = source;
                    break;
            }

            PixelMatrix = OledImage.GetPixelsBW(updatedImage);
        }

        internal override void Draw()
        {
            byte startCol = 0;
            byte startPage = 0;
            var data = new List<byte>();
            for (int y = 0; y < Controller.DisplayHeight / 8; y++)
                for (int x = 0; x < Controller.DisplayWidth; x++)
                {
                    if (data.Count >= Controller.MaxLength)
                    {
                        WriteSingleMonoFrame(startPage, startCol, data.ToArray());
                        data.Clear();
                        startCol = (byte)x;
                        startPage = (byte)y;
                    }

                    data.Add(GetSingleColumnPage(y * 8, x));
                }

            if (data.Count > 0)
                WriteSingleMonoFrame(startPage, startCol, data.ToArray());
        }

        private byte GetSingleColumnPage(int pageStart, int column)
        {
            var bits = new BitArray(8);
            for (int pageValue = 0; pageValue < 8; pageValue++)
                bits[pageValue] = PixelMatrix[column, pageStart + pageValue];
            var data = new byte[1];
            bits.CopyTo(data, 0);
            return data[0];
        }
    }
}