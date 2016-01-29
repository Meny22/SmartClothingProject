void setup() {
  pinMode(2,OUTPUT);
  pinMode(3,OUTPUT);
  pinMode(4,OUTPUT);
  pinMode(5,OUTPUT);
  pinMode(6,OUTPUT);
  pinMode(7,OUTPUT);
  pinMode(8,OUTPUT);
  pinMode(9,OUTPUT);
  pinMode(10,OUTPUT);
  pinMode(11,OUTPUT);
  pinMode(12,OUTPUT);
  pinMode(13,OUTPUT);
  Serial.begin(9600);
}

void loop() {
  if(Serial.available() > 0) {
    int message = Serial.parseInt();
    Serial.print(message);
    if(message == 16 || message == 17) {
      activateSide(message);
    }
    activateOne(message);
  }
}

int randomVibrations() {
  return random(1000,5000);
}

void activateOne(int message) {
   if(message != 14 && message != 15) {
     digitalWrite(message,HIGH);
     delay(100);
     digitalWrite(message,LOW);
   } else {
     analogWrite(message,255);
     delay(100);
     analogWrite(message,0);
   }
}

void activateSide(int message) {
  if(message == 17) {
    digitalWrite(6,HIGH);
    digitalWrite(8,HIGH);
    digitalWrite(11,HIGH);
    digitalWrite(13,HIGH);
    digitalWrite(14,HIGH);
    digitalWrite(15,HIGH);
    delay(500);
    clearAll();
  } else if(message == 16) {
    digitalWrite(3,HIGH);
    digitalWrite(5,HIGH);
    digitalWrite(7,HIGH);
    digitalWrite(9,HIGH);
    digitalWrite(10,HIGH);
    digitalWrite(12,HIGH);
    delay(500);
    clearAll();
  }
}


void activateAll() {
  digitalWrite(3,HIGH);
  digitalWrite(4,HIGH);
  digitalWrite(5,HIGH);
  digitalWrite(6,HIGH);
  digitalWrite(7,HIGH);
  digitalWrite(8,HIGH);
  digitalWrite(9,HIGH);
  digitalWrite(10,HIGH);
  digitalWrite(11,HIGH);
  digitalWrite(12,HIGH);
  digitalWrite(13,HIGH);
  analogWrite(A0,255);
  analogWrite(A1,255);
}

void activateChest() {
  //digitalWrite(12,HIGH);
  digitalWrite(13,HIGH);
}

void clearChest() {
  //digitalWrite(12,LOW);
  digitalWrite(13,LOW);
}

void clearAll() {
  digitalWrite(3,LOW);
  digitalWrite(4,LOW);
  digitalWrite(5,LOW);
  digitalWrite(6,LOW);
  digitalWrite(7,LOW);
  digitalWrite(8,LOW);
  digitalWrite(9,LOW);
  digitalWrite(10,LOW);
  digitalWrite(11,LOW);
  digitalWrite(12,LOW);
  digitalWrite(13,LOW);
  analogWrite(A0,0);
  analogWrite(A1,0);
}

