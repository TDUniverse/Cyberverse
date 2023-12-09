#pragma once
#include "RED4ext/Handle.hpp"
#include "RED4ext/Scripting/Natives/Generated/EulerAngles.hpp"
#include "RED4ext/Scripting/Natives/Generated/Vector4.hpp"
#include "RED4ext/Scripting/Natives/Generated/WorldTransform.hpp"
#include "RED4ext/Scripting/Natives/Generated/cp/PlayerSystem.hpp"
#include "RED4ext/Scripting/Natives/Generated/game/Object.hpp"
#include "RedLib.hpp"

namespace CyberM::Utils
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
}

