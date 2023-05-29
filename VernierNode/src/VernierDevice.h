#ifndef VERNIERDEVICE_H
#define VERNIERDEVICE_H
#define CONFIG_BLUEDROID_ENABLED 1

#include "config.h"
#include "Arduino.h"
#include "buffer.h"
#include "BLEDevice.h"
#include "Packets.h"
#include "MeasurementResponse.h"
#include "SensorInfo.h"

#define COMMAND_UUID "f4bf14a6-c7d5-4b6d-8aa8-df1a7c83adcb"
#define RESPONSE_UUID "b41e6675-a329-40e0-aa01-44d2f444babe"
#define SERVICE_UUID "d91714ef-28b9-4f91-ba16-f0d9a604f112"

static BLEUUID commandUUID(COMMAND_UUID);
static BLEUUID responseUUID(RESPONSE_UUID);
static BLEUUID serviceUUID(SERVICE_UUID);


struct VernierDevice {

public:


    // HEAP
    SensorInfo *sensorInfos[32];

    // GET
    BLEClient *getClient() const { return _client; }

    bool isConnected() const { return _connected; }

    uint32_t enabledSensors() const { return _enabledSensorsMask; }


    uint64_t serialId;

    VernierDevice(BLEAddress address,esp_ble_addr_type_t addressType, uint64_t id) {
        _addressType = addressType;
        esp_bd_addr_t *native = address.getNative();
        memcpy(_nativeAddress, native, ESP_BD_ADDR_LEN);
        serialId = id;

        for (auto & sensorInfo : sensorInfos) {
            sensorInfo = nullptr;
        }
    }

    ~VernierDevice() {
        for (size_t i = 0; i < 32; i++) {
            if (_enabledSensorsMask >> i & 1) {
                delete sensorInfos[i];
            }
        }
    }


    bool begin();

    bool setDefaultPeriod() {
        return setMeasurementPeriod(_defaultPeriod);
    }

    bool startDefaultSensors() {
        return startMeasurement(_defaultSensorsMask);
    }

    bool startAvailableSensors() {
        return startMeasurement(_availableSensorsMask);
    }


//private:
    esp_bd_addr_t _nativeAddress;
    esp_ble_addr_type_t _addressType;

    BLEClient *_client = nullptr;
    BLERemoteService *_service = nullptr;
    BLERemoteCharacteristic *_commandCharacteristic = nullptr;
    BLERemoteCharacteristic *_responseCharacteristic = nullptr;

    uint32_t _availableSensorsMask = 0;
    uint32_t _defaultSensorsMask = 0;
    uint32_t _enabledSensorsMask = 0;

    uint32_t _defaultPeriod = 0xFFFFFFFF;

    bool _connected = false;

    buffer _responseBuffer;
    std::map<uint8_t, buffer *> _responses;

    bool setMeasurementPeriod(uint32_t period);

    bool startMeasurement(uint32_t mask);

    bool stopMeasurement();

    void HandleMeasurements(MeasurementResponse *response);

    bool initSensors();

    bool initConnection();

    bool sendWithResponse(PacketBase *packet, buffer *response);

    void sendPacket(PacketBase *packet);


};


#endif
