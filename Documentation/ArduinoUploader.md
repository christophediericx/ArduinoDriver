[<- Back to HOME](../../../)

### ArduinoUploader (library) ###

The *ArduinoUploader* library talks to the Arduino's bootloader directly through a dialect of the STK-500 protocol in order to flash the memory on the device with the contents of an Intel HEX file.

The solution comes with a seperate [ArduinoSketchUploader](ArduinoSketchUploader.md) command line utility which can be used to upload compiled sketch programs to the Arduino directly without requiring interaction from the Arduino IDE. It's fully self-contained and not just a wrapper for avrdude.
