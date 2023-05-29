//
// Created by lukas on 07/12/2022.
//

#ifndef VERNIERNODE_DEVICESERVICE_H
#define VERNIERNODE_DEVICESERVICE_H

#include "ServiceBase.h"
#include "VernierDevice.h"
#include "EventService.h"

static void
responseNotifyCallback(BLERemoteCharacteristic *pBLERemoteCharacteristic, uint8_t *data, size_t length, bool isNotify);


class VernierAdvertisedDeviceCallbacks : public BLEAdvertisedDeviceCallbacks {
    void onResult(BLEAdvertisedDevice advertisedDevice) final;
};

class VernierClientCallbacks : public BLEClientCallbacks {
    void onDisconnect(BLEClient *pClient) final;

    void onConnect(BLEClient *pClient) final {

    }
};


class DeviceService : public ServiceBase {
public:
    bool scanMode = false;

    void InternalSetup(void *parameters) override {


        BLEDevice::init("SafranekVernierNode");


        bleScan = BLEDevice::getScan();
        bleScan->setAdvertisedDeviceCallbacks(&scanCallbacks);
        bleScan->setActiveScan(true);
    }

    void InternalLoop(void *parameters) override {

        if (scanMode) {
            bleScan->start(5);
        }
        delay(1000);
    }

    void startScan() {
        //TODO: stop all sensors and disconnect devices
        clean();
        scanMode = true;
        StaticService<EventService>::instance.EnqueueEvent(VernierEvent::scanStarted());
    }

    void clean() {
        for (auto const &pair: devicesById) {
            stopSensors(pair.first);
            disconnectFromDevice(pair.first);
        }
    }

    void stopScan() {
        scanMode = false;
        StaticService<EventService>::instance.EnqueueEvent(VernierEvent::scanStopped());
        Serial.println("Scan stopped");
    }

    bool connect(VernierDevice *device) {
        bool success = internalConnect(device);
        if (success) {
            StaticService<EventService>::instance.EnqueueEvent(
                    VernierEvent::deviceConnectSuccess(device->serialId, device));
        } else {
            StaticService<EventService>::instance.EnqueueEvent(
                    VernierEvent::deviceConnectFailed(device->serialId));
        }
        return success;
    }

    bool internalConnect(VernierDevice *device) {
        if (!device->initConnection()) {
            return false;
        }
        devicesByClient[device->_client] = device;
        devicesByResponseCharacteristic[device->_responseCharacteristic] = device;
        device->_responseCharacteristic->registerForNotify(responseNotifyCallback);
        device->_client->setClientCallbacks(&clientCallbacks);

        buffer response;
        InitPacket initPacket;
        if (!device->sendWithResponse(&initPacket, &response)) {
            Serial.println("InitPacket failed!");
            return false;
        } else {
            Serial.println("InitPacket success");
        }

        if (!device->initSensors()) {
            Serial.println("Sensors init failed!!");
            return false;
        }
        Serial.println("Sensors init complete");
        for (int i = 0; i < 32; ++i) {
            if ((device->_availableSensorsMask >> i) & 1) {
                StaticService<EventService>::instance.EnqueueEvent(
                        VernierEvent::sensorInfo(device, device->sensorInfos[i]));
            }
        }

        return true;
    }


    std::map<BLERemoteCharacteristic *, VernierDevice *> devicesByResponseCharacteristic;
    std::map<BLEClient *, VernierDevice *> devicesByClient;
    std::map<uint64_t, VernierDevice *> devicesById;
    BLEScan *bleScan;
    VernierAdvertisedDeviceCallbacks scanCallbacks;
    VernierClientCallbacks clientCallbacks;


    void connectToDevice(uint64_t serialId) {
        VernierDevice *device = devicesById[serialId];
        connect(device);
    }

    void disconnectFromDevice(uint64_t serialId) {
        //TODO: add disconnecting of sensors

        devicesById[serialId]->_client->disconnect();

    }

