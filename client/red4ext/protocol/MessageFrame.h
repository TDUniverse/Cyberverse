#pragma once

constexpr uint32_t PROTOCOL_VERSION_CURRENT = 0x0;

struct MessageFrame {
    uint8_t channel_id;
    uint16_t message_type;
    uint8_t reserved;
};
