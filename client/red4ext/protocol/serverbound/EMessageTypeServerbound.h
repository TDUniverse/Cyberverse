#pragma once

#include <cstdint>

enum EMessageTypeServerbound: uint16_t {
    EINIT_AUTH = 0,
    EPLAYER_JOIN_WORLD = 1,
    ePlayerActionTracked = 2,
    ePlayerPositionUpdate = 3,
};