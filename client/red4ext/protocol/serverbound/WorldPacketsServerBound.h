#pragma once

#include "../MessageFrame.h"
#include "EMessageTypeServerbound.h"

#include <zpp_bits.h>

struct PlayerJoinWorld: MessageFrame {
    using serialize = zpp::bits::members<5>;

    float position_x;
    float position_y;
    float position_z;

    PlayerJoinWorld() : MessageFrame() {
        message_type = EPLAYER_JOIN_WORLD;
        channel_id = 0; // TODO
    }
};