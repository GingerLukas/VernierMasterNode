//
// Created by lukas on 07/12/2022.
//
#include "SensorInfo.h"

void SensorInfo::parse(buffer *buff) {
    number = buff->data[6];
    spareByte = buff->data[7];
    id = *(uint32_t *) &buff->data[8];
    numberMeasType = buff->data[12];
    samplingMode = buff->data[13];
    memcpy(&description, &buff->data[14], 60);
    memcpy(&unit, &buff->data[74], 32);
    measurementUncertainty = *(double *) &buff->data[106];
    minMeasurement = *(double *) &buff->data[114];
    maxMeasurement = *(double *) &buff->data[122];
    minMeasurementPeriod = *(uint32_t *) &buff->data[130];
    maxMeasurementPeriod = *(uint64_t *) &buff->data[134];
    typicalMeasurementPeriod = *(uint32_t *) &buff->data[142];
    measurementPeriodGranularity = *(uint32_t *) &buff->data[146];
    mutualExclusionMask = *(uint32_t *) &buff->data[150];
}
