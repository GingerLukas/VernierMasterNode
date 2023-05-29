#pragma once
#include "buffer.h"
#include "Arduino.h"
#include "DualLockable.h"

struct SensorInfo
{

	void parse(buffer *buff);
	bool isInts;

	int8_t number;
	uint8_t spareByte;
	uint32_t id;
	uint8_t numberMeasType;
	uint8_t samplingMode;
	char description[60];
	char unit[32];
	double measurementUncertainty;
	double minMeasurement;
	double maxMeasurement;
	uint32_t minMeasurementPeriod;
	uint64_t maxMeasurementPeriod;
	uint32_t typicalMeasurementPeriod;
	uint32_t measurementPeriodGranularity;
	uint32_t mutualExclusionMask;

    dual_lockable<buffer> values_buffer;
};