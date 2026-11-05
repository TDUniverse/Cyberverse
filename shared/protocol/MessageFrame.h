#pragma once

#include <cstdint>

constexpr uint32_t PROTOCOL_VERSION_CURRENT = 0x0;

// The C# server reads and writes the packet structs as raw memory, so C# mirrors each one it handles under the same
// name. server/Managed.Tests/ProtocolLayoutTests.cs compares both layouts, as compiled by clang.

struct MessageFrame {
    uint8_t channel_id;
    uint16_t message_type;
    uint8_t reserved;
};

// TODO: refactor into a different file
struct Vector3
{
    float x;
    float y;
    float z;
};