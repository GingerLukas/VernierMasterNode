//
// Created by lukas on 07/12/2022.
//
#include "VernierDevice.h"

bool VernierDevice::initConnection() {
    Serial.println("initConnection started");

    int i = 0;
    while (!_connected && i < 3) {
        delete _client;
        _client = BLEDevice::createClient();
        _connected = _client->connect(BLEAddress(_nativeAddress), _addressType);
        i++;
    }
    if (!_connected) {
        Serial.println("Connect failed!");
        return false;
    }
    Serial.println("Connected to device");

    _service = _client->getService(serviceUUID);

    if (_service == nullptr) {
        Serial.println("Failed to get service!!");
        return false;
    }

    _commandCharacteristic = _service->getCharacteristic(commandUUID);
    _responseCharacteristic = _service->getCharacteristic(responseUUID);

    if (_commandCharacteristic == nullptr || _responseCharacteristic == nullptr) {
        Serial.println("Failed to get characteristics!");
        return false;
    }


    return true;
}

bool VernierDevice::sendWithResponse(PacketBase *packet, buffer *response) {
    uint8_t id = packet->getId();
    response->clear();
    _responses[id] = response;

    sendPacket(packet);

    uint32_t timeSpent = 0;
    const uint32_t timeDelay = 10;
    while (response->length == 0) {
        delay(timeDelay);
        timeSpent += timeDelay;
        if (timeSpent > 5000) {
            break;
        }
    }
    _responses.erase(id);
    return response->length != 0;
}

void VernierDevice::sendPacket(PacketBase *packet) {
    uint8_t *data = packet->toArray();
    uint8_t lenRemaining = packet->getLength();
    uint8_t offset = 0;

    while (lenRemaining > 0) {
        uint8_t chunk = min(lenRemaining, (uint8_t) 20);
        _commandCharacteristic->writeValue(data + offset, chunk);
        lenRemaining -= chunk;
        offset += chunk;
    }
}

bool VernierDevice::initSensors() {
    buffer response;
    PacketBase availableSensors(EVernierCommand::GetSensorAvailableMask);
    if (!sendWithResponse(&availableSensors, &response)) {
        return false;
    }
    _availableSensorsMask = response.data[6];

    PacketBase defaultSensors(EVernierCommand::GetDefaultSensorsMask);
    if (!sendWithResponse(&defaultSensors, &response)) {
        return false;
    }
    _defaultSensorsMask = response.data[6];

    for (size_t i = 0; i < 32; i++) {
        if (_availableSensorsMask >> i & 0x1) {
            SimplePacket sensorInfoPacket(EVernierCommand::GetSensorInfo, i);
            if (!sendWithResponse(&sensorInfoPacket, &response)) {
                continue;
            }
            SensorInfo *info = new SensorInfo();
            info->parse(&response);
            sensorInfos[i] = info;

            if (info->typicalMeasurementPeriod < _defaultPeriod) {
                _defaultPeriod = info->typicalMeasurementPeriod;
            }
        }
    }

    return true;
}

void VernierDevice::HandleMeasurements(MeasurementResponse *response) {
    if (!response->isData()) {
        Serial.println("Got non Data measurement!!!!");
        return;
    }
    uint32_t mask = response->getMask();
    uint8_t offset = 0;
    for (uint8_t i = 0; i < 32; i++) {
        if (!((mask >> i) & 1)) {
            continue;
        }


        SensorInfo *info = sensorInfos[i];
        uint8_t *data = response->getData(offset++);
        uint8_t dataCount = response->getDataCount();

        info->values_buffer.lock();
        buffer *buff = info->values_buffer.current();

        // TODO: remove and create protection for buffer overflow
        if (buff->length + dataCount >= 256) {
            Serial.println("ERROR: SensorInfo buffer was about to overflow!");
            buff->clear();
        }

        buff->write(data, dataCount);
        info->isInts = response->isInts();
        info->values_buffer.unlock();
    }
}


bool VernierDevice::startMeasurement(uint32_t mask) {
    buffer response;
    StartMeasurementPacket packet = StartMeasurementPacket(mask);
    if (!sendWithResponse(&packet, &response)) {
        return false;
    }
    _enabledSensorsMask |= mask;
    return true;
}

bool VernierDevice::stopMeasurement() {
    buffer response;
    StopMeasurementPacket packet;
    if (!sendWithResponse(&packet, &response)) {
        return false;
    }
    _enabledSensorsMask = 0;
    return true;
}

bool VernierDevice::setMeasurementPeriod(uint32_t period) {
    buffer response;
    SetMeasurementPeriodPacket packet = SetMeasurementPeriodPacket(period);
    return sendWithResponse(&packet, &response);
}


bool VernierDevice::begin() {
    if (!initConnection()) {
        Serial.println("Connection init failed!!");
        return false;
    }
    Serial.println("Connection init complete");

    buffer response;
    InitPacket initPacket;
    if (!sendWithResponse(&initPacket, &response)) {
        Serial.println("InitPacket failed!");
        return false;
    } else {
        Serial.println("InitPacket success");
    }

    if (!initSensors()) {
        Serial.println("Sensors init failed!!");
        return false;
    }
    Serial.println("Sensors init complete");

    return true;
}
