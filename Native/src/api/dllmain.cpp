#include <queue>

#include "message.h"
#include "GameServer.h"

extern "C" __declspec(dllexport) bool init_gameserver() {
    return GameServer::Initialize();
}

extern "C" __declspec(dllexport) void destroy_gameserver()
{
    GameServer::Destroy();
}
extern "C" __declspec(dllexport) GameServer *server_create(const uint16_t server_port)
{
    const auto server = new GameServer();
    if (!server->ListenOn(server_port))
    {
        return nullptr;
    }

    return server;
}

/// This method is not recommended (at the moment) as it does never return until you kill the process
extern "C" __declspec(dllexport) void server_run_blocking(GameServer *server)
{
    server->RunBlocking();
}

extern "C" __declspec(dllexport) void server_update(GameServer *server, float deltaTime)
{
    server->Update(deltaTime);
}

extern "C" __declspec(dllexport) Message server_poll_message(const GameServer *server) {
    return server->PollRecvQueue();
}

extern "C" __declspec(dllexport) void message_data_release(const uintptr_t data) {
    // technically this bypasses the message data struct destructor, but the API is there to make this a giant switch-case
    // if ever needed to be (but I'd not see a reason a packet struct needing a destructor)
    free(reinterpret_cast<void*>(data));
}

/// Note: this may override the channelId based on the messageType.
extern "C" __declspec(dllexport) void server_enqueue_message(const GameServer *server, const Message msg)
{
    server->EnqueueSendQueue(msg);
}

/// Utility method to allocate X bytes so they can be passed into the send queue as Message->data and we can free it
/// _after_ sending. Otherwise, if it was a caller unmanaged memory, we would have to copy it as we should expect
/// it's lifetime to not surpass the call to server_enqueue_message. But since we provide a way to allocate it into
/// our native heap, we can control the lifetime of the passed data.
extern "C" __declspec(dllexport) uintptr_t message_data_allocate(const uint64_t size)
{
    return reinterpret_cast<uintptr_t>(malloc(size));
}
