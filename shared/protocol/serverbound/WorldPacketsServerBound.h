#pragma once

#include "../MessageFrame.h"
#include "EMessageTypeServerbound.h"

struct PlayerJoinWorld {
    Vector3 position;

    inline static void FillMessageFrame(MessageFrame& frame)
    {
        frame.message_type = EPLAYER_JOIN_WORLD;
        frame.channel_id = 0; // TODO
    }
};

enum PlayerAction: uint8_t
{
    eACTION_JUMP,
    eACTION_RANGED_ATTACK,
};

struct PlayerActionTracked
{
    // TODO: bool buttonState? (We could trigger on press or release)
    // TODO: uint64_t networkTick (relative to the connect in relative server timestamps, because we probably get those batched and relay those batched)
    PlayerAction action;
    Vector3 worldTransform;

    inline static void FillMessageFrame(MessageFrame& frame)
    {
        frame.message_type = ePlayerActionTracked;
        frame.channel_id = 1; // TODO
    }
};

struct PlayerPositionUpdate
{
    // TODO: uint64_t networkTick (relative to the connect in relative server timestamps, because we probably get those batched and relay those batched)
    Vector3 worldTransform;
    float yaw;

    inline static void FillMessageFrame(MessageFrame& frame)
    {
        frame.message_type = ePlayerPositionUpdate;
        frame.channel_id = 1; // TODO
    }
};