using System;
using System.Collections;
using System.Windows.Media;

namespace LibOLEDController
{
    internal class DeviceAttributes
    {
        internal enum CanvasControl
        {
            ByFirmware = 0,
            ByHost = 1
        };

        internal enum SystemMode
        {
            Eco,
            Standard,
            Speed
        };

        internal ushort DisplayWidth { get; private set; }
        internal ushort DisplayHeight { get; private set; }
        internal byte DisplayMaxLength { get; private set; }
        internal Color DisplayColor { get; private set; }
        internal ushort DisplayColorSetting { get; private set; }
        internal OledController.CanvasRotation DisplayCanvasRotation { get; private set; }

        internal bool GSensorInitStatus { get; private set; }
        internal ushort GSensorXAccel { get; private set; }
        internal ushort GSensorYAccel { get; private set; }
        internal ushort GSensorZAccel { get; private set; }
        internal byte GSensorXAngle { get; private set; }
        internal byte GSensorYAngle { get; private set; }
        internal byte GSensorZAngle { get; private set; }

        internal CanvasControl CanvasControlMode { get; private set; }

        internal DateTime CurrentTime { get; private set; }
        internal byte CurrentVolume { get; private set; }
        internal bool ShowTime { get; private set; }
        internal SystemMode CurrentMode { get; private set; }

        internal byte SmbusTrafficStatus { get; private set; }

        internal int ButtonRepeatDelay { get; private set; }
        internal int ButtonRepeatRate { get; private set; }

        internal bool LedPower { get; private set; }
        internal bool LedChassis { get; private set; }
        internal bool LedLaserBeam { get; private set; }
        internal bool LedLaserBeamChanged { get; private set; }
        
        internal DeviceAttributes(byte[] Payload)
        {
            DisplayWidth = BitConverter.ToUInt16(Payload, 0);
            DisplayHeight = BitConverter.ToUInt16(Payload, 2);
            DisplayMaxLength = Payload[4];

            var gSensorData = new BitArray(new byte[] {Payload[5]});
            GSensorInitStatus = gSensorData[7]; // this may be inverted from 7 to 0

            var deviceAttributes = new BitArray(Payload[6]);
            CanvasControlMode = deviceAttributes[6] ? CanvasControl.ByFirmware : CanvasControl.ByHost;
            // todo: power button behavior

            SmbusTrafficStatus = Payload[7];

            ButtonRepeatDelay = Payload[8]*100;

            ButtonRepeatRate = 49+Payload[9];

            var timeY = BitConverter.ToUInt16(Payload, 10);
            var timeMo = Payload[12];
            var timeD = Payload[13];
            var timeH = Payload[14];
            var timeMi = Payload[15];
            var timeS = Payload[16];
            var dayOfWeek = Payload[17];
            CurrentTime = new DateTime(timeY, timeMo, timeD, timeH, timeMi, timeS);

            DisplayColor = Color.FromRgb(Payload[18], Payload[19], Payload[20]);
            DisplayColorSetting = BitConverter.ToUInt16(Payload, 21);

            var canvasRotation = new BitArray(new byte[]{Payload[23]});
            if (!canvasRotation[1] && !canvasRotation[0])
                DisplayCanvasRotation = OledController.CanvasRotation.Degree0;
            else if (!canvasRotation[1] && canvasRotation[0])
                DisplayCanvasRotation = OledController.CanvasRotation.Degree90;
            else if (canvasRotation[1] && !canvasRotation[0])
                DisplayCanvasRotation = OledController.CanvasRotation.Degree180;
            else if (canvasRotation[1] && canvasRotation[0])
                DisplayCanvasRotation = OledController.CanvasRotation.Degree270;

            GSensorXAccel = BitConverter.ToUInt16(Payload, 24);
            GSensorYAccel = BitConverter.ToUInt16(Payload, 26);
            GSensorZAccel = BitConverter.ToUInt16(Payload, 28);
            GSensorXAngle = Payload[30];
            GSensorYAngle = Payload[31];
            GSensorZAngle = Payload[32];

            CurrentVolume = Payload[33];
            ShowTime = (Payload[34] == 0 ? true : false);

            switch (Payload[35])
            {
                case 0:
                    CurrentMode = SystemMode.Eco;
                    break;
                case 1:
                    CurrentMode = SystemMode.Standard;
                    break;
                case 2:
                    CurrentMode = SystemMode.Speed;
                    break;
            }

            var displayControl = new BitArray(new byte[] {Payload[36]});
            LedPower = displayControl[7];
            LedChassis = displayControl[6];
            LedLaserBeam = displayControl[5];
            LedLaserBeamChanged = displayControl[7];
        }
    }
}
