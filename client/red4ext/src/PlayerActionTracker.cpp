#include "PlayerActionTracker.h"
#include "Main.h"
#include "RED4ext/Api/Sdk.hpp"

#include "NetworkGameSystem.h"
#include "Utils.h"

#include <serverbound/WorldPacketsServerBound.h>

void PlayerActionTracker::RecordPlayerAction(RED4ext::CName actionName, RED4ext::gameinputActionType actionType,
                                             float value)
{
    auto name = std::string(actionName.ToString());
    // SDK->logger->InfoF(PLUGIN, "Player Action %s (%d) -> %f", name.c_str(), actionType, value);

    // We ignore movement inputs like Forward, MoveX, MoveY for now, because we don't care about animations but just want the most recent position
    // And for that, those inputs are very unreliable due to physics/collision etc anyway.
    if (actionType == RED4ext::game::input::ActionType::RELATIVE_CHANGE)
    {
        // think about mouse movements, at least mouse_x is important as yaw.
        // alternatively CameraMouseX.
    }
    else if (name == "Jump" && actionType == RED4ext::game::input::ActionType::BUTTON_RELEASED)
    {
        const auto player = CyberM::Utils::GetPlayer();
        const auto [X, Y, Z, W] = CyberM::Utils::Entity_GetWorldPosition(player);

        PlayerActionTracked tracked = {};
        tracked.action = eACTION_JUMP;

        tracked.worldTransform = {};
        tracked.worldTransform.x = X;
        tracked.worldTransform.y = Y;
        tracked.worldTransform.z = Z;

        Red::GetGameSystem<NetworkGameSystem>()->EnqueueMessage(1, tracked);
    }
}
