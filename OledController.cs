using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using LibOLEDController.Images;
using UsbHid;
using UsbHid.USB.Classes.Messaging;

namespace LibOLEDController
{
    public class OledController
    {
        public enum CanvasRotation
        {
            Degree0,
            Degree90,
            Degree180,
            Degree270
        };
        public enum UserInputType
        {
            ChassisRotated,
            ButtonPushed,
            DialTurnedLeft,
            DialTurnedRight
        }
        public delegate void InputReceivedDelegate(UserInputType type);
        /// <summary>
        /// Fired whenever the user manipulates the dial, turns the chassis, or presses the button
        /// </summary>
        public event InputReceivedDelegate InputReceived;

        private UsbHidDevice Device { get; set; }
        private DeviceAttributes Attributes { get; set; }
        private Timer KeepAliveTimer { get; set; }

        internal byte DisplayWidth { get { return (byte)Attributes.DisplayWidth; } }
        internal byte DisplayHeight { get { return (byte) Attributes.DisplayHeight; } }
        internal byte MaxLength { get { return (byte) Attributes.DisplayMaxLength; } }

        /// <summary>
        /// If the power LED is on or off
        /// </summary>
        public bool PowerLedOn { get { return Attributes.LedPower; } }

        /// <summary>
        /// If the decor LEDs in the chassis are on or off
        /// </summary>
        public bool ChassisLedOn { get { return Attributes.LedChassis; } }

        /// <summary>
        /// Connect to the device's OLED display controller using the default VID/PID
        /// </summary>
        public OledController()
        {
            Device = new UsbHidDevice(0x0416, 0xE007);
            Device.DataReceived += Device_DataReceived;
            Device.Connect();
            KeepAliveTimer = new Timer(state =>
            {
                if (Device != null && Device.IsDeviceConnected) Device.SendCommandMessage(0x21);
            });
            UpdateAttributes();
        }

        void Device_DataReceived(byte[] data)
        {
            if (data[1] == 0x81)
                Attributes = new DeviceAttributes(data.Skip(4).ToArray());
        }

        /// <summary>
        /// Update the configuration attributes from the device
        /// </summary>
        public void UpdateAttributes()
        {
            WriteUsbPacket(new byte[]{0x01});
        }

        /// <summary>
        /// Draw a monochrome image
        /// </summary>
        /// <param name="dithering">Dithering method</param>
        /// <param name="source">Source image</param>
        public void DrawMono(OledMonoImage.DitheringMethod dithering, BitmapSource source)
        {
            var monoImg = new OledMonoImage(this, dithering, source);
            monoImg.Draw();
        }
        /// <summary>
        /// Draw a monochrome image
        /// </summary>
        /// <param name="dithering">Dithering method</param>
        /// <param name="source">Source image</param>
        public void DrawMono(OledMonoImage.DitheringMethod dithering, Bitmap source)
        {
            var monoImg = new OledMonoImage(this, dithering, source);
            monoImg.Draw();
        }

        /// <summary>
        /// Draw a color image (unsupported currently)
        /// </summary>
        /// <param name="source">Source image</param>
        public void DrawColor(BitmapSource source)
        {
            var rgbImg = new OledRgbImage(this, source);
            rgbImg.Draw();
        }

        /// <summary>
        /// Draw a color image (unsupported currently)
        /// </summary>
        /// <param name="source">Source image</param>
        public void DrawColor(Bitmap source)
        {
            var rgbImg = new OledRgbImage(this, source);
            rgbImg.Draw();
        }

        internal void WriteUsbPacket(byte[] data)
        {
            if (data.Length == 1)
                Device.SendCommandMessage(data[0]);
            else
                Device.SendMessage(new CommandMessage(data[0], data.Skip(1).ToArray()));
        }
    }
}
