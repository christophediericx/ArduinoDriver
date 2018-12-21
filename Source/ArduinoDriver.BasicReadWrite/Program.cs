using System;
using ArduinoDriver;
using ArduinoDriver.SerialProtocol;
using ArduinoUploader.Hardware;
using System.Diagnostics;
using System.Windows.Forms;
using System.Data;


namespace AutoStillDotNet
{

    static class Program
    {


        [STAThread]
        static void Main()
        {

            //Declare the arduino itself -- Note the COM Port and Model Specifications -- If no COM port is specified the program
            //searches for a single available COM Port. If there are multiple devices it must be specified
            var driver = new ArduinoDriver.ArduinoDriver(ArduinoModel.Mega2560, true);
            //var driver = new ArduinoDriver.ArduinoDriver(ArduinoModel.Leonardo, "COM6", true);
            //var driver = new ArduinoDriver.ArduinoDriver(ArduinoModel.NanoR3, "COM5", true);


            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Declare all your things and assign each one a pin number (LEDS, Sensors, Relays etc)
            //Digital Outputs -- LEDS with positive lead connected to pins 22 and 23
            byte LED1 = 22;          
            byte LED2 = 23;         

            //Digital Inputs 
            byte Switch = 24; //A switch connected from 5v or 3.3v to Pin 24

            //Analog Inputs
            byte TempSensor = 54; //Temperature sensor
            byte VacuumSensor = 55; // Vacuum sensor (Or any resistance based sensor really)


            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Setup the Arduino itself by setting the pinmodes
            //Digital outputs
            driver.Send(new PinModeRequest(LED1, PinMode.Output));
            driver.Send(new PinModeRequest(LED2, PinMode.Output));

            //Digial inputs
            driver.Send(new PinModeRequest(Switch, PinMode.Input));

            //Sensor Inputs
            driver.Send(new PinModeRequest(TempSensor, PinMode.Input));
            driver.Send(new PinModeRequest(VacuumSensor, PinMode.Input));


            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Tell the arduino to read and write           
            driver.Send(new DigitalWriteRequest(LED1, DigitalValue.High)); //Turn LED 1 and 2 On
            driver.Send(new DigitalWriteRequest(LED2, DigitalValue.High));

            //Read from the sensors and write the value to a variable
            string Temperature = driver.Send(new AnalogReadRequest(TempSensor)).PinValue.ToString();
            string Pressure = driver.Send(new AnalogReadRequest(VacuumSensor)).PinValue.ToString();

            //Use a switch connected from 5v to Pin24 to turn off LED2            
            while (driver.Send(new DigitalReadRequest(Switch)).PinValue.ToString() == "Low")
            {
                System.Threading.Thread.Sleep(1000);
            }
            driver.Send(new DigitalWriteRequest(LED2, DigitalValue.Low));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
