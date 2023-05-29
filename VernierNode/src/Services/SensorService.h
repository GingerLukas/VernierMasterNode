//
// Created by lukas on 07/12/2022.
//

#ifndef VERNIERNODE_SENSORSERVICE_H
#define VERNIERNODE_SENSORSERVICE_H

#include "NetworkServiceBase.h"
#include "StaticService.h"
#include "VernierDevice.h"
#include "DeviceService.h"

class SensorService : public NetworkServiceBase<large_buffer, 2224> {
public:
    void InternalSetup(void *parameters) override {
    }

    void InternalLoop(void *parameters) override {

        unsigned long time = millis();
        EnsureConnection();

        //allocate space for final length
        _buffer.length += 4;

        //allocate space for device count
        _buffer.length += 1;

        uint8_t deviceCount = 0;

        for (auto const &nd: StaticService<DeviceService>::instance.devicesById) {
            VernierDevice *device = nd.second;


            //allocate space for device address
            uint8_t addressLocation = _buffer.length;
            _buffer.length += 8;

            //allocate space for sensor count
            uint8_t sum = 0;
            _buffer.length += 1;

            uint32_t mask = device->enabledSensors();
            for (int i = 0; i < 32; i++) {
                if (!((mask >> i) & 1)) continue;
                SensorInfo *info = device->sensorInfos[i];
                info->values_buffer.lock();
                buffer *buff = info->values_buffer.swap();
                info->values_buffer.unlock();
                if (buff->length == 0) continue;

                sum++;
                _buffer.write(&info->id, 4);
                _buffer.write(info->isInts);
                _buffer.write(buff->length / 4);
                _buffer.write(buff->data, buff->length);
                buff->clear();
            }
            if (sum > 0) {
                uint32_t pos = _buffer.length;
                _buffer.length = addressLocation;
                _buffer.write(&device->serialId, 8);
                _buffer.write(sum);
                _buffer.length = pos;
                deviceCount++;
            } else {
                //delete allocated space for address
                _buffer.length -= 8;
                //delete allocated space for sensor count
                _buffer.length -= 1;
            }
        }
        if (deviceCount > 0) {
            uint32_t totalLen = _buffer.length;
            _buffer.length = 0;
            _buffer.write(&totalLen, 4);
            _buffer.write(deviceCount);
            _buffer.length = totalLen;


            _client.write(_buffer.data, _buffer.length);


            Serial.print("buffer len: ");
            Serial.println(_buffer.length);

        }
        time = millis() - time;

/*
        Serial.print("SensorLoop finished in ");
        Serial.print(time);
        Serial.println("ms");
        */


        delay(min((uint32_t) 100, (uint32_t)max(100 - (int32_t) time, (int32_t) 0)));
    }
};


#endif //VERNIERNODE_SENSORSERVICE_H
