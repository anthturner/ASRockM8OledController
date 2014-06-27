ASRockM8OledController
======================

This library is designed to allow users to draw data to and receive input from the OLED dial controller on the front of the ASRock M8 barebones system.

There is still a significant amount of work to be done on the library; there is experimental RGB support I've found but haven't made work yet. (Classes are stubbed out presently)

I haven't tested this in Mono yet, but I'm guessing it's dependent on getting the USB HID interface to work; it's fairly easy to slot in and out, but if I find one that's cross-platform between .NET and Mono, I'll switch to that. Right now I'm using UsbHid.

I'm redistributing UsbHid.dll because I can't remember where I got it or what the license is. If you own it, please let me know if I'm infringing on your license and I'll be happy to take it down and point to your website instead.

If you're using this, please let me know! I always appreciate hearing people use this.
