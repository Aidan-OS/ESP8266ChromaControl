#include <ESP8266WebServer.h>
#include <WiFiClient.h>
#include <Adafruit_NeoPixel.h>

const char* ssid = "SSID";            //CHANGE ME
const char* pass = "PASSWORD";        //CHANGE ME

int SerialCommunicationSpeed = 9600;  //CHANGE ME (IF NEEDED)

#define LEDPIN 2                      //CHANGE ME (IF NEEDED)
#define NUMPIXELS 25                  //CHANGE ME

String rs = "r";
String gs = "g";
String bs = "b";

#define HTTP_REST_PORT 8080
ESP8266WebServer httpRestServer(HTTP_REST_PORT);

Adafruit_NeoPixel pixels (NUMPIXELS, LEDPIN, NEO_GRB + NEO_KHZ800);

void handleLED()
{
  Serial.println("LED Request Received");
  int zoneSize = NUMPIXELS / 5;

  for (int i = 0; i < 5; i++)
  {
    char r[4];
    char g[4];
    char b[4];
    int k = i + 1;

    httpRestServer.arg(rs + k).toCharArray(r, 4);
    httpRestServer.arg(gs + k).toCharArray(g, 4);
    httpRestServer.arg(bs + k).toCharArray(b, 4);

    uint32_t colour = pixels.Color(atoi(r), atoi(g), atoi(b));

    pixels.fill(colour, i * zoneSize, zoneSize);
  }

  pixels.show();
  httpRestServer.send(200, "text/plain", "Success");
}

void restServerRouting()
{
  httpRestServer.on("/LED", HTTP_POST, handleLED);
}
 
void setup()
{
  Serial.begin(SerialCommunicationSpeed);

  pixels.begin();
  pixels.fill(pixels.Color(0, 0, 0), 0, NUMPIXELS - 1);
  pixels.show();
  
  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, pass);

  int count = 0;
  while(WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
    if(count > 10)
    {
      Serial.println("\nFailed to Connect");
      return;
    }
    count++;
  }
  Serial.println("");
  Serial.print("Connected to ");
  Serial.println(ssid);
  Serial.print("IP address: ");
  Serial.println(WiFi.localIP());
  
  restServerRouting();
  httpRestServer.begin();
}

void loop()
{
  httpRestServer.handleClient();
}
