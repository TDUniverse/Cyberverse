#pragma once
#include "AuthController.h"
#include "api/message.h"

#include <queue>
#include <serverbound/AuthPacketsServerBound.h>

#include <steam/isteamnetworkingsockets.h>
#include <steam/steamnetworkingtypes.h>

// TODO: add a callback for error logging instead of printing to stderr
class GameServer {
private:
    HSteamListenSocket m_hListenSock = 0;
    HSteamNetPollGroup m_hPollGroup = 0;
    ISteamNetworkingSockets *m_pInterface = nullptr;
    AuthController m_authController;

    std::queue<Message>* dll_recv_queue = new std::queue<Message>();
    std::queue<Message>* dll_send_queue = new std::queue<Message>();
    void (*m_ConnectionStatusCallback)(uint32_t, uint32_t) = nullptr;

protected:
    void PollConnectionStateChanges() const;
    void AddToRecvQueue(uint16_t message_type, uint32 connectionId, uint8_t channelId, uintptr_t data) const;
    template<typename T> void AddToRecvQueue(uint16_t message_type, uint32 connectionId, uint8_t channelId,
                                const T& data) const;
    void PollIncomingMessages();
    void ProcessSendQueue();
public:
    static bool Initialize();
    static void Destroy();

public:
    void OnConnectionStatusChanged(const SteamNetConnectionStatusChangedCallback_t* pInfo) const;
public:
    /* DLL API start */
    bool ListenOn(uint16_t nPort = 1337);
    void RunBlocking();
    void Update(float deltaTime);

    [[nodiscard]] Message PollRecvQueue() const;
    void EnqueueSendQueue(Message msg) const;
    void SetConnectionStatusChangedCallback(void (*cb)(uint32_t, uint32_t));
    /* DLL API stop */

    template<typename T>
    bool EnqueueMessage(HSteamNetConnection connection, uint8_t channel_id, T frame);
};

extern GameServer *SINGLETON_GAMESERVER;
