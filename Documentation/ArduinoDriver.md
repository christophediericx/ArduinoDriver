
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

