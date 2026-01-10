#include <M5Cardputer.h>
#include <WiFi.h>
#include <WiFiUdp.h>

// Tactical Configuration
const char* SSID = "AEGIS_LINK_NET";
const char* PASS = "TACTICAL_OPS";
const int LOC_PORT = 5005;
const int REM_PORT = 5005;
const uint8_t TACTICAL_KEY = 0x42;

WiFiUDP udp;
bool isLinked = false;

#pragma pack(push, 1)
struct TelemetryFrame {
    int32_t BatteryLevel;    // 4 bytes
    float SignalStrength;    // 4 bytes
    double Latitude;         // 8 bytes
    double Longitude;        // 8 bytes
    uint32_t StatusCodes;    // 4 bytes
};
#pragma pack(pop)

void setup() {
    M5.begin();
    M5.Display.setRotation(1);
    M5.Display.setTextSize(2);
    M5.Display.setTextColor(ORANGE);
    M5.Display.println("INIT SYSTEM...");

    WiFi.begin(SSID, PASS);
    // In real life we'd wait for connection, but for sim logic we proceed
    // while (WiFi.status() != WL_CONNECTED) delay(500);

    udp.begin(LOC_PORT);
    M5.Display.println("RADIO SILENCE");
}

void loop() {
    M5.update();
    
    // Check for Challenge
    int packetSize = udp.parsePacket();
    if (packetSize > 0) {
        uint8_t challengeByte = udp.read();
        uint8_t response = challengeByte ^ TACTICAL_KEY;
        
        udp.beginPacket(udp.remoteIP(), udp.remotePort());
        udp.write(&response, 1);
        udp.endPacket();
        
        isLinked = true;
        M5.Display.fillScreen(BLACK);
        M5.Display.setCursor(0,0);
        M5.Display.setTextColor(CYAN);
        M5.Display.println("LINK: SECURE");
    }
    
    if (isLinked) {
        TelemetryFrame frame;
        frame.BatteryLevel = M5.Power.getBatteryLevel();
        frame.SignalStrength = WiFi.RSSI();
        // Simulating Movement
        frame.Latitude = 120.5 + (millis() * 0.00001); 
        frame.Longitude = 30.2 + (millis() * 0.00001);
        
        // 'G' Key for FIRE command status
        if (M5.Keyboard.isKeyPressed('G')) {
            frame.StatusCodes = 0xFF; // FIRE STATUS
            M5.Display.println("STATUS: FIRE");
        } else {
            frame.StatusCodes = 0x00; // NORMAL
        }
        
        udp.beginPacket(udp.remoteIP(), REM_PORT);
        udp.write((uint8_t*)&frame, sizeof(TelemetryFrame));
        udp.endPacket();
    }
    
    delay(100);
}
