# ArduinoDriver

A .NET library to easily connect, drive and debug an Arduino through a simple and highly Arduino syntax compatible request / response protocol running over the serial (USB) connection.

![ArduinoDriver](https://github.com/christophediericx/ArduinoDriver/blob/master/Images/ArduinoDriver.png)

## Summary ##
Add two using statements to you project and the two-line snippet below can illustrate some of the library's features:
```csharp

        using ArduinoDriver.SerialProtocol;
        using ArduinoUploader.Hardware;
        
        var driver = new ArduinoDriver.ArduinoDriver(ArduinoModel.UnoR3, true);
        driver.Send(new ToneRequest(8, 200, 1000));

```

* This creates an ArduinoDriver for a specific Arduino Model (in this case an Uno).

* The relevant COM port is autodetected (although a constructor overload for specifying the port exists).

* The second parameter to the constructor (*autoBootstrap*) enables automated deployment of our [protocol listener](Source/ArduinoDriver/ArduinoListener/ArduinoListener.ino) onto the Arduino (if necessary). This means you don't have to compile / deploy anything on the Arduino itself in order to start using this library (the library uses the related [ArduinoSketchUploader](https://github.com/christophediericx/ArduinoSketchUploader) library to achieve this).

* Use the *Send* method on the driver in order to send a message to the Arduino, and receive a response. Most of the typical Arduino library methods have completely analogous request / counterparts:

  * AnalogReadRequest / AnalogReadResponse
  * AnalogWriteRequest / AnalogWriteResponse
  * DigitalReadRequest / DigitalReadResponse
  * DigitalWriteRequest / DigitalWriteResponse
  * PinModeRequest / PinModeResponse
  * ...

The protocol itself supports:
* Handshaking and version negotation
* High speed communication
* Fault tolerance and error detection (through fletcher-16 checksums)

## Compatibility ##

The library has been tested with the following configurations:

| Arduino Model | MCU           | Bootloader protocol |
| ------------- |:-------------:| -------------------:|
| Leonardo      | ATMega32U4    | AVR109              |
| Mega 2560     | ATMega2560    | STK500v2            |
| Micro         | ATMega32U4    | AVR109              |
| Nano (R3)     | ATMega328P    | STK500v1            |
| Uno (R3)      | ATMega328P    | STK500v1            |

> *If you have a need for this library to run on another Arduino model, feel free to open an issue on GitHub, it should be relatively straightforward to add support (for most).*

## How to use the .NET library ##

[![NuGet version](https://badge.fury.io/nu/ArduinoDriver.svg)](https://badge.fury.io/nu/ArduinoDriver)

Link the following nuget package in your project in order to use the ArduinoDriver: https://www.nuget.org/packages/ArduinoDriver/

Alternatively, install the package using the nuget package manager console:

```
Install-Package ArduinoDriver
```

## Logging ##

The library channels log messages (in varying levels, from Info to Trace) via NLog. Optionally, add a nuget NLog dependency (and configuration file) in any project that uses ArduinoDriver in order to redirect these log messages to preferred log targets.

## Sample Code Project: Super Mario Bros "Underworld" theme ##

This sample project uses the library above to play this classic retro tune on an Arduino with C#.

One pin of the buzzer should be connected to digital pin 8. The other pin should be connected to GND.

The sample code is configured for an UNO. Don't forget to change the following line if you have another Arduino model attached:

```csharp
// ----------> CHANGE THIS!
private const ArduinoModel AttachedArduino = ArduinoModel.UnoR3;
```
