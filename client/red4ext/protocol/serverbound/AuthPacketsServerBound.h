#pragma once

#include "../MessageFrame.h"
#include "EMessageTypeServerbound.h"

#include <zpp_bits.h>

struct InitAuthServerBound {
    std::string username; // TODO: limit to 0x255
    // TODO: proof
    uint32_t protocol_version;

    inline static void FillMessageFrame(MessageFrame& frame)
    {
        frame.message_type = EINIT_AUTH;
        frame.channel_id = 0; // TODO
    }
};