#ifndef NETWORKMANAGERCONTROLLER_H
#define NETWORKMANAGERCONTROLLER_H
#include "RED4ext/Api/Sdk.hpp"
#include "RED4ext/SystemUpdate.hpp"

#include "RED4ext/Scripting/IScriptable.hpp"
#include "RED4ext/Scripting/Stack.hpp"

#include <MessageFrame.h>
#include <RedLib.hpp>
#include <steam/isteamnetworkingsockets.h>
#include <steam/steamnetworkingtypes.h>

class NetworkGameSystem : public Red::IGameSystem
{
private:
    HSteamNetConnection m_hConnection;
    ISteamNetworkingSockets *m_pInterface;

private:
    static void ConnectionStatusChangedCallback(SteamNetConnectionStatusChangedCallback_t* pInfo);
    void OnRegisterUpdates(RED4ext::UpdateRegistrar* aRegistrar) override;
    void OnNetworkUpdate(RED4ext::FrameInfo& frame_info, RED4ext::JobQueue& job_queue);

protected:
    template<typename T> bool EnqueueMessage(uint8_t channel_id, T frame);
    void PollIncomingMessages();

public:
    bool ConnectToServer(RED4ext::CString host, uint16_t port);

    /// Called from the plugin load and unload events
    static bool Load();
    /// Called from the plugin load and unload events
    static void Unload();
private:
    RTTI_IMPL_TYPEINFO(NetworkGameSystem);
    RTTI_IMPL_ALLOCATOR();
};

RTTI_DEFINE_CLASS(NetworkGameSystem, {
    RTTI_METHOD(ConnectToServer);
    RTTI_ALIAS("CyberM.Network.Managers.NetworkGameSystem");
});

#endif //NETWORKMANAGERCONTROLLER_H
