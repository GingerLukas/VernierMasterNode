//
// Created by lukas on 08/12/2022.
//

#ifndef VERNIERNODE_COMMANDSERVICE_H
#define VERNIERNODE_COMMANDSERVICE_H

#include "NetworkServiceBase.h"
#include "buffer.h"
#include "StaticService.h"
#include "DeviceService.h"

enum class ECommand : uint8_t {
    Unknown = 0x0,
    StartScan = 0x01, //
    StopScan = 0x02, //
    ConnectToDevice = 0x03, //
    DisconnectFromDevice = 0x04, //
    StartSensor = 0x05,
    StopSensor = 0x06,
};

class CommandService : public NetworkServiceBase<buffer, 4444> {
public:
    void InternalSetup(void *parameters) override {

    }

    void InternalLoop(void *parameters) override {
        EnsureConnection();
        if (_client.available() > 0) {
            uint8_t size = 0;
            _client.read(&size, 1);

            if (size < 2) {
                Serial.println("Got ZERO len packet from command service");
                return;
            }

            //sub size by 'size byte'
            size--;

            uint32_t waitTime = 0;
            //wait for packet to be transmitted whole
            while (_client.available() < size) {
                //TODO: check if packet wait time is not too long
                delay(10);
                waitTime++;
                if (waitTime % 100 == 0){
                    Serial.printf("waiting for %i ms (need %i, got %i)\n", waitTime * 10, size, _client.available());
                }
            }

            uint8_t packet[size];
            _client.read(packet, size);

            handleCommand(packet, size);
        }

        delay(100);
    }


    void handleCommand(uint8_t *packet, uint8_t size) {

        DeviceService *deviceService = &StaticService<DeviceService>::instance;

        //sub first byte which is always command type
        size--;
        switch ((ECommand) packet[0]) {

            default:
            case ECommand::Unknown:
                Serial.println("unknown packet");
                break;
            case ECommand::StartScan:
                Serial.println("startScan packet");
                deviceService->startScan();
                break;
            case ECommand::StopScan:
                Serial.println("stopScan packet");
                deviceService->stopScan();
                break;
            case ECommand::ConnectToDevice:
                Serial.println("connectToDevice packet");
                if (size != 8) {
                    Serial.printf("Command ConnectToDevice: insufficient size of argument, need 8 got %i\n", size);
                    break;
                }
                deviceService->connectToDevice(*(uint64_t *) &packet[1]);
                break;
            case ECommand::DisconnectFromDevice:
                Serial.println("disconnectToDevice packet");
                if (size != 8) {
                    Serial.printf("Command DisconnectFromDevice: insufficient size of argument, need 8 got %i\n", size);
                    break;
                }
                deviceService->disconnectFromDevice(*(uint64_t *) &packet[1]);
                break;
            case ECommand::StartSensor:
                Serial.println("startSensor packet");
                if (size != 12) {
                    Serial.printf("Command StartSensor: insufficient size of argument, need 12 got %i\n", size);
                    break;
                }
                deviceService->startSensor(*(uint64_t *) &packet[1], *(uint32_t *) &packet[9]);
                break;
            case ECommand::StopSensor:
                Serial.println("stopSensor packet");
                if (size != 8) {
                    Serial.printf("Command StopSensor: insufficient size of argument, need 8 got %i\n", size);
                    break;
                }
                deviceService->stopSensors(*(uint64_t *) &packet[1]);
                break;
        }
    }
};

#endif //VERNIERNODE_COMMANDSERVICE_H
