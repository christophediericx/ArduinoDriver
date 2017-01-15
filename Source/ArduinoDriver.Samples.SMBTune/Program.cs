using System.Threading;
using ArduinoDriver.SerialProtocol;
using ArduinoUploader.Hardware;

namespace ArduinoDriver.Samples.SMBTune
{
    /// <summary>
    /// This program uses the ArduinoLibCSharp libraries to play the Super Mario Bros *Underworld* tune on a connected Arduino.
    /// 
    /// Since the constructor for the ArduinoDriver is passed 'true' for autobootstrap, this will automatically deploy the listener
    /// on the Arduino itself.
    /// 
    /// The only thing that should be changed in this program is the Arduino "model".
    /// 
    /// One pin of the buzzer should be connected to digital pin 8 ("DigitalPinBuzzer").
    /// The other pin should be connected to GND.
    /// 
    /// Credit for tones and tempo: http://www.linuxcircle.com/2013/03/31/playing-mario-bros-tune-with-arduino-and-piezo-buzzer/
    /// </summary>
    internal class Program
    {
        // ----------> CHANGE THIS!
        private const ArduinoModel AttachedArduino = ArduinoModel.UnoR3;

        private const int DigitalPinBuzzer = 8;

        private const int D3 = 147;
        private const int DS3 = 156;
        private const int E3 = 165;
        private const int F3 = 175;
        private const int G3 = 196;
        private const int GS3 = 208;
        private const int A3 = 220;
        private const int AS3 = 233;
        private const int B3 = 247;
        private const int C4 = 262;
        private const int CS4 = 277;
        private const int D4 = 294;
        private const int DS4 = 311;
        private const int F4 = 349;
        private const int FS4 = 370;
        private const int GS4 = 415;
        private const int A4 = 440;
        private const int AS4 = 466;
        private const int E5 = 523;

        private static readonly int[] melody = 
        {
          C4, E5, A3, A4, AS3, AS4, 0, 0, C4, E5, A3, A4, AS3, AS4, 0, 0, F3, F4, D3, D4, DS3, DS4, 0, 0, F3, F4, D3, D4, DS3, DS4, 0,
          0, DS4, CS4, D4, CS4, DS4, DS4, GS3, G3, CS4, C4, FS4, F4, E3, AS4, A4, GS4, DS4, B3, AS3, A3, GS3, 0, 0, 0
        };

        private static readonly int[] tempo = 
        {
          12, 12, 12, 12, 12, 12, 6, 3, 12, 12, 12, 12, 12, 12, 6, 3, 12, 12, 12, 12, 12, 12, 6, 3, 12, 12, 12, 12, 12, 12, 6,
          6, 18, 18, 18, 6, 6, 6, 6, 6, 6, 18, 18, 18, 18, 18, 18, 10, 10, 10, 10, 10, 10, 3, 3, 3 
        };

        private static void Main(string[] args)
        {
            using (var driver = new ArduinoDriver(AttachedArduino, true))
            {
                for (var i = 0; i < melody.Length; i++)
                {
                    var noteDuration = 1000 / tempo[i];
                    driver.Send(new ToneRequest(DigitalPinBuzzer, (ushort) melody[i], (uint) noteDuration));
                    Thread.Sleep((int) (noteDuration * 1.40));
                    driver.Send(new NoToneRequest(DigitalPinBuzzer));
                }
            }
        }
    }
}