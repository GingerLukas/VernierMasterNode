
#ifndef RESPONSE_H
#define RESPONSE_H
#include "Arduino.h"
#include "buffer.h"

enum EMeasurementType : uint8_t {
    NormalReal32 = 0x06,
    WideReal32 = 0x07,
    SingleChannelReal32 = 0x08,
    SingleChannelInt32 = 0x09,
    AperiodicReal32 = 0x0a,
    AperiodicInt32 = 0x0b,
    StartTime = 0x0c,
    Dropped = 0x0d,
    Period = 0x0e
};


struct MeasurementResponse {
public:
    EMeasurementType getType() const { return _measurementType; }

    uint8_t getCount() const { return _count; }

    bool isData() const { return _isData; }

    bool isInts() const { return _isInts; }

    float *getValues() { return (float *) _valueData; }

    int32_t *getValuesAsInts() { return (int32_t *) _valueData; }

    uint8_t *getData(uint8_t index) { return (uint8_t *) &_valueData[_count * index]; }

    uint8_t getDataCount() const { return _count * 4; }

    uint32_t getMask() const { return _mask; }



    explicit MeasurementResponse(uint8_t *data) {
        _valueData = nullptr;
        _measurementType = (EMeasurementType) data[4];
        _mask = 0;
        _count = 0;
        _isData = true;
        switch (_measurementType) {
            case EMeasurementType::NormalReal32:
                _mask = *(uint16_t *) &data[5];
                _count = data[7];
                parseValues(9, data);
                break;
            case EMeasurementType::WideReal32:
                _mask = *(uint32_t *) &data[5];
                _count = data[9];
                parseValues(11, data);

                break;
            case EMeasurementType::SingleChannelReal32:
            case EMeasurementType::AperiodicReal32:
                _mask = 1 << data[6];
                _count = data[7];
                parseValues(8, data);

                break;
            case EMeasurementType::SingleChannelInt32:
            case EMeasurementType::AperiodicInt32:
                _mask = 1 << data[6];
                _count = data[7];
                parseValues(8, data);
                _isInts = true;
                break;
            case EMeasurementType::StartTime:
            case EMeasurementType::Dropped:
            case EMeasurementType::Period:
                _isData = false;
                break;

            default:
                break;
        }
    }

private:
    uint8_t *_valueData;
    EMeasurementType _measurementType;
    bool _isData;
    bool _isInts = false;
    uint32_t _mask;
    uint8_t _count;

    void parseValues(uint8_t index, uint8_t *src) {
        _valueData = src+index;
    }
};

#endif