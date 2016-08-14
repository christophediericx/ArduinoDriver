# ArduinoLibCSharp
ArduinoLibCSharp is a comprehensive library for controlling an Arduino board directly with C# / Visual Studio.

![ArduinoLibCSharp](https://github.com/christophediericx/ArduinoLibCSharp/blob/master/Images/ArduinoLibCSharp-header-color.png)

## Components ##

### ArduinoDriver (library)###
An ArduinoDriver is created in order to communicate with an attached Arduino (through sending messages). The commands available are functionally identical to the ones present in the Arduino Language libraries itself (e.g. read / write analog and digital outputs, set pinmodes, send tone / notone ,...). This makes that most Arduino snippets online can be very easily adapted to work with the ArduinoDriver.

```csharp
    var pin = 8;
    var driver = new ArduinoDriver.ArduinoDriver(true);
    driver.Send(new ToneRequest(pin, 200, 1000));
    Thread.Sleep(500);
    driver.Send(new NoToneRequest(pin));
```

### ArduinoUploader (library) ###
### ArduinoSketchUploader (command line utility) ###
### Sample Code: Super Mario Bros "Underworld" theme
