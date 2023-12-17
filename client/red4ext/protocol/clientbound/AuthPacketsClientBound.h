#pragma once

#include "../MessageFrame.h"
#include "EMessageTypeClientbound.h"

enum EAuthResult: uint8_t {
    EAuthResult_Ok,
    EAuthResult_VersionMismatch,
    EAuthResult_ValidationFailed // From wrong "password" to being banned.
};

struct AuthResultClientBound {
    EAuthResult auth_result;
    uint32_t protocol_version;

    inline static void FillMessageFrame(MessageFrame& frame)
    {
        frame.message_type = EINIT_AUTH_RESULT;
        frame.channel_id = 0; // TODO
    }
};