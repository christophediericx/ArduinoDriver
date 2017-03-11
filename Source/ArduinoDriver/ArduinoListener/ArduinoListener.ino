/*
 *
 * ArduinoLibCSharp ArduinoDriver Serial Protocol - Arduino Listener.
 * Version 1.1.
 */

const long BAUD_RATE = 115200;
const unsigned int SYNC_TIMEOUT = 250;
const unsigned int READ_REQUESTPAYLOAD_TIMEOUT = 1000;
const unsigned int SYNC_REQUEST_LENGTH = 4;
const unsigned int CMD_HANDSHAKE_LENGTH = 3;

const byte START_OF_REQUEST_MARKER    = 0xfb;
const byte ALL_BYTES_WRITTEN          = 0xfa;
const byte START_OF_RESPONSE_MARKER   = 0xf9;
const byte ERROR_MARKER               = 0xef;

const unsigned int PROTOCOL_VERSION_MAJOR = 1;
const unsigned int PROTOCOL_VERSION_MINOR = 1;

const byte CMD_HANDSHAKE_INITIATE     = 0x01;
const byte ACK_HANDSHAKE_INITIATE     = 0x02;

const byte CMD_DIGITALREAD            = 0x03;
const byte ACK_DIGITALREAD            = 0x04;
const byte CMD_DIGITALWRITE           = 0x05;
const byte ACK_DIGITALWRITE           = 0x06;
const byte CMD_PINMODE                = 0x07;
const byte ACK_PINMODE                = 0x08;
const byte CMD_ANALOGREAD             = 0x09;
const byte ACK_ANALOGREAD             = 0x0a;
const byte CMD_ANALOGWRITE            = 0x0b;
const byte ACK_ANALOGWRITE            = 0x0c;
const byte CMD_TONE                   = 0x0d;
const byte ACK_TONE                   = 0x0e;
const byte CMD_NOTONE                 = 0x0f;
const byte ACK_NOTONE                 = 0x10;
const byte CMD_ANALOGREFERENCE        = 0x11;
const byte ACK_ANALOGREFERENCE        = 0x12;

byte data[64];
byte commandByte, lengthByte, syncByte, fletcherByte1, fletcherByte2;
unsigned int fletcher16, f0, f1, c0, c1;

unsigned int digitalPinToRead;
unsigned int digitalPinToWrite;
unsigned int digitalPinState;
unsigned int analogPinToRead;
unsigned int analogPinToWrite;
unsigned int analogPinValueToWrite;
unsigned int analogReadResult;
unsigned int analogReferenceType;
unsigned int toneFrequency;
unsigned long toneDuration;

void setup() {
  Serial.begin(BAUD_RATE);
  while (!Serial) { ; }
}

