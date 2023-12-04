#pragma once

#include "../MessageFrame.h"
#include "EMessageTypeClientbound.h"

#include <zpp_bits.h>

enum EAuthResult: uint8_t {
    EAuthResult_Ok,
    EAuthResult_VersionMismatch,
    EAuthResult_ValidationFailed // From wrong "password" to being banned.
};

struct AuthResultClientBound: MessageFrame {
    using serialize = zpp::bits::members<4>;
    EAuthResult auth_result;
    // TODO: Why does the protocol_version break things here? I guess we need to refrain from subclassing messageframe and rather go into composition
    //uint32_t protocol_version;

    AuthResultClientBound() : MessageFrame() {
        message_type = EINIT_AUTH_RESULT;
        channel_id = 0; // TODO
    }
};