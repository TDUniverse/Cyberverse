#pragma once

#include <cstdint>

struct Message {
    uint8_t channelId = 0;
    uint16_t messageType = 0;
    uint32_t connectionId = 0;
    uintptr_t data = 0;
};