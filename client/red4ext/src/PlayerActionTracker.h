#pragma once
#include "RED4ext/CName.hpp"
#include "RED4ext/Scripting/Natives/Generated/game/input/ActionType.hpp"
#include "RedLib.hpp"

class PlayerActionTracker: public Red::IScriptable
{
public:
    void RecordPlayerAction(RED4ext::CName actionName, RED4ext::gameinputActionType actionType, float value);
private:
    RTTI_IMPL_TYPEINFO(PlayerActionTracker);
    RTTI_IMPL_ALLOCATOR();
};

RTTI_DEFINE_CLASS(PlayerActionTracker, {
    RTTI_METHOD(RecordPlayerAction);
    RTTI_ALIAS("CyberM.Network.Managers.PlayerActionTracker");
});
