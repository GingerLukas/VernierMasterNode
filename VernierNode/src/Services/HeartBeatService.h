//
// Created by lukas on 07/12/2022.
//
#ifndef VERNIERNODE_HEARTBEATSERVICE_H
#define VERNIERNODE_HEARTBEATSERVICE_H

#include "NetworkServiceBase.h"
#include "StaticService.h"
#include "buffer.h"
#include "EventService.h"


static void onUdpPacketHandler(AsyncUDPPacket &packet) {
    if (!packet.isMulticast()) {
        return;
    }
    uint8_t *data = packet.data();
    char *dataAsText = (char *) data;
    //TODO: use strncmp
    std::string string(dataAsText);
    if (string.rfind("VernierMasterNode") != 0) {
        Serial.println(
                "Wrong udp payload format, \"VernierMasterNode\" not found at the start. The packet was corrupted or something else is on this multicast group, consider changing config");
        return;
    }

    masterIp = packet.remoteIP();
}

class HeartBeatService : public NetworkServiceBase<buffer, 4224> {
public:
    bool connectedToMaster = false;

    void InternalSetup(void *parameters) final {
        Serial.print("Listening on multicast address: ");
        Serial.println(multicastGroup);
        if (!_udp.listenMulticast(multicastGroup, 2442)) {
            Serial.print("Failed to start UDP on port 2442");
        }
        _udp.onPacket(onUdpPacketHandler);
    }

    void InternalLoop(void *parameters) final {
        EnsureConnection();
        uint32_t size = 5;
        _buffer.write(&size, 4);
        _buffer.write(_rolling++);
        _client.write(_buffer.data, _buffer.length);


        delay(1000);
    }


private:
    uint8_t _rolling = 0;
    AsyncUDP _udp;

};


#endif //VERNIERNODE_HEARTBEATSERVICE_H
