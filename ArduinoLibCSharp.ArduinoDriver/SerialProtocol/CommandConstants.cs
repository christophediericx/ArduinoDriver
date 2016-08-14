namespace ArduinoLibCSharp.ArduinoDriver.SerialProtocol
{
    public static class  CommandConstants
    {
        public static readonly byte[] SyncBytes     = { 0xff, 0xfe, 0xfd, 0xfc };
        public static readonly byte[] SyncAckBytes  = { 0xfc, 0xfd, 0xfe, 0xff };

        public const byte StartOfMessage            = 0xFB;
        public const byte AllBytesWritten           = 0xFA;
        public const byte StartOfResponseMarker     = 0xF9;
        public const byte Error                     = 0xEF;

        public const byte HandshakeInitiate         = 0x01;
        public const byte HandshakeAck              = 0x02;
        public const byte DigitalRead               = 0x03;
        public const byte DigitalReadAck            = 0x04;
        public const byte DigitalWrite              = 0x05;
        public const byte DigitalWriteAck           = 0x06;
        public const byte PinMode                   = 0x07;
        public const byte PinModeAck                = 0x08;
        public const byte AnalogRead                = 0x09;
        public const byte AnalogReadAck             = 0x0A;
        public const byte AnalogWrite               = 0x0B;
        public const byte AnalogWriteAck            = 0x0C;
        public const byte Tone                      = 0x0D;
        public const byte ToneAck                   = 0x0E;
        public const byte NoTone                    = 0x0F;
        public const byte NoToneAck                 = 0x10;
    }
}
