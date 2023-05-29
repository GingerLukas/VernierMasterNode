#define CONFIG_BLUEDROID_ENABLED 1
#include <Arduino.h>
#include "config.h"
#include "buffer.h"
#include "WiFi.h"
#include "AsyncUDP.h"
#include "Services/SensorService.h"
#include "Services/HeartBeatService.h"
#include "Services/DeviceService.h"
#include "Services/CommandService.h"
#include "Services/EventService.h"


void printLocalTime() {
    tm timeinfo;
    if (!getLocalTime(&timeinfo)) {
        Serial.println("Failed to obtain time");
        return;
    }
    Serial.println(&timeinfo, "%A, %B %d %Y %H:%M:%S");
}

void setup() {
    Serial.begin(115200);
    Serial.println("Vernier BLE node starting...");

    WiFi.mode(WIFI_STA);
    WiFi.begin(ssid, password);
    Serial.printf("Connecting to %s", ssid);
    uint8_t i = 0;
    while (WiFi.status() != WL_CONNECTED) {
        Serial.print(".");
        delay(500);
        i++;
        if ((i % 16) == 0) {
            Serial.print("Status: ");
            Serial.println(WiFi.status());
            Serial.println("Still connecting");
        }
    }
    Serial.println();
    Serial.print("Connected with local address: ");
    Serial.println(WiFi.localIP());

    configTime(gmtOffset_sec, daylightOffset_sec, ntpServer);
    printLocalTime();


    StaticService<HeartBeatService>::Start("HEARTBEAT_SERVICE",8);
    StaticService<SensorService>::Start("SENSOR_SERVICE",6);
    StaticService<DeviceService>::Start("DEVICE_SERVICE");
    StaticService<CommandService>::Start("COMMAND_SERVICE");
    StaticService<EventService>::Start("EVENT_SERVICE");


}

void loop() {

    delay(1000);
}

