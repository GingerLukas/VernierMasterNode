//
// Created by lukas on 07/12/2022.
//

#ifndef VERNIERNODE_SERVICEBASE_H
#define VERNIERNODE_SERVICEBASE_H

class ServiceBase{
    virtual void InternalLoop(void *parameters) {

    }

public:
    virtual void InternalSetup(void *parameters) {

    }
};

#endif //VERNIERNODE_SERVICEBASE_H


