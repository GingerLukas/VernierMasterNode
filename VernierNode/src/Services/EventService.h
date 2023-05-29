//
// Created by lukas on 08/12/2022.
//

#ifndef VERNIERNODE_EVENTSERVICE_H
#define VERNIERNODE_EVENTSERVICE_H

#include "buffer.h"
#include "NetworkServiceBase.h"
#include "DualLockable.h"
#include "VernierEvent.h"

class EventService : NetworkServiceBase<large_buffer, 2222> {
public:
    void EnqueueEvent(VernierEvent *event) {
        events.lock();
        events.current()->write(event);
        events.unlock();
    }

    void InternalLoop(void *parameters) override {
        EnsureConnection();
        events.lock();
        ptr_buffer<VernierEvent> *buff = events.swap();
        events.unlock();

        if (buff->length > 0) {
            _buffer.length += 4;
            _buffer.write(buff->length);

            for (int i = 0; i < buff->length; ++i) {
                VernierEvent *event = buff->data[i];
                _buffer.write(event->_buffer.length);
                _buffer.write(event->_buffer.data, event->_buffer.length);
            }

            uint32_t len = _buffer.length;
            _buffer.length = 0;
            _buffer.write(&len, 4);
            _buffer.length = len;
            _client.write(_buffer.data, _buffer.length);
        }
        buff->clear();
        delay(100);
    }

    dual_lockable<ptr_buffer<VernierEvent>> events;

    void InternalSetup(void *parameters) override {

    }
};

#endif //VERNIERNODE_EVENTSERVICE_H
