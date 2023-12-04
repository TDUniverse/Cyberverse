#pragma once

#include "../MessageFrame.h"
#include "EMessageTypeServerbound.h"

#include <zpp_bits.h>

struct InitAuthServerBound: MessageFrame {
    using serialize = zpp::bits::members<5>;

    std::string username; // TODO: limit to 0x255
    // TODO: proof
    uint32_t protocol_version;

    InitAuthServerBound() : MessageFrame() {
        message_type = EINIT_AUTH;
        channel_id = 0; // TODO
    }
};