#pragma once

#include "../MessageFrame.h"
#include "EMessageTypeClientbound.h"

struct SpawnEntity {
    uint64_t networkedEntityId;
    std::string recordId;
    Vector3 spawnPosition;

    inline static void FillMessageFrame(MessageFrame& frame)
    {
        frame.message_type = eSpawnEntity;
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
