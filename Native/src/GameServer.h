#pragma once
#include "AuthController.h"
#include "NetworkGameSystem.h"

#include <string>

class GameServer {
private:
    HSteamListenSocket m_hListenSock;
    HSteamNetPollGroup m_hPollGroup;
    ISteamNetworkingSockets *m_pInterface = nullptr;
    AuthController m_authController;
protected:
    void PollConnectionStateChanges() const;
public:
    bool Initialize();
    bool ListenOn(uint16_t nPort = 1337);
    void PollIncomingMessages();
    void RunBlocking();
    void Destroy();
    void OnConnectionStatusChanged(const SteamNetConnectionStatusChangedCallback_t* pInfo);
    template<typename T> bool EnqueueMessage(HSteamNetConnection connection, uint8_t channel_id, T frame);
};

extern GameServer SINGLETON_GAMESERVER;
