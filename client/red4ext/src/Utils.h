#pragma once
#include "RED4ext/Handle.hpp"
#include "RED4ext/Scripting/Natives/Generated/EulerAngles.hpp"
#include "RED4ext/Scripting/Natives/Generated/Vector4.hpp"
#include "RED4ext/Scripting/Natives/Generated/WorldTransform.hpp"
#include "RED4ext/Scripting/Natives/Generated/cp/PlayerSystem.hpp"
#include "RED4ext/Scripting/Natives/Generated/ent/EntityID.hpp"
#include "RED4ext/Scripting/Natives/Generated/game/Object.hpp"
#include "RED4ext/Scripting/Natives/Generated/vehicle/BaseObject.hpp"
#include "RedLib.hpp"

namespace Cyberverse::Utils
{
    inline RED4ext::Handle<RED4ext::GameObject> GetPlayer()
    {
        const auto system = Red::GetGameSystem<Red::PlayerSystem>();
        Red::Handle<Red::GameObject> player;
        Red::CallVirtual(system, "GetLocalPlayerControlledGameObject", player);
        //Red::CallVirtual(system, "GetLocalPlayerMainGameObject", player);
        return player;
    }

    /// Caution: Do NOT use the WorldPosition directly.
    inline RED4ext::WorldTransform Entity_GetWorldTransform(const RED4ext::Handle<RED4ext::Entity>& entity)
    {
        RED4ext::WorldTransform transform = {};
        Red::CallVirtual(entity, "GetWorldTransform", transform);
        return transform;
    }

    inline RED4ext::Vector4 Entity_GetWorldPosition(const RED4ext::Handle<RED4ext::Entity>& entity)
    {
        RED4ext::Vector4 position = {};
        Red::CallVirtual(entity, "GetWorldPosition", position);
        return position;
    }

    inline RED4ext::Quaternion Entity_GetWorldOrientation(const RED4ext::Handle<RED4ext::Entity>& entity)
    {
        RED4ext::Quaternion quaternion = {};
        Red::CallVirtual(entity, "GetWorldOrientation", quaternion);
        return quaternion;
    }

    /// Caution: This doesn't even seem to work as expected.
    /// Edit: Maybe it does now
    inline RED4ext::Vector4 WorldPosition_ToVector4(const RED4ext::WorldPosition position)
    {
        RED4ext::Vector4 vec4 = {};
        Red::CallStatic("WorldPosition", "ToVector4", vec4, position);
        return vec4;
    }

    inline RED4ext::EulerAngles Quaternion_ToEulerAngles(const RED4ext::Quaternion quat)
    {
        RED4ext::EulerAngles euler = {};
        Red::CallStatic("Quaternion", "ToEulerAngles", euler, quat);
        return euler;
    }

    inline RED4ext::Vector4 Vector3To4(const RED4ext::Vector3 vec)
    {
        RED4ext::Vector4 out = {};
        Red::CallStatic("Vector4", "Vector3To4", out, vec);
        return out;
    }

    inline RED4ext::Vector3 Vector4To3(const RED4ext::Vector4 vec)
    {
        RED4ext::Vector3 out = {};
        Red::CallStatic("Vector4", "Vector4To3", out, vec);
        return out;
    }

    inline std::optional<Red::Handle<Red::Entity>> GetDynamicEntity(const RED4ext::EntityID& entityId)
    {
        Red::Handle<Red::IGameSystem> dynamicEntitySystem;
        if (!Red::CallStatic("ScriptGameInstance", "GetDynamicEntitySystem", dynamicEntitySystem))
        {
            SDK->logger->Warn(PLUGIN, "Getting the dynamic entity system failed");
            return {};
        }

        Red::Handle<Red::Entity> entity;
        if (!Red::CallVirtual(dynamicEntitySystem, "GetEntity", entity, entityId) || entity == nullptr)
        {
            SDK->logger->WarnF(PLUGIN, "Failed to get the entity (%llu) by id", entityId.hash);
            return {};
        }

        return entity;
    }

    inline RED4ext::Vector3 LerpLocal(const RED4ext::Vector3& from, const RED4ext::Vector3& to, float t) noexcept
    {
        const auto x = from.X + (to.X - from.X) * t;
        const auto y = from.Y + (to.Y - from.Y) * t;
        const auto z = from.Z + (to.Z - from.Z) * t;
        return RED4ext::Vector3(x, y, z);
    }

    inline RED4ext::TweakDBID VehicleObject_GetRecordID(const RED4ext::Handle<RED4ext::VehicleObject>& vehicle)
    {
        RED4ext::TweakDBID dbId = {};
        Red::CallVirtual(vehicle, "GetRecordID", dbId);
        return dbId;
    }
}

