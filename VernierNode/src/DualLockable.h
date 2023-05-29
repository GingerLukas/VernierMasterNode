//
// Created by lukas on 07/12/2022.
//

#ifndef VERNIERNODE_DUALLOCKABLE_H
#define VERNIERNODE_DUALLOCKABLE_H

#include "Arduino.h"

template<class T>
class dual_lockable {
public:
    dual_lockable() {
        _mutex = xSemaphoreCreateMutex();
        _currentIndex = 0;
    }

    void lock() {
        xSemaphoreTake(_mutex, portMAX_DELAY);
    }

    void unlock() {
        xSemaphoreGive(_mutex);
    }

    T *swap() {
        T *old = current();
        _currentIndex = (_currentIndex + 1) % 2;
        return old;
    }

    T *current() {
        return &_lockables[_currentIndex];
    }

private:
    T _lockables[2];
    volatile uint8_t _currentIndex;
    SemaphoreHandle_t _mutex;
};

#endif //VERNIERNODE_DUALLOCKABLE_H
