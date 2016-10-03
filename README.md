# ArduinoDriver

A library to easily connect, drive (and debug) an Arduino from within Visual Studio (through a highly syntax compatible implementation running over the serial connection - at high speed).

![ArduinoDriver](https://github.com/christophediericx/ArduinoLibCSharp/blob/master/Images/ArduinoLibCSharp-header-color.png)

> *Compatibility note: This library has only been tested with UNO based Arduino boards. It is expected that tweaking of hardware constants in the STK-500 bootloader communication is required in order to support other architectures.*

## How to use ##

Link the following nuget package in your project in order to use the ArduinoDriver: https://www.nuget.org/packages/ArduinoDriver/

Alternatively, install the package using the nuget package manager console:

```
Install-Package ArduinoDriver
```
An *ArduinoDriver* can be created in order to communicate with an attached board (through sending messages). The available commands are mimicking the functions in the Arduino Language libraries itself (analog and digital reads / writes, set pinmodes, send tone,...) so most Arduino snippets found online can be directly ported to work with the ArduinoDriver instead.

```csharp
using ArduinoDriver.SerialProtocol;

namespace ArduinoDriverDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Create an ArduinoDriver, autobootstrap the listener on the Arduino
            // (by passing true to the constructor) and send a ToneRequest to pin 8
            const byte tonePin = 8;

            var driver = new ArduinoDriver.ArduinoDriver(true);
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

## Sample Code: Super Mario Bros "Underworld" theme ##

This sample project uses the library above to play this classic retro tune on an Arduino with C#.

One pin of the buzzer should be connected to digital pin 8. The other pin should be connected to GND.
