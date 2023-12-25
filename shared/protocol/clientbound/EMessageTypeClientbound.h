#pragma once

#include <cstdint>

enum EMessageTypeClientbound: uint16_t {
    EINIT_AUTH_RESULT = 0,
    eSpawnEntity = 1,
    eTeleportEntity = 2,
    eDestroyEntity = 3,
    eEquipItemEntity = 4,
};