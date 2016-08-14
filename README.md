# ArduinoLibCSharp
ArduinoLibCSharp is a library for controlling an Arduino board directly with C# / Visual Studio.

![ArduinoLibCSharp](https://github.com/christophediericx/ArduinoLibCSharp/blob/master/Images/ArduinoLibCSharp-header-color.png)

## Components ##

### ArduinoDriver (library)###
An *ArduinoDriver* can be created in order to communicate with an attached Arduino (through sending *messages*). The available commands are fully mimicking the functions in the Arduino Language libraries itself (read / write analog and digital outputs, set pinmodes, send tone / notone ,...), so most Arduino snippets online can be directly ported to work with the ArduinoDriver instead.

```csharp
    const byte pin = 8;
    var driver = new ArduinoDriver(true);
    driver.Send(new ToneRequest(pin, 200, 1000));
```
For this to work, the C# ArduinoDriver library implements a serial communication protocol with a corresponding listener for the Arduino ([ArduinoListener.ino](Source/ArduinoLibCSharp.ArduinoDriver/ArduinoListener/ArduinoListener.ino)).

The protocol supports:
* Handshaking and version negotation
* High speed communication (baud rate: 115200)
* Fault tolerance and error correction (Fletcher 16 checksums)
* Automated deployment of the listener code to the Arduino through interacting with the Arduino's bootloader protocol

### ArduinoUploader (library) ###

The *ArduinoUploader* library talks to the Arduino's bootloader directly through a dialect of the STK-500 protocol in order to flash the devices memory with the contents of an Intel HEX file.

The solution comes with a seperate *ArduinoSketchUploader* command line utility which can be used to directly upload compiled sketch programs to the Arduino from C# without requiring interaction from the Arduino IDE or AVRDude.

### Sample Code: Super Mario Bros "Underworld" theme
