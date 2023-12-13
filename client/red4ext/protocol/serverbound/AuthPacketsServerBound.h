#pragma once

#include "../MessageFrame.h"
#include "EMessageTypeServerbound.h"

#include <zpp_bits.h>

// TODO: we need to find a solution for strings, as zpp::bits doesn't support char* and c# doesn't support std::string.
// constant arrays are wasteful. So we add yet another type just for the C# ABI
struct InitAuthServerBound {
    std::string username; // TODO: Limit to 0xFF
    // TODO: proof
    uint32_t protocol_version;

    inline static void FillMessageFrame(MessageFrame& frame)
    {
        frame.message_type = EINIT_AUTH;
        frame.channel_id = 0; // TODO
    }
};

struct InitAuthServerBoundCSharp
{
    char username[255] { 0 }; // can't be a char* here either, otherwise we _do_ need message type dependant deconstructors
    uint32_t protocol_version;

    explicit InitAuthServerBoundCSharp(const InitAuthServerBound& other)
    {
        memcpy_s(username, 255, other.username.c_str(), other.username.length());
        protocol_version = other.protocol_version;
    }
};