# ArduinoLibCSharp
ArduinoLibCSharp is a library for controlling an Arduino board directly with C# / Visual Studio.

.NET assemblies:

  * **ArduinoDriver**: A library to easily connect, drive and debug an Arduino from within Visual Studio (through a highly *syntax compatible* implementation running over the serial connection - at high speed, with automatic error detection and correction).
  * **ArduinoUploader**: A library to upload an Intel HEX file (e.g. compiled sketches) directly to an Arduino over a (USB) serial port without having to use the Arduino IDE (or avrdude).
  * **IntelHexFormatReader**: Parse the contents of a HEX file into a representative "memory representation".

Command line applications:

  * **ArduinoSketchUploader**

![ArduinoLibCSharp](https://github.com/christophediericx/ArduinoLibCSharp/blob/master/Images/ArduinoLibCSharp-header-color.png)

> *Compatibility note: This library has only been tested with UNO based Arduino boards. It is expected that tweaking of hardware constants in the STK-500 bootloader communication is required in order to support other architectures.*

## Components ##

### ArduinoDriver (library)###
An *ArduinoDriver* can be created in order to communicate with an attached board (through sending messages). The available commands are mimicking the functions in the Arduino Language libraries itself (analog and digital reads / writes, set pinmodes, send tone,...) so most Arduino snippets found online can be directly ported to work with the ArduinoDriver instead.

```csharp
    const byte pin = 8;
    var driver = new ArduinoDriver(true);
    driver.Send(new ToneRequest(pin, 200, 1000));
```
For this to work, the C# ArduinoDriver library implements a serial communication protocol with a corresponding listener for the Arduino ([ArduinoListener.ino](Source/ArduinoLibCSharp.ArduinoDriver/ArduinoListener/ArduinoListener.ino)).

The protocol supports:
* Handshaking and version negotation
* High speed communication
* Fault tolerance and error detection (fletcher-16 checksums)
* Automated deployment of the listener code to the Arduino

### ArduinoUploader (library) ###

The *ArduinoUploader* library talks to the Arduino's bootloader directly through a dialect of the STK-500 protocol in order to flash the memory on the device with the contents of an Intel HEX file.

The solution comes with a seperate **ArduinoSketchUploader** command line utility which can be used to upload compiled sketch programs to the Arduino directly without requiring interaction from the Arduino IDE. It's fully self-contained and not just a wrapper for avrdude.

### Sample Code: Playing the Super Mario Bros "Underworld" theme

In a sample project, the above libraries are used to play this retro tune on the Arduino directly from a C# program.
