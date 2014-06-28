using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LibOLEDController.Images
{
    public abstract class OledImage
    {
        protected OledController Controller { get; private set; }

        protected OledImage(OledController controller)
        {
            Controller = controller;
        }

        internal abstract void Draw();
        
        protected void WriteSingleMonoFrame(byte page, byte column, byte[] frame)
        {
            var bytes = new List<byte>();
            bytes.Add(0x03);
            bytes.Add((byte)(3 + Controller.MaxLength));
            bytes.Add(page);
            bytes.Add(column);
            bytes.Add((byte)frame.Length);
            bytes.AddRange(frame);

            if (frame.Length < Controller.MaxLength)
                bytes.AddRange(new byte[Controller.MaxLength - frame.Length]);

            Controller.WriteUsbPacket(bytes.ToArray());
        }

        protected void WriteSingleRgbFrame(byte x, byte y, byte[] frame)
        {
            var bytes = new List<byte>();
            bytes.Add(0x04);
            bytes.Add((byte)(3 + Controller.MaxLength));
            bytes.Add(x);
            bytes.Add(y);
            bytes.Add((byte)frame.Length);
            bytes.AddRange(frame);

            if (frame.Length < Controller.MaxLength)
                bytes.AddRange(new byte[Controller.MaxLength - frame.Length]);

            Controller.WriteUsbPacket(bytes.ToArray());
        }

        #region Floyd-Steinberg
        protected static BitmapSource GetFloydSteinbergDithered(BitmapSource input)
        {
            return ConvertBitmap(GetFloydSteinbergDithered(BitmapFromSource(input)));
        }

        protected static Bitmap GetFloydSteinbergDithered(Bitmap input)
        {
            var masks = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
            var output = new Bitmap(input.Width, input.Height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            var data = new sbyte[input.Width, input.Height];
            var inputData = input.LockBits(new Rectangle(0, 0, input.Width, input.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            try
            {
                var scanLine = inputData.Scan0;
                var line = new byte[inputData.Stride];
                for (var y = 0; y < inputData.Height; y++, scanLine += inputData.Stride)
                {
                    Marshal.Copy(scanLine, line, 0, line.Length);
                    for (var x = 0; x < input.Width; x++)
                    {
                        data[x, y] = (sbyte)(64 * (GetGreyLevel(line[x * 3 + 2], line[x * 3 + 1], line[x * 3 + 0]) - 0.5));
                    }
                }
            }
            finally
            {
                input.UnlockBits(inputData);
            }
            var outputData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            try
            {
                var scanLine = outputData.Scan0;
                for (var y = 0; y < outputData.Height; y++, scanLine += outputData.Stride)
                {
                    var line = new byte[outputData.Stride];
                    for (var x = 0; x < input.Width; x++)
                    {
                        var j = data[x, y] > 0;
                        if (j) line[x / 8] |= masks[x % 8];
                        var error = (sbyte)(data[x, y] - (j ? 32 : -32));
                        if (x < input.Width - 1) data[x + 1, y] += (sbyte)(7 * error / 16);
                        if (y < input.Height - 1)
                        {
                            if (x > 0) data[x - 1, y + 1] += (sbyte)(3 * error / 16);
                            data[x, y + 1] += (sbyte)(5 * error / 16);
                            if (x < input.Width - 1) data[x + 1, y + 1] += (sbyte)(1 * error / 16);
                        }
                    }
                    Marshal.Copy(line, 0, scanLine, outputData.Stride);
                }
            }
            finally
            {
                output.UnlockBits(outputData);
            }
            return output;
        }
        #endregion

        #region Get Pixels
        protected static bool[,] GetPixelsBW(Bitmap source)
        {
            return GetPixelsBW(ConvertBitmap(source));
        }

        protected static bool[,] GetPixelsBW(BitmapSource source)
        {
            var px = GetPixels(source);
            var bools = new bool[source.PixelWidth, source.PixelHeight];
            for (int w = 0; w < px.GetLength(0); w++)
                for (int h = 0; h < px.GetLength(1); h++)
                {
                    var c = px[w, h];
                    bools[w, h] = !(GetGreyLevel(c.Red, c.Green, c.Blue) < 0.5d);
                }
            return bools;
        }

        protected static PixelColor[,] GetPixels(Bitmap source)
        {
            return GetPixels(ConvertBitmap(source));
        }

        protected static PixelColor[,] GetPixels(BitmapSource source)
        {
            if (source.Format != PixelFormats.Bgra32)
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            var height = source.PixelHeight;
            var width = source.PixelWidth;
            var pixels = new PixelColor[width, height];
            var stride = (source.Format.BitsPerPixel/8) * width;
            var offset = 0;
            var pixelBytes = new byte[height * width * 4];
            source.CopyPixels(pixelBytes, stride, 0);
            int y0 = offset / width;
            int x0 = offset - width * y0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    pixels[x + x0, y + y0] = new PixelColor
                    {
                        Blue = pixelBytes[(y * width + x) * 4 + 0],
                        Green = pixelBytes[(y * width + x) * 4 + 1],
                        Red = pixelBytes[(y * width + x) * 4 + 2],
                        Alpha = pixelBytes[(y * width + x) * 4 + 3],
                    };
            return pixels;
        }

        private static double GetGreyLevel(byte r, byte g, byte b)
        {
            return (r * 0.299 + g * 0.587 + b * 0.114) / 255;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct PixelColor
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Alpha;

            public byte[] AsRgb565()
            {
                var result = new byte[2];
                var r = (byte)((Red / 255.0) * 31); //R component
                var g = (byte)((Green / 255.0) * 63); //G component
                var b = (byte)((Blue / 255.0) * 31); //B component
                result[1] = (byte)(((r & 0x1f) << 3) | ((g >> 3) & 0x7)); //R (5 bits) +  G (upper 3 bits)
                result[0] = (byte)(((g & 0x7) << 5) | (b & 0x1f)); //G (lower 3 bits) + B (5 bits)

                return result;
            }
        }
        #endregion

        #region Bitmap Conversion
        protected static BitmapSource ConvertBitmap(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }

        protected static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }
        #endregion
    }
}
