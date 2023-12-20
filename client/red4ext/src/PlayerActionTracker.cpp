#include "PlayerActionTracker.h"
#include "Main.h"
#include "RED4ext/Api/Sdk.hpp"

#include "NetworkGameSystem.h"
#include "RED4ext/Scripting/Natives/Generated/game/MountEventData.hpp"
#include "RED4ext/Scripting/Natives/Generated/game/damage/AttackData.hpp"
#include "RED4ext/Scripting/Natives/Generated/game/mounting/MountingRequest.hpp"
#include "Utils.h"

#include <serverbound/WorldPacketsServerBound.h>

void PlayerActionTracker::RecordPlayerAction(RED4ext::CName actionName, RED4ext::gameinputActionType actionType,
                                             float value)
{
    const auto name = std::string(actionName.ToString());
    //SDK->logger->InfoF(PLUGIN, "Player Action %s (%d) -> %f", name.c_str(), actionType, value);

    // We ignore movement inputs like Forward, MoveX, MoveY for now, because we don't care about animations but just
    // want the most recent position And for that, those inputs are very unreliable due to physics/collision etc anyway.
    if (actionType == RED4ext::game::input::ActionType::RELATIVE_CHANGE)
    {
        // think about mouse movements, at least mouse_x is important as yaw.
        // alternatively CameraMouseX.
    }
    else if (name == "Jump" && actionType == RED4ext::game::input::ActionType::BUTTON_RELEASED)
    {
        const auto player = Cyberverse::Utils::GetPlayer();
        const auto [X, Y, Z, W] = Cyberverse::Utils::Entity_GetWorldPosition(player);

        PlayerActionTracked tracked = {};
        tracked.action = eACTION_JUMP;

        tracked.worldTransform = {};
        tracked.worldTransform.x = X;
        tracked.worldTransform.y = Y;
        tracked.worldTransform.z = Z;

        Red::GetGameSystem<NetworkGameSystem>()->EnqueueMessage(1, tracked);
    }
}

void PlayerActionTracker::OnShoot(RED4ext::Handle<RED4ext::gameprojectileShootEvent> event)
{
    SDK->logger->InfoF(PLUGIN, "OnShoot! range: %f", event->params.range);
    // // TODO: This is the completely wrong event, we want the WeaponObject and finding out that it has been excecuted
    // // even. There's scriptedPuppet->GetActiveWeapon()
    // const auto player = Cyberverse::Utils::GetPlayer();
    // const auto [X, Y, Z, W] = Cyberverse::Utils::Entity_GetWorldPosition(player);
    //
    // PlayerActionTracked tracked = {};
    // tracked.action = eACTION_RANGED_ATTACK;
    //
    // tracked.worldTransform = {};
    // tracked.worldTransform.x = X;
    // tracked.worldTransform.y = Y;
    // tracked.worldTransform.z = Z;
}

void PlayerActionTracker::OnHit(RED4ext::Handle<RED4ext::GameObject> gameObject,
                                RED4ext::Handle<RED4ext::gameHitEvent> event)
{
    SDK->logger->InfoF(PLUGIN, "OnHit! %d", event->attackData->attackType);
}

void PlayerActionTracker::OnMounting(RED4ext::Handle<RED4ext::game::mounting::MountingEvent> event)
{
    if (event->relationship.otherMountableType != RED4ext::game::MountingObjectType::Vehicle)
    {
        // mountingSubType would tell us whether bike or car.
        return;
    }

    if (event->relationship.relationshipType != RED4ext::game::MountingRelationshipType::Parent)
    {
        // If the car mounts us, do nothing
        return;
    }

    if (event->relationship.otherObject.Expired())
    {
        SDK->logger->Warn(PLUGIN, "Cannot Process PlayerActionTracker::OnMounting as the vehicle's weak ref has expried");
        return;
    }

    // Keep alive as long as the derived vehicleInstance
    const auto strongLock = event->relationship.otherObject.Lock();
    const RED4ext::game::Object* ptr = strongLock; // TODO: if you figure out how to inline that, you're welcome to do so.
    const auto vehicleInstance = RED4ext::Handle((RED4ext::VehicleObject*)ptr);

    // TODO: vehicles.swift, there is VehicleObject.GetRecordID(), but there is also GameObject::GetTDBID(go) which is more generic and handles casting,
    // so it works for puppets, devices _and_ vehicles. Question is if we should here just implement both so we can avoid casting and misleading?

    const auto recordId = Cyberverse::Utils::VehicleObject_GetRecordID(vehicleInstance);
    const auto [X, Y, Z, W] = Cyberverse::Utils::Entity_GetWorldPosition(vehicleInstance);
    const auto orientation = Cyberverse::Utils::Entity_GetWorldOrientation(vehicleInstance);
    const auto [Roll, Pitch, Yaw] = Cyberverse::Utils::Quaternion_ToEulerAngles(orientation);

    PlayerSpawnCar spawnCar = {};
    spawnCar.recordId = recordId.value;
    spawnCar.worldTransform = Vector3 { X, Y, Z };
    spawnCar.yaw = Yaw;

    Red::GetGameSystem<NetworkGameSystem>()->EnqueueMessage(0, spawnCar);
}
