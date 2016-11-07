# ArduinoDriver

A .NET library to easily connect, drive (and debug) an Arduino (through a highly syntax compatible implementation running over the serial connection - at high speed).

![ArduinoDriver](https://github.com/christophediericx/ArduinoLibCSharp/blob/master/Images/ArduinoLibCSharp-header-color.png)

## Compatibility ##

The library has been tested with the following configurations:

| Arduino Model | MCU           | Bootloader protocol |
| ------------- |:-------------:| -------------------:|
| Mega 2560     | ATMega2560    | STK500v2            |
| Nano (R3)     | ATMega328P    | STK500v1            |
| Uno (R3)      | ATMega328P    | STK500v1            |

> *If you have a need for this library to run on another Arduino model, feel free to open an issue on GitHub, it should be relatively straightforward to add support (for most).*

## How to use the .NET library ##

Link the following nuget package in your project in order to use the ArduinoDriver: https://www.nuget.org/packages/ArduinoDriver/

Alternatively, install the package using the nuget package manager console:

```
Install-Package ArduinoDriver
```
An *ArduinoDriver* can be created in order to communicate with an attached board (through sending messages). The available commands mimick the functions available in the Arduino Language libraries itself (analog and digital reads / writes, set pinmodes, send tone,...) so most Arduino snippets found online can be directly ported to work with the ArduinoDriver instead.

For example, this small snippet illustrates on how to send a *Tone* request to Pin 8 on an attached Uno.

```csharp
using ArduinoDriver.SerialProtocol;
using ArduinoUploader.Hardware;

namespace ArduinoDriverDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Create an ArduinoDriver, autobootstrap the listener on the Arduino
            // (by passing true to the constructor) and send a ToneRequest to pin 8
            const byte tonePin = 8;

            var driver = new ArduinoDriver.ArduinoDriver(ArduinoModel.UnoR3, true);
            driver.Send(new ToneRequest(tonePin, 200, 1000));
        }
    }
}
```
For this to work, the C# ArduinoDriver library implements a serial communication protocol with a corresponding listener for the Arduino ([ArduinoListener.ino](Source/ArduinoDriver/ArduinoListener/ArduinoListener.ino)).

The protocol supports:
* Handshaking and version negotation
* High speed communication
* Fault tolerance and error detection (fletcher-16 checksums)
* Automated deployment of the listener code to the Arduino

The library emits log messages (in varying levels, from *Info* to *Trace*) via NLog. Hook up an NLog dependency (and configuration) in any project that uses *ArduinoDriver* to automagically emit these messages as well.

## Sample Code: Super Mario Bros "Underworld" theme ##

This sample project uses the library above to play this classic retro tune on an Arduino with C#.

One pin of the buzzer should be connected to digital pin 8. The other pin should be connected to GND.
