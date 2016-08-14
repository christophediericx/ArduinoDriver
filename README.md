# ArduinoLibCSharp
ArduinoLibCSharp is a library for controlling an Arduino board directly with C# / Visual Studio.

![ArduinoLibCSharp](https://github.com/christophediericx/ArduinoLibCSharp/blob/master/Images/ArduinoLibCSharp-header-color.png)

## Components ##

### ArduinoDriver (library)###
An *ArduinoDriver* can be created in order to communicate with an attached Arduino (through sending *messages*). The available commands are fully mimicking the functions in the Arduino Language libraries (read / write analog and digital outputs, set pinmodes, send tone / notone ,...), so most Arduino snippets online can be directly ported to work with the ArduinoDriver instead.

```csharp
    const byte pin = 8;
    var driver = new ArduinoDriver(true);
    driver.Send(new ToneRequest(pin, 200, 1000));
```
For this to work, the C# ArduinoDriver library implements a serial communication protocol with a corresponding listener for the Arduino ([ArduinoListener.ino](Source/ArduinoLibCSharp.ArduinoDriver/ArduinoListener/ArduinoListener.ino)).

The protocol supports:
* Handshaking and version negotation
* Fault tolerance / Error correction (through Fletcher 16 checksums)
* Automated deployment of the listener code to the Arduino through interacting with the Arduino's bootloader protocol.

### ArduinoUploader (library) ###
### ArduinoSketchUploader (command line utility) ###
### Sample Code: Super Mario Bros "Underworld" theme
