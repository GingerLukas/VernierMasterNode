//
// Created by lukas on 07/12/2022.
//

#ifndef VERNIERNODE_STATICSERVICE_H
#define VERNIERNODE_STATICSERVICE_H

#include "Arduino.h"

template<class ServiceT>
class StaticService : public ServiceT {
public:
    StaticService() {
    }

    static void Start(const char *const name, uint8_t priority = 0);

    static void LoopProxy(void *parameters);

    TaskHandle_t _serviceTaskHandle;

    static StaticService<ServiceT> instance;


};

template<class ServiceT>
void StaticService<ServiceT>::Start(const char *const name, uint8_t priority) {
    xTaskCreate(LoopProxy, name, 4096, nullptr, priority, &instance._serviceTaskHandle);
}

template<class ServiceT>
void StaticService<ServiceT>::LoopProxy(void *parameters) {
    instance.InternalSetup(parameters);
    while (true) {
        instance.InternalLoop(parameters);
    }
}


template<class ServiceT>
StaticService<ServiceT> StaticService<ServiceT>::instance;


#endif //VERNIERNODE_STATICSERVICE_H
