//
// Created by lukas on 08/12/2022.
//

#ifndef VERNIERNODE_VERNIEREVENT_H
#define VERNIERNODE_VERNIEREVENT_H

enum class EVernierEvent : uint8_t {
    Unknown = 0x00,
    ScanStarted = 0x01,//
    ScanStopped = 0x02,//
    DeviceConnectionSuccess = 0x03,//
    DeviceConnectionFailed = 0x04,//
    DeviceDisconnected = 0x05,//
    DeviceFound = 0x06,//
    SensorStarted = 0x07,//
    SensorStopped = 0x08,//
    SensorInfo = 0x09,//
};

struct VernierEvent {
    buffer _buffer;

    static VernierEvent *scanStarted() {
        VernierEvent *event = new VernierEvent();
        event->_buffer.write(static_cast<uint8_t>(EVernierEvent::ScanStarted));
        return event;
    }

    static VernierEvent *scanStopped() {
        VernierEvent *event = new VernierEvent();
        event->_buffer.write(static_cast<uint8_t>(EVernierEvent::ScanStopped));
        return event;
    }

    static VernierEvent *deviceConnectSuccess(uint64_t serialId, VernierDevice *device) {
        VernierEvent *event = new VernierEvent();
        event->_buffer.write(static_cast<uint8_t>(EVernierEvent::DeviceConnectionSuccess));
        event->_buffer.write(&serialId, 8);
        event->_buffer.write(&device->_availableSensorsMask, 4);
        for (int i = 0; i < 32; ++i) {
            if (((device->_availableSensorsMask >> i) & 1) == 1) {
                event->_buffer.write(&(device->sensorInfos[i]->id), 4);
            }
        }
        return event;
    }

    static VernierEvent *deviceConnectFailed(uint64_t serialId) {
        VernierEvent *event = new VernierEvent();
        event->_buffer.write(static_cast<uint8_t>(EVernierEvent::DeviceConnectionFailed));
        event->_buffer.write(&serialId, 8);
        return event;
    }

    static VernierEvent *deviceDisconnected(uint64_t serialId) {
        VernierEvent *event = new VernierEvent();
        event->_buffer.write(static_cast<uint8_t>(EVernierEvent::DeviceDisconnected));
        event->_buffer.write(&serialId, 8);
        return event;
    }

    static VernierEvent *deviceFound(uint64_t serialId) {
        VernierEvent *event = new VernierEvent();
        event->_buffer.write(static_cast<uint8_t>(EVernierEvent::DeviceFound));
        event->_buffer.write(&serialId, 8);
        return event;
    }



    static VernierEvent *sensorStarted(uint64_t serialId, uint32_t sensorId) {
        VernierEvent *event = new VernierEvent();
        event->_buffer.write(static_cast<uint8_t>(EVernierEvent::SensorStarted));
        event->_buffer.write(&serialId, 8);
        event->_buffer.write(&sensorId, 4);
        return event;
    }

    static VernierEvent *sensorsStopped(uint64_t serialId) {
        VernierEvent *event = new VernierEvent();
        event->_buffer.write(static_cast<uint8_t>(EVernierEvent::SensorStopped));
        event->_buffer.write(&serialId, 8);
        return event;
    }


    static VernierEvent *sensorInfo(VernierDevice *device, SensorInfo *sensor) {
        VernierEvent *event = new VernierEvent();
        event->_buffer.write(static_cast<uint8_t>(EVernierEvent::SensorInfo));
        event->_buffer.write(&device->serialId, 8);

        event->_buffer.write(sensor->isInts);
        event->_buffer.write(sensor->number);
        event->_buffer.write(sensor->spareByte);
        event->_buffer.write(&sensor->id,4);
        event->_buffer.write(sensor->numberMeasType);
        event->_buffer.write(sensor->samplingMode);
        event->_buffer.write(sensor->description,60);
        event->_buffer.write(sensor->unit,32);
        event->_buffer.write(&sensor->measurementUncertainty,8);
        event->_buffer.write(&sensor->minMeasurement,8);
        event->_buffer.write(&sensor->maxMeasurement,8);
        event->_buffer.write(&sensor->maxMeasurementPeriod,4);
        event->_buffer.write(&sensor->minMeasurementPeriod,8);
        event->_buffer.write(&sensor->typicalMeasurementPeriod,4);
        event->_buffer.write(&sensor->measurementPeriodGranularity,4);
        event->_buffer.write(&sensor->mutualExclusionMask,4);


        return event;
    }


};

#endif //VERNIERNODE_VERNIEREVENT_H
