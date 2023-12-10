#pragma once
#include "Main.h"
#include "RED4ext/Scripting/Natives/Generated/Vector3.hpp"

#include <MessageFrame.h>
#include <chrono>

struct InterpolationData {
    RED4ext::Vector3 positionSource;
    RED4ext::Vector3 positionTarget;
    float rotationSource;
    float rotationTarget;

    // TODO: get chrono to work. On the other hand, they are low res anyway, thus we just mutability change flaots
    //std::chrono::time_point<std::chrono::system_clock, std::chrono::duration<float>> endTime;
    float durationSeconds;
    float timeElapsed;

    [[nodiscard]] inline float CalcInterpolationProgress(float deltaTime) noexcept
    {
        // const std::chrono::time_point<std::chrono::system_clock, std::chrono::duration<float>> now = std::chrono::system_clock::now();
        // const auto elapsedSeconds = std::chrono::duration_cast<std::chrono::duration<float>>(endTime - now);
        //
        // SDK->logger->InfoF(PLUGIN, "Time Debug %f %f %f", now.time_since_epoch(), elapsedSeconds.count(), endTime.time_since_epoch());
        // return elapsedSeconds.count() / durationSeconds;
        // const auto elapsedSeconds = (endTime - std::chrono::system_clock::now().time_since_epoch().count()) / 1000000.0f;
        // return elapsedSeconds / durationSeconds;
        timeElapsed += deltaTime;
        return timeElapsed / durationSeconds;
    }

    InterpolationData()
        : positionSource()
        , positionTarget()
        , rotationSource(0)
        , rotationTarget(0)
        , durationSeconds(0)
        , timeElapsed(0.0)
    {
    }

    InterpolationData(const RED4ext::Vector3 source, const Vector3 target, const float rotationSource, const float rotationTarget, float durationSeconds)
    {
        this->positionSource = source;
        this->positionTarget = { target.x, target.y, target.z };
        this->rotationSource = rotationSource;
        this->rotationTarget = rotationTarget;
        this->durationSeconds = durationSeconds;
        this->timeElapsed = 0.0f;
        const auto durationNanos = std::chrono::duration_cast<std::chrono::microseconds>(std::chrono::duration<float>(durationSeconds));

        //this->endTime = now + std::chrono::duration<float>(durationSeconds);
        // const auto end = std::chrono::system_clock::now().time_since_epoch().count() + (int64_t)(1000000.0 * durationSeconds);
        // SDK->logger->InfoF(PLUGIN, "endTime: %lld, nau: %lld", end,  std::chrono::system_clock::now().time_since_epoch().count());
    }
};
