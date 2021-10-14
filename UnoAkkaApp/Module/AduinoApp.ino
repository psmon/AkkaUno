#include <MsTimer2.h>
#include <Wire.h>
#include <LiquidCrystal_I2C.h>

LiquidCrystal_I2C lcd(0x27,16,2);

const int pinREG = A1;

//응용
int registerVal = -1;

int lampStart = 2;
int lampCount = 9;

float temperature;  
int reading;  
int lm35Pin = A0;

void setup() { 
  analogReference(INTERNAL);
  Serial.begin(9600);
  Serial.println("=========== app setup ===============");

  //pinMode(pinREG,INPUT);
  
  pinMode(2, OUTPUT);
  pinMode(3, OUTPUT);
  pinMode(4, OUTPUT);
  pinMode(5, OUTPUT);
  pinMode(6, OUTPUT);
  pinMode(7, OUTPUT);
  pinMode(8, OUTPUT);
  pinMode(9, OUTPUT);
  pinMode(10, OUTPUT);

  lcd.init();  // LCD초기 설정
  lcd.backlight(); // LCD초기 설정
  lcd.setCursor(0,0);
  lcd.print("hello");  
  
  MsTimer2::set(100, onTimer);
  MsTimer2::start();  
  
}


void lampUpDown(int outPin){
  Serial.print("read :");
  Serial.println(outPin);
  int pinNum = outPin+1;    
  digitalWrite(outPin+1, HIGH);
}

void lampAllDown(){
  digitalWrite(2, LOW); 
  digitalWrite(3, LOW); 
  digitalWrite(4, LOW); 
  digitalWrite(5, LOW); 
  digitalWrite(6, LOW); 
  digitalWrite(7, LOW); 
  digitalWrite(8, LOW); 
  digitalWrite(9, LOW); 
  digitalWrite(10, LOW); 
}

void displayRegister(){  
  int curval = (analogRead(pinREG)/100)*100;
  if(curval!=registerVal){            
      lcd.setCursor(0,0);      
      lcd.print(curval);
      lcd.print(" :======");      
  }  
  registerVal = curval;
}

void serialLamp(){
  char input = Serial.read();

  switch(input){
    case '1':
      lampUpDown(1);
      break;
    case '2':
      lampUpDown(2);
      break;
    case '3':
      lampUpDown(3);
      break;
    case '4':
      lampUpDown(4);
      break;
    case '5':
      lampUpDown(5);
      break;
    case '6':
      lampUpDown(6);
      break;
    case '7':
      lampUpDown(7);
      break;
    case '8':
      lampUpDown(8);
      break;
    case '9':
      lampUpDown(9);
      break;
    default:
      lampAllDown();
      break;
  }
}

void loop() {
  displayRegister();
}

void onTimer(){
  serialLamp();
}