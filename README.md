# ArduinoLibCSharp
ArduinoLibCSharp is a set of libaries and programs for controlling an Arduino board directly with C# / Visual Studio.

![ArduinoLibCSharp](https://github.com/christophediericx/ArduinoLibCSharp/blob/master/Images/ArduinoLibCSharp-header-color.png)

It consists of the following .NET libraries / assemblies:

  * **ArduinoDriver**: A library to easily connect, drive (and debug) an Arduino from within Visual Studio (through a highly *syntax compatible* implementation running over the serial connection - at high speed). 
  
  [(View Documentation)](Documentation/ArduinoDriver.md) [(nuget package)](https://www.nuget.org/packages/ArduinoLibCSharp.ArduinoDriver/)
  * **ArduinoUploader**: A library to upload an Intel HEX file (e.g. compiled sketches) directly to an Arduino over a (USB) serial port without having to use the Arduino IDE (or avrdude).
  
  [(View Documentation)](Documentation/ArduinoUploader.md) [(nuget package)](https://www.nuget.org/packages/ArduinoLibCSharp.ArduinoUploader/)
  * **IntelHexFormatReader**: A library to parse the contents of [an Intel HEX file](https://en.wikipedia.org/wiki/Intel_HEX) into a representative "memory representation".
  
and the following command line applications:

  * **ArduinoSketchUploader**: A command line utility to flash a HEX file directly to an Arduino (UNO) board.
  * **Samples.SMBTune**: A sample project that uses the above libraries to play a retro tune ("Underworld Theme") on the Arduino directly from a C# program.

> *Compatibility note: This library has only been tested with UNO based Arduino boards. It is expected that tweaking of hardware constants in the STK-500 bootloader communication is required in order to support other architectures.*

