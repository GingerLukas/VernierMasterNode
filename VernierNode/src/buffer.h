#include "Arduino.h"

#ifndef BUFFER_H
#define BUFFER_H

#define BUFFER_SIZE 256

struct buffer {
    uint8_t data[BUFFER_SIZE];
    uint8_t length;

    void write(uint8_t value) {
        if (length + 1 < BUFFER_SIZE) data[length++] = value;
    }

    void write(void *values, size_t count) {
        if (length + count <= BUFFER_SIZE) {
            memcpy(data + length, values, count);
            length += count;
        }
    }

    void clear() {
        length = 0;
    }

    buffer() { clear(); }
};


template<class T>
struct ptr_buffer {
    T *data[BUFFER_SIZE];
    uint8_t length{};

    void write(T *value) {
        if (length + 1 < BUFFER_SIZE) data[length++] = value;
    }


    void clear() {
        for (int i = 0; i < length; i++) {
            delete data[i];
        }
        length = 0;
    }

    ptr_buffer() {
        for (int i = 0; i < BUFFER_SIZE; i++) {
            data[i] = nullptr;
        }
        clear();
    }

    ~ptr_buffer() { clear(); }
};

struct large_buffer {
    uint8_t data[BUFFER_SIZE * 4];
    uint32_t length;

    void write(uint8_t value) {
        if (length + 1 < BUFFER_SIZE * 4)data[length++] = value;
    }

    void write(void *values, size_t count) {
        if (length + count <= BUFFER_SIZE * 4) {
            memcpy(data + length, values, count);
            length += count;
        }

    }

    void clear() {
        length = 0;
    }

    large_buffer() { length = 0; }
};

static buffer empty_buffer;

#endif
