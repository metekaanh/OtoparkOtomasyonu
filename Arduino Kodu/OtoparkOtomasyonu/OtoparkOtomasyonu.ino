#include<Servo.h>

Servo servo1;
#include <SPI.h>

#include <MFRC522.h>

#define DEBUG 0
#ifdef DEBUG
#define DEBUG_PRINT(x)  Serial.println(x)
#else
#define DEBUG_PRINT(x)
#endif

int aci;
char c;
const int trigPin = 46,
          echoPin = 47;
long mesafe;
long sure;
int aracNo = -1;
int ledcnt=4;

// PIN Numbers : RESET + SDAs
#define RST_PIN 5
#define SS_1_PIN 31
#define SS_2_PIN 33
#define SS_3_PIN 35
#define SS_4_PIN 37

// Led PINS
int rfidk[] = {22, 24, 26, 28};
int rfidy[] = {30, 32, 36, 34};
int rfidm[] = {38, 40, 42, 43};

int IDS[] = {107, 11, 219, 91};
#define NR_OF_READERS 4
byte ssPins[] = {
  SS_1_PIN,
  SS_2_PIN,
  SS_3_PIN,
  SS_4_PIN
};
MFRC522 mfrc522[NR_OF_READERS];
void setup() {
  servo1.attach(3);
  pinMode(trigPin, OUTPUT);
  pinMode(echoPin, INPUT);
  Serial.begin(9600); // Initialize serial communications with the PC
  while (!Serial); // Do nothing if no serial port is opened (added for Arduinos based on ATMEGA32U4)

  SPI.begin(); // Init SPI bus

  /* Initializing Inputs and Outputs */
  pinMode(46, OUTPUT);
  digitalWrite(46, LOW);
  pinMode(38, OUTPUT);
  digitalWrite(38, LOW);
  pinMode(36, OUTPUT);
  digitalWrite(36, LOW);
  for(int p=0; p<ledcnt; p++)
   {
       pinMode(rfidk[p], OUTPUT); // Set the mode to OUTPUT
   }
  for(int p=0; p<ledcnt; p++)
   {
       pinMode(rfidy[p], OUTPUT); // Set the mode to OUTPUT
   }
  for(int p=0; p<ledcnt; p++)
   {
       pinMode(rfidm[p], OUTPUT); // Set the mode to OUTPUT
   }

  /* looking for MFRC522 readers */
#ifdef DEBUG
  for (uint8_t reader = 0; reader < NR_OF_READERS; reader++) {
    mfrc522[reader].PCD_Init(ssPins[reader], RST_PIN);
    Serial.print(F("Reader "));
    Serial.print(reader);
    Serial.print(F(": "));
    mfrc522[reader].PCD_DumpVersionToSerial();
    //mfrc522[reader].PCD_SetAntennaGain(mfrc522[reader].RxGain_max);
  }
#endif

}
void parkyeri()
{
  for (uint8_t reader = 0; reader < NR_OF_READERS; reader++) {

    // Looking for new cards
    int newCardPresent = mfrc522[reader].PICC_IsNewCardPresent();
    int readSerialCard = mfrc522[reader].PICC_ReadCardSerial();
    if (newCardPresent && readSerialCard)
    {
#ifdef DEBUG
      Serial.print("Reader ");
      Serial.println(reader);
      Serial.println((int)*mfrc522[reader].uid.uidByte);//rfid nosunu ekrana bas
#endif
      int gelenAraba = (int) * mfrc522[reader].uid.uidByte; //gelen araba idsi
      if (gelenAraba == IDS[reader])//arac dogru yerdeyse calisir
      {
        digitalWrite(rfidk[reader], LOW);
        digitalWrite(rfidy[reader],HIGH);
        switch (reader) {
          case 0:
            
            break;
          case 2:
            
            break;
          default:
            break;
        }
      }

      else
      {
        digitalWrite(rfidm[reader], HIGH);
        DEBUG_PRINT("Yanlış Plaka");
      }
      *mfrc522[reader].uid.uidByte = 0;
    }
  }

}

void dump_byte_array(byte * buffer, byte bufferSize) {
  for (byte i = 0; i < bufferSize; i++) {
    Serial.print(buffer[i] < 0x10 ? " 0" : " ");
    Serial.print(buffer[i], HEX);
  }
}
void loop() {


  if (Serial.available())
  {
    c = Serial.read();
    if (c == 'e') {

      digitalWrite(trigPin, LOW);
      delayMicroseconds(2);
      digitalWrite(trigPin, HIGH);
      delayMicroseconds(10);

      digitalWrite(trigPin, LOW);
      sure = pulseIn(echoPin, HIGH); //süreyi hesaplıyoruz
      mesafe = sure / 58.2; //mesafeyi hesaplıyoruz
      Serial.println(mesafe);
    }
    else if (c == 'b')
    {
      aracNo = Serial.read() - '0';
      DEBUG_PRINT(aracNo);
      for (aci = 90; aci >= 0; aci -= 5) {
        servo1.write(aci);
      }
      if (aracNo >= 0) {
        digitalWrite(rfidk[aracNo], HIGH);
        aracNo = -1;
      }
    }
    else if (c == 'c') {
      for (aci = 0; aci <= 90; aci += 10) {
        servo1.write(aci);
      }
    }
  }
  parkyeri();

}