    void startSensor(uint64_t serialId, uint32_t sensorId) {
        Serial.print("startSensor: ");
        Serial.print(serialId);
        Serial.print("->");
        Serial.println(sensorId);
        //TODO: add start sensor

        VernierDevice *device = devicesById[serialId];


        int i;
        for (i = 0; i < 32; ++i) {
            if (device->sensorInfos[i] != nullptr && device->sensorInfos[i]->id == sensorId) {
                break;
            }
        }

        if (i == 32) {
            Serial.println("Sensor cannot be found, thus cannot be started");
            return;
        }
        Serial.print("Sensor found at index: ");
        Serial.println(i);


        device->setDefaultPeriod();

        bool success = device->startMeasurement(1u << i);

        Serial.print("Result of startSensor: ");
        Serial.println(success);


        StaticService<EventService>::instance.EnqueueEvent(VernierEvent::sensorStarted(serialId, sensorId));
    }

    void stopSensors(uint64_t serialId) {
        Serial.println("stopSensor not implemented!");
        //TODO: add stop sensor

        VernierDevice *device = devicesById[serialId];


        //stop measurement
        device->stopMeasurement();

        StaticService<EventService>::instance.EnqueueEvent(VernierEvent::sensorsStopped(serialId));
    }
};

static void responseNotifyCallback(BLERemoteCharacteristic *pBLERemoteCharacteristic, uint8_t *data, size_t length,
                                   bool isNotify) {
    VernierDevice *device = StaticService<DeviceService>::instance.devicesByResponseCharacteristic[pBLERemoteCharacteristic];

    buffer *responseBuffer = &device->_responseBuffer;

    responseBuffer->write(data, length);
    if (responseBuffer->length > 1 && responseBuffer->length == responseBuffer->data[1]) {
        if (responseBuffer->data[0] == 0x20) {
            MeasurementResponse mesResponse(responseBuffer->data);
            device->HandleMeasurements(&mesResponse);
        } else {
            device->_responses[responseBuffer->data[5]]->write(responseBuffer->data, responseBuffer->length);
        }

        responseBuffer->clear();
    }
}

void VernierAdvertisedDeviceCallbacks::onResult(BLEAdvertisedDevice advertisedDevice) {

    std::string name = advertisedDevice.getName();
    // TODO: handle found devices
    if (name.rfind("GDX", 0) == 0) {
        BLEAddress address = advertisedDevice.getAddress();
        DeviceService *deviceService = &StaticService<DeviceService>::instance;
        uint64_t id;
        uint8_t index = name.find(' ') + 1;
        memcpy(&id, name.c_str() + index, 8);

        char* con = "032005J3";
        //char* con = "031030W5";
        uint64_t con_id = *(uint64_t*)con;

        char* drop = "053000K4";
        uint64_t drop_id = *(uint64_t*)drop;

        if (deviceService->devicesById.find(id) != deviceService->devicesById.end() || (id != con_id && id != drop_id)) {
            //Serial.println("Device already added");
            return;
        }


        Serial.print("Found device: ");
        Serial.println(name.c_str());
        Serial.println(address.toString().c_str());

        VernierDevice *device = new VernierDevice(address, advertisedDevice.getAddressType(), id);
        deviceService->devicesById[device->serialId] = device;
        StaticService<EventService>::instance.EnqueueEvent(VernierEvent::deviceFound(device->serialId));

        Serial.println("Device added");
    }
}

void VernierClientCallbacks::onDisconnect(BLEClient *pClient) {
    Serial.println("Device disconnecting");
    Serial.println("Trying to get device");
    DeviceService *deviceService = &StaticService<DeviceService>::instance;
    VernierDevice *device = deviceService->devicesByClient[pClient];
    if (device == nullptr) {
        Serial.println("Get device failed!");
        return;
    }

    Serial.println("Unregistering from maps");
    deviceService->devicesByResponseCharacteristic.erase(device->_responseCharacteristic);
    deviceService->devicesByClient.erase(device->_client);
    deviceService->devicesById.erase(device->serialId);
    Serial.println("Unregistered from maps");

    Serial.println("Trying to delete BLEClient");
    delete pClient;
    Serial.println("Deleted BLEClient");

    StaticService<EventService>::instance.EnqueueEvent(VernierEvent::deviceDisconnected(device->serialId));

    delete device;
}


#endif //VERNIERNODE_DEVICESERVICE_H
