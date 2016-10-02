[<- Back to HOME](../../../)

### ArduinoDriver (library) ###

A library to easily connect, drive (and debug) an Arduino from within Visual Studio (through a highly syntax compatible implementation running over the serial connection - at high speed).

#### How to use ####

Link the following nuget package in your project in order to use the ArduinoDriver: https://www.nuget.org/packages/ArduinoLibCSharp.ArduinoDriver/

Alternatively, install the package using the nuget package manager console:

```
Install-Package ArduinoLibCSharp.ArduinoDriver
```
An *ArduinoDriver* can be created in order to communicate with an attached board (through sending messages). The available commands are mimicking the functions in the Arduino Language libraries itself (analog and digital reads / writes, set pinmodes, send tone,...) so most Arduino snippets found online can be directly ported to work with the ArduinoDriver instead.

```csharp
    // Create an ArduinoDriver, autobootstrap the listener on the Arduino
    // (by passing true to the constructor) and send a ToneRequest to pin 8
    const byte tonePin = 8;

    var driver = new ArduinoDriver(true);
    driver.Send(new ToneRequest(tonePin, 200, 1000));
```
For this to work, the C# ArduinoDriver library implements a serial communication protocol with a corresponding listener for the Arduino ([ArduinoListener.ino](Source/ArduinoLibCSharp.ArduinoDriver/ArduinoListener/ArduinoListener.ino)).

The protocol supports:
* Handshaking and version negotation
* High speed communication
* Fault tolerance and error detection (fletcher-16 checksums)
* Automated deployment of the listener code to the Arduino

