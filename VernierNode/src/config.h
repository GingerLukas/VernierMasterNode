//
// Created by lukas on 07/12/2022.
//

#ifndef VERNIERNODE_CONFIG_H
#define VERNIERNODE_CONFIG_H


#include "IPAddress.h"
#include "Arduino.h"

static char *ssid = "Vernier";
static char *password = "heslo1234";

static IPAddress multicastGroup(239, 244, 244, 224);

static char *espUid = "FFAACCAABBFF";

static IPAddress masterIp;

static const char *ntpServer = "pool.ntp.org";
static const long gmtOffset_sec = 3600;
static const int daylightOffset_sec = 3600;


#endif //VERNIERNODE_CONFIG_H
