ASRockM8OledController
======================

This library is designed to allow users to draw data to and receive input from the OLED dial controller on the front of the ASRock M8 barebones system.

There is still a significant amount of work to be done on the library; there is experimental RGB support I've found but haven't made work yet. (Classes are stubbed out presently)

I haven't tested this in Mono yet, but I'm guessing it's dependent on getting the USB HID interface to work; it's fairly easy to slot in and out, but if I find one that's cross-platform between .NET and Mono, I'll switch to that. Right now I'm using UsbHid.

I'm redistributing UsbHid.dll because I can't remember where I got it or what the license is. If you own it, please let me know if I'm infringing on your license and I'll be happy to take it down and point to your website instead.

If you're using this, please let me know! I always appreciate hearing people use this.

** All images sent to the draw routines must be 96x96px to match the size of the OLED display **

## Sample Code ##
### Displaying a GIF ###
```csharp
var oledController = new LibOLEDController.OledController();
var imageDecode = new GifBitmapDecoder(new Uri("mygif.gif"), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
foreach (var frame in imageDecode.Frames)
  oledController.DrawMono(OledMonoImage.DitheringMethod.FloydSteinberg, frame);
```
### Catching Input ###
```csharp
var oledController = new LibOLEDController.OledController();
oledController.InputReceived += type =>
{
  switch (type)
  {
    case OledController.UserInputType.ButtonPushed:
      Console.WriteLine("User pushed button on the front");
      break;
    case OledController.UserInputType.ChassisRotated:
      Console.WriteLine("User rotated the chassis 90 degrees");
      break;
    case OledController.UserInputType.DialTurnedLeft:
      Console.WriteLine("User turned the dial to the left one click");
      break;
    case OledController.UserInputType.DialTurnedRight:
      Console.WriteLine("User turned the dial to the right one click");
      break;
  }
};
```
