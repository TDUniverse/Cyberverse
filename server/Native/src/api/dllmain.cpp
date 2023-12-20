#include <cstdint>

#include "message.h"
#include "GameServer.h"

// https://stackoverflow.com/a/2164853
#if defined(_MSC_VER)
    //  Microsoft
    #define EXPORT __declspec(dllexport)
    #define IMPORT __declspec(dllimport)
#elif defined(__GNUC__)
    //  GCC
    #define EXPORT __attribute__((visibility("default")))
    #define IMPORT
#else
    //  do nothing and hope for the best?
    #define EXPORT
    #define IMPORT
    #pragma warning Unknown dynamic link import/export semantics.
#endif

// TODO: Currently we can't use CYBERVERSE_PUBLIC,
// #if CYBERVERSE_COMPILING
// #   define CYBERVERSE_PUBLIC EXPORT
// #else
// #   define CYBERVERSE_PUBLIC IMPORT
// #endif



extern "C" EXPORT bool init_gameserver() {
    return GameServer::Initialize();
}

extern "C" EXPORT void destroy_gameserver()
{
    GameServer::Destroy();
}
extern "C" EXPORT GameServer *server_create(const uint16_t server_port)
{
    const auto server = new GameServer();
    if (!server->ListenOn(server_port))
    {
        return nullptr;
    }

    return server;
}

/// This method is not recommended (at the moment) as it does never return until you kill the process
extern "C" EXPORT void server_run_blocking(GameServer *server)
{
    server->RunBlocking();
}

extern "C" EXPORT void server_update(GameServer *server, float deltaTime)
{
    server->Update(deltaTime);
}

extern "C" EXPORT Message server_poll_message(const GameServer *server) {
    return server->PollRecvQueue();
}

extern "C" EXPORT void message_data_release(const uintptr_t data) {
    // technically this bypasses the message data struct destructor, but the API is there to make this a giant switch-case
    // if ever needed to be (but I'd not see a reason a packet struct needing a destructor)
    free(reinterpret_cast<void*>(data));
}

/// Note: this may override the channelId based on the messageType.
extern "C" EXPORT void server_enqueue_message(const GameServer *server, const Message msg)
{
    server->EnqueueSendQueue(msg);
}

/// Utility method to allocate X bytes so they can be passed into the send queue as Message->data and we can free it
/// _after_ sending. Otherwise, if it was a caller unmanaged memory, we would have to copy it as we should expect
/// it's lifetime to not surpass the call to server_enqueue_message. But since we provide a way to allocate it into
/// our native heap, we can control the lifetime of the passed data.
extern "C" EXPORT uintptr_t message_data_allocate(const uint64_t size)
{
    return reinterpret_cast<uintptr_t>(malloc(size));
}
