#include "NetworkGameSystem.h"

#include "Main.h"

#include <steam/isteamnetworkingsockets.h>
#include <steam/isteamnetworkingutils.h> // Required, see https://github.com/ValveSoftware/GameNetworkingSockets/issues/171^
#include <steam/steamnetworkingsockets.h>

bool NetworkGameSystem::Load()
{
    if (SteamDatagramErrMsg errMsg; !GameNetworkingSockets_Init(nullptr, errMsg))
    {
        //SDK->logger->ErrorF(PLUGIN, "Could not initialize GameNetworkingSockets: %s", errMsg);
        return false;
    }

    return true;
}
void NetworkGameSystem::Unload()
{
    GameNetworkingSockets_Kill();
}

// bool NetworkGameSystem::ConnectToServer(/*RED4ext::CString host, uint16_t port*/)
// {
//     SDK->logger->InfoF(PLUGIN, "Trying to connect to server at %s:%d", host, port);
//
//     if (m_pInterface != nullptr)
//     {
//         SDK->logger->Warn(PLUGIN, "Trying to connect while already being connected. Aborting");
//         return false;
//     }
//
//     m_pInterface = SteamNetworkingSockets();
//
//     if (m_pInterface == nullptr)
//     {
//         SDK->logger->Error(PLUGIN, "Failed to initialize the networking library");
//     }
//
//     // Since I don't want to parse the ip manually and support both IP versions, I need to create a string first...
//     const auto connection_string_size = host.length + 2 + 5; // 2 (':' + \0) and 5 (65535)
//     const auto connection_string = new char[connection_string_size];
//     memset(connection_string, '\0', connection_string_size);
//     sprintf_s(connection_string, connection_string_size, "%s:%d", host.c_str(), port);
//
//     SteamNetworkingIPAddr address = {};
//     if (!address.ParseString(connection_string))
//     {
//         SDK->logger->WarnF(PLUGIN, "Failed to parse connection string \"%s\"", connection_string);
//         return false;
//     }
//
//     SteamNetworkingConfigValue_t opt = {};
//     opt.SetPtr(k_ESteamNetworkingConfig_Callback_ConnectionStatusChanged,
//                reinterpret_cast<void*>(ConnectionStatusChangedCallback));
//
//     // m_hConnection = m_pInterface->ConnectToHostedDedicatedServer(identity, 0, 1, &opt);
//     m_hConnection = m_pInterface->ConnectByIPAddress(address, 1, &opt);
//
//     if (m_hConnection == k_HSteamNetConnection_Invalid)
//     {
//         SDK->logger->WarnF(PLUGIN, "Could not create connection for string \"%s\": invalid", connection_string);
//         return false;
//     }
//
//     // TODO: Call it somewhere. Update().
//     // while (true)
//     {
//         m_pInterface->RunCallbacks(); // This shall be called in a loop.
//     }
//
//     return true;
// }
//
// void NetworkGameSystem::OnRegisterUpdates(RED4ext::UpdateRegistrar* aRegistrar)
// {
//     IGameSystem::OnRegisterUpdates(aRegistrar);
//     //aRegistrar->RegisterUpdate(RED4ext::UpdateTickGroup::FrameBegin, this, "NetworkUpdate", OnNetworkUpdate);
// }

void NetworkGameSystem::ConnectionStatusChangedCallback(SteamNetConnectionStatusChangedCallback_t* pInfo)
{
    //SDK->logger->InfoF(PLUGIN, "Connection Status Changed (%d): %s", pInfo->m_info.m_eState, pInfo->m_info.m_szEndDebug);
}
