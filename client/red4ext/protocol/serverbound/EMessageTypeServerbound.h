#pragma once
enum EMessageTypeServerbound {
    EINIT_AUTH = 0,
    EPLAYER_JOIN_WORLD = 1,
    ePlayerActionTracked = 2,
    ePlayerPositionUpdate = 3,
};