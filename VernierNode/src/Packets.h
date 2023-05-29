//
// Created by lukas on 08/12/2022.
//

#ifndef VERNIERNODE_PACKETS_H
#define VERNIERNODE_PACKETS_H


#include "Arduino.h"
#include "buffer.h"

enum EVernierCommand : byte
{
    GetStatus = 0x10, //TODO
    StartMeasurements = 0x18,
    StopMeasurements = 0x19,
    Init = 0x1A,
    SetMeasurementPeriod = 0x1B,
    GetSensorInfo = 0x50,
    GetSensorAvailableMask = 0x51,
    Disconnect = 0x54, //TODO
    GetDeviceInfo = 0x55, //TODO
    GetDefaultSensorsMask = 0x56
};

struct PacketBase
{

public:
    static uint8_t RollingCounter;
    inline uint8_t getId()
    {
        return _buffer.data[2];
    }
    inline uint8_t getLength() { return _buffer.data[1]; }
    uint8_t *toArray()
    {
        _buffer.data[1] = _buffer.length;
        calcCheckSum();
        return _buffer.data;
    }
    inline uint8_t getCheckSum() { return _buffer.data[3]; }

    explicit PacketBase(uint8_t command)
    {
        _buffer.write(0x58);			 // start byte
        _buffer.write(0x05);			 // length
        _buffer.write(--RollingCounter); // rolling counter, packet id?
        _buffer.write(0x00);			 // checksum
        _buffer.write(command);			 // command
    }

protected:
    buffer _buffer;
    inline void calcCheckSum()
    {
        uint8_t sum = 0;
        for (size_t i = 0; i < _buffer.length; i++)
        {
            sum += _buffer.data[i];
        }
        sum -= getCheckSum();
        setCheckSum(sum);
    }
    inline void setCheckSum(uint8_t value) { _buffer.data[3] = value; }
};


struct InitPacket : public PacketBase
{
    InitPacket() : PacketBase(EVernierCommand::Init)
    {
        RollingCounter = 0xFF;
        _buffer.data[2] = --RollingCounter;

        static uint8_t data[] = {
                0xa5, 0x4a, 0x06, 0x49,
                0x07, 0x48, 0x08, 0x47,
                0x09, 0x46, 0x0a, 0x45,
                0x0b, 0x44, 0x0c, 0x43,
                0x0d, 0x42, 0x0e, 0x41};

        _buffer.write(data, 20);
    }
};

struct SetMeasurementPeriod : public PacketBase
{
    explicit SetMeasurementPeriod(uint32_t period) : PacketBase(EVernierCommand::SetMeasurementPeriod)
    {
        // header??
        _buffer.write(0xFF);
        _buffer.write(0x00);

        // period
        _buffer.write((period >> 0) & 0xFF);
        _buffer.write((period >> 8) & 0xFF);
        _buffer.write((period >> 16) & 0xFF);
        _buffer.write((period >> 24) & 0xFF);

        // padding
        _buffer.write(0x00);
        _buffer.write(0x00);
        _buffer.write(0x00);
        _buffer.write(0x00);
    }
};

struct StartMeasurements : public PacketBase
{
    explicit StartMeasurements(uint32_t mask) : PacketBase(EVernierCommand::StartMeasurements)
    {
        // header??
        _buffer.write(0xFF);
        _buffer.write(0x01);

        // mask
        _buffer.write((mask >> 0) & 0xFF);
        _buffer.write((mask >> 8) & 0xFF);
        _buffer.write((mask >> 16) & 0xFF);
        _buffer.write((mask >> 24) & 0xFF);

        // padding
        _buffer.write(0x00);
        _buffer.write(0x00);
        _buffer.write(0x00);
        _buffer.write(0x00);

        _buffer.write(0x00);
        _buffer.write(0x00);
        _buffer.write(0x00);
        _buffer.write(0x00);
    }
};

struct SimplePacket : public PacketBase
{
    SimplePacket(uint8_t command, uint8_t value) : PacketBase(command)
    {
        _buffer.write(value);
    }
};

struct SetMeasurementPeriodPacket : public PacketBase
{
    explicit SetMeasurementPeriodPacket(uint32_t period) : PacketBase(EVernierCommand::SetMeasurementPeriod)
    {
        _buffer.write(0xFF);
        _buffer.write(0x00);

        _buffer.write((uint8_t*)&period, 4);

        _buffer.write(0x00);
        _buffer.write(0x00);
        _buffer.write(0x00);
        _buffer.write(0x00);
    }
};

struct StartMeasurementPacket : public PacketBase
{
    explicit StartMeasurementPacket(uint32_t mask) : PacketBase(EVernierCommand::StartMeasurements)
    {
        _buffer.write(0xFF);
        _buffer.write(0x01);

        _buffer.write(&mask, 4);

        _buffer.write(0x00);
        _buffer.write(0x00);
        _buffer.write(0x00);
        _buffer.write(0x00);

        _buffer.write(0x00);
        _buffer.write(0x00);
        _buffer.write(0x00);
        _buffer.write(0x00);
    }
};

struct StopMeasurementPacket : public PacketBase
{
    StopMeasurementPacket() : PacketBase(EVernierCommand::StopMeasurements)
    {
        _buffer.write(0xFF);
        _buffer.write(0x00);

        _buffer.write(0xFF);
        _buffer.write(0xFF);
        _buffer.write(0xFF);
        _buffer.write(0xFF);
    }
};


#endif //VERNIERNODE_PACKETS_H
