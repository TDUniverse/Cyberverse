#pragma once

#include "../MessageFrame.h"
#include "EMessageTypeClientbound.h"

// TODO: we need to find a solution for strings, as zpp::bits doesn't support char* and c# doesn't support std::string.
// constant arrays are wasteful. So we add yet another type just for the C# ABI
// Actually it can't be a char* there either, otherwise we _do_ need message type dependant deconstructors

struct SpawnEntity {
    uint64_t networkedEntityId = 0;
    uint64_t recordId;
    Vector3 spawnPosition {0, 0, 0};

    inline static void FillMessageFrame(MessageFrame& frame)
    {
        frame.message_type = eSpawnEntity;
        frame.channel_id = 1; // TODO:
    }
};

struct DestroyEntity {
    uint64_t networkedEntityId = 0;

    inline static void FillMessageFrame(MessageFrame& frame) {
        frame.message_type = eDestroyEntity;
        frame.channel_id = 1; // TODO:
    }
};

// TODO: There will be better packets, containing target locations for interpolation, with timestamps.
struct TeleportEntity
{
    uint64_t networkedEntityId;
    Vector3 targetPosition;
    float yaw; // NPCs (i.e. players) only have a yaw that you can set anyway, not a full rotation.

    inline static void FillMessageFrame(MessageFrame& frame)
    {
        frame.message_type = eTeleportEntity;
        frame.channel_id = 1; // TODO:
    }
};
