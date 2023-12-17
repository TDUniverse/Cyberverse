#pragma once
#include <RED4ext/RED4ext.hpp>
#include "RedLib.hpp"
#include "RED4ext/CName.hpp"
#include "RED4ext/Scripting/Natives/Generated/game/Object.hpp"
#include "RED4ext/Scripting/Natives/Generated/game/events/HitEvent.hpp"
#include "RED4ext/Scripting/Natives/Generated/game/input/ActionType.hpp"
#include "RED4ext/Scripting/Natives/Generated/game/projectile/ShootEvent.hpp"

class PlayerActionTracker final : public Red::IScriptable
{
public:
    void RecordPlayerAction(RED4ext::CName actionName, RED4ext::gameinputActionType actionType, float value);
    void OnShoot(RED4ext::Handle<RED4ext::gameprojectileShootEvent> event);
    void OnHit(RED4ext::Handle<RED4ext::GameObject> gameObject, RED4ext::Handle<RED4ext::gameHitEvent> event);
private:
    RTTI_IMPL_TYPEINFO(PlayerActionTracker);
    RTTI_IMPL_ALLOCATOR();
};

RTTI_DEFINE_CLASS(PlayerActionTracker, {
    RTTI_METHOD(RecordPlayerAction);
    RTTI_METHOD(OnShoot);
    RTTI_METHOD(OnHit);
    RTTI_ALIAS("CyberM.Network.Managers.PlayerActionTracker");
});
