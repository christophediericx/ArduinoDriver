[<- Back to HOME](../../../)

### ArduinoUploader (library) ###

A library to upload an Intel HEX file (e.g. compiled sketches) directly to an Arduino over a (USB) serial port without having to use the Arduino IDE (or avrdude).

#### How to use ####

Link the following nuget package in your project in order to use the ArduinoUploader: https://www.nuget.org/packages/ArduinoLibCSharp.ArduinoUploader/

Alternatively, install the package using the nuget package manager console:

```
Install-Package ArduinoLibCSharp.ArduinoUploader
```

The *ArduinoUploader* library talks to the Arduino's bootloader directly through a dialect of the STK-500 protocol in order to flash the memory on the device with the contents of an Intel HEX file. This solution is fully self-contained (and native C#) and not just a wrapper for avrdude.

The following minimal snippet shows how to upload a .hex file to an Arduino (UNO) board with the library:

```csharp
using ArduinoLibCSharp.ArduinoUploader;

namespace ArduinoUploaderDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var uploader = new ArduinoSketchUploader(
                new ArduinoSketchUploaderOptions()
                {
                    FileName = @"C:\MyHexFiles\MyHexFile.hex",
                    PortName = "COM3"
                });

            uploader.UploadSketch();
        }
    }
}
```
