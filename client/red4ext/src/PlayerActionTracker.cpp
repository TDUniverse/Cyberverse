#include "RED4ext/Api/Sdk.hpp"
#include "Main.h"
#include "PlayerActionTracker.h"

void PlayerActionTracker::RecordPlayerAction(RED4ext::CName actionName, RED4ext::gameinputActionType actionType,
                                             float value)
{
    SDK->logger->InfoF(PLUGIN, "Player Action %s (%d) -> %f", actionName.ToString(), actionType, value);
}
