//
// Created by lukas on 07/12/2022.
//

#ifndef VERNIERNODE_NETWORKSERVICEBASE_H
#define VERNIERNODE_NETWORKSERVICEBASE_H


#include <lwip/sockets.h>
#include "ServiceBase.h"
#include "WiFiClient.h"

template<typename buffer_type, uint16_t port>
class NetworkServiceBase : public ServiceBase {
public:

protected:
    void EnsureConnection() {
        bool newConnection = false;
        while (!_client.connected()) {
            Serial.print("Connecting to service on: ");
            Serial.print(masterIp);
            Serial.print(":");
            Serial.println(port);
            newConnection = true;
            _client.connect(masterIp, port);

            delay(1000);
        }

        _buffer.clear();
        if (newConnection) {
            Serial.print("Connected to service on: ");
            Serial.print(masterIp);
            Serial.print(":");
            Serial.println(port);
            _buffer.write(espUid, 12);
            _client.write(_buffer.data, _buffer.length);
            _buffer.clear();
        }
    }

    WiFiClient _client;
    buffer_type _buffer;
};


#endif //VERNIERNODE_NETWORKSERVICEBASE_H