void loop() {

  // Reset variables
  for (int i = 0; i < 64; i++) { data[i] = 0x00; }

  // Try to acquire SYNC. Try to read up to four bytes, and only advance if the pattern matches ff fe fd fc. 
  // Blocking (the client must retry if getting sync fails).
  while (Serial.available() < SYNC_REQUEST_LENGTH) { ; }
  if ((syncByte = Serial.read()) != 0xff) return;
  if ((syncByte = Serial.read()) != 0xfe) return;
  if ((syncByte = Serial.read()) != 0xfd) return; 
  if ((syncByte = Serial.read()) != 0xfc) return;  

  // Write out SYNC ACK (fc fd fe ff).
  Serial.write(0xfc);
  Serial.write(0xfd);
  Serial.write(0xfe);
  Serial.write(0xff);
  Serial.flush();

  // Now expect the START_OF_REQUEST_MARKER (0xfb), followed by our command byte, and a length byte
  // To acknowledge, we will write out the sequence in reverse (length byte, command byte, START_OF_REQUEST_MARKER)
  // We cannot be blocking as the client expects an answer. 
  if (!expectNumberOfBytesToArrive(CMD_HANDSHAKE_LENGTH, SYNC_TIMEOUT)) return;

  if(Serial.read() != START_OF_REQUEST_MARKER) return; 
  commandByte = Serial.read();
  lengthByte = Serial.read();

  // Write out acknowledgement.
  Serial.write(lengthByte);
  Serial.write(commandByte);
  Serial.write(START_OF_REQUEST_MARKER);
  Serial.flush();

  // Read length bytes + 4 (first two bytes are commandByte + length repeated, last two bytes are fletcher16 checksums)
  if (!expectNumberOfBytesToArrive(lengthByte + 4, READ_REQUESTPAYLOAD_TIMEOUT)) return;
  for (int i = 0; i < lengthByte + 4; i++) { data[i] = Serial.read(); }    

  fletcherByte1 = data[lengthByte + 2];
  fletcherByte2 = data[lengthByte + 3];

  // Expect all bytes written package to come in (0xfa), non blocking
  if (!expectNumberOfBytesToArrive(1, READ_REQUESTPAYLOAD_TIMEOUT)) return;
  if(Serial.read() != ALL_BYTES_WRITTEN) return; 

  // Packet checks: do fletcher16 checksum!
  fletcher16 = Fletcher16(data, lengthByte + 2);
  f0 = fletcher16 & 0xff;
  f1 = (fletcher16 >> 8) & 0xff;
  c0 = 0xff - (( f0 + f1) % 0xff);
  c1 = 0xff - (( f0 + c0 ) % 0xff);

  // Sanity check of checksum + command and length values, so that we can trust the entire packet.
  if (c0 != fletcherByte1 || c1 != fletcherByte2 || commandByte != data[0] || lengthByte != data[1]) {
    WriteError();
    return;
  }

  switch (commandByte) {
    case CMD_HANDSHAKE_INITIATE:
       Serial.write(START_OF_RESPONSE_MARKER);
       Serial.write(3);
       Serial.write(ACK_HANDSHAKE_INITIATE);
       Serial.write(PROTOCOL_VERSION_MAJOR);
       Serial.write(PROTOCOL_VERSION_MINOR);
       Serial.flush();
       break;
    case CMD_DIGITALREAD:
       digitalPinToRead = data[2];
       digitalPinState = digitalRead(digitalPinToRead);
       Serial.write(START_OF_RESPONSE_MARKER);
       Serial.write(3);       
       Serial.write(ACK_DIGITALREAD);
       Serial.write(digitalPinToRead); // pin read
       Serial.write(digitalPinState); // pin state
       Serial.flush();
       break;
    case CMD_DIGITALWRITE:
       digitalPinToWrite = data[2];
       digitalPinState = data[3];
       if (digitalPinState == 0) { digitalWrite(digitalPinToWrite, LOW); }
       if (digitalPinState == 1) { digitalWrite(digitalPinToWrite, HIGH); }
       Serial.write(START_OF_RESPONSE_MARKER);
       Serial.write(3);              
       Serial.write(ACK_DIGITALWRITE);
       Serial.write(digitalPinToWrite); // pin written to
       Serial.write(digitalPinState); // pin state
       Serial.flush();
       break;
    case CMD_PINMODE:
       digitalPinToWrite = data[2];
       digitalPinState = data[3];
       if (digitalPinState == 0) { pinMode(digitalPinToWrite, INPUT); }
       if (digitalPinState == 1) { pinMode(digitalPinToWrite, INPUT_PULLUP); }
       if (digitalPinState == 2) { pinMode(digitalPinToWrite, OUTPUT); }
       Serial.write(START_OF_RESPONSE_MARKER);
       Serial.write(3);      
       Serial.write(ACK_PINMODE);
       Serial.write(digitalPinToWrite); // pin written to
       Serial.write(digitalPinState); // mode set
       Serial.flush();
       break;     
    case CMD_ANALOGREAD:
       analogPinToRead = data[2];
       analogReadResult = analogRead(analogPinToRead);
       Serial.write(START_OF_RESPONSE_MARKER);
       Serial.write(4);          
       Serial.write(ACK_ANALOGREAD);
       Serial.write(analogPinToRead); // pin read from
       Serial.write(analogReadResult >> 8);
       Serial.write(analogReadResult & 0xff);
       Serial.flush();
       break;
    case CMD_ANALOGWRITE:
      analogPinToWrite = data[2];
      analogPinValueToWrite = data[3];
      analogWrite(analogPinToWrite, analogPinValueToWrite);
      Serial.write(START_OF_RESPONSE_MARKER);
      Serial.write(3);       
      Serial.write(ACK_ANALOGWRITE);
      Serial.write(analogPinToWrite); // pin written to
      Serial.write(analogPinValueToWrite); // value written
      Serial.flush();
      break;    
    case CMD_TONE:
      digitalPinToWrite = data[2];
      toneFrequency = (data[3] << 8) + data[4];
      toneDuration = (data[5] << 24) + (data[6] << 16) + (data[7] << 8) + data[8];
      if (toneDuration > 0) {
        tone(digitalPinToWrite, toneFrequency, toneDuration);
      } else {
        tone(digitalPinToWrite, toneFrequency);
      }
      Serial.write(START_OF_RESPONSE_MARKER);
      Serial.write(1);        
      Serial.write(ACK_TONE);
      Serial.flush();
      break;
    case CMD_NOTONE:
      digitalPinToWrite = data[2];
      noTone(digitalPinToWrite);
      Serial.write(START_OF_RESPONSE_MARKER);
      Serial.write(1);        
      Serial.write(ACK_NOTONE);
      Serial.flush();
      break;
    case CMD_ANALOGREFERENCE:
      analogReferenceType = data[2];
      if (analogReferenceType == 0) { analogReference(DEFAULT); }
      if (analogReferenceType == 1) { analogReference(3); } // INTERNAL,     Not on Arduino MEGA
      if (analogReferenceType == 2) { analogReference(2); } // INTERNAL1v1,  Arduino MEGA only
      if (analogReferenceType == 3) { analogReference(3); } // INTERNAL2v56, Arduino MEGA only
      if (analogReferenceType == 4) { analogReference(EXTERNAL); }
      Serial.write(START_OF_RESPONSE_MARKER);
      Serial.write(2);
      Serial.write(ACK_ANALOGREFERENCE);
      Serial.write(analogReferenceType);
      Serial.flush();
      break;
    default:
      WriteError();
      break;
  }
}

bool expectNumberOfBytesToArrive(byte numberOfBytes, unsigned long timeout) {
  unsigned long timeoutStartTicks = millis();
  while ((Serial.available() < numberOfBytes) && ((millis() - timeoutStartTicks) < timeout)) { ; }
  if (Serial.available() < numberOfBytes) {
    // Unable to get sufficient bytes, perhaps one was lost in transportation? Write out three error marker bytes.
    WriteError();
    return false;
  }  
  return true;
}

void WriteError() {
  for (int i = 0; i < 3; i++) { Serial.write(ERROR_MARKER); }
  Serial.flush();
}

unsigned int Fletcher16(byte data[], int count ) {
  unsigned int sum1 = 0;
  unsigned int sum2 = 0;
  unsigned int idx;
  for(idx = 0; idx < count; ++idx) {
    sum1 = (sum1 + data[idx]) % 255;
    sum2 = (sum2 + sum1) % 255;
  }
  return (sum2 << 8) | sum1;
}

