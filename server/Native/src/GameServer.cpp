#include "GameServer.h"

#include <cassert>
#include <cstdio>
#include <steam/steamnetworkingsockets.h>
#include <thread>
#include <zpp_bits.h>
#include <serverbound/EMessageTypeServerbound.h>
#include <serverbound/AuthPacketsServerBound.h>
#include <serverbound/WorldPacketsServerBound.h>
#include <clientbound/EMessageTypeClientbound.h>
#include <clientbound/AuthPacketsClientBound.h>
#include <clientbound/WorldPacketsClientBound.h>

bool GameServer::Initialize()
{
    if (SteamDatagramErrMsg errMsg; !GameNetworkingSockets_Init(nullptr, errMsg))
    {
        fprintf(stderr, "Could not initialize GameNetworkingSockets: %s\n", errMsg);
        return false;
    }

    return true;
}

static void SteamNetConnectionStatusChangedCallback(SteamNetConnectionStatusChangedCallback_t *pInfo)
{
    SINGLETON_GAMESERVER->OnConnectionStatusChanged(pInfo);
}

bool GameServer::ListenOn(const uint16_t nPort)
{
    m_pInterface = SteamNetworkingSockets();
    if (m_pInterface == nullptr)
    {
        fprintf(stderr, "Failed to initialize the networking library\n");
    }

    // Start listening
    SteamNetworkingIPAddr serverLocalAddr = {};
    serverLocalAddr.Clear();
    serverLocalAddr.m_port = nPort;

    // SteamNetConnectionStatusChangedCallback needs a singleton to get back to this instance, so we're overwriting it
    // (for now) maybe there's a better way like a dynamic jump table that just pushes ecx to a static value?
    SINGLETON_GAMESERVER = this;

    SteamNetworkingConfigValue_t opt = {};
    opt.SetPtr(k_ESteamNetworkingConfig_Callback_ConnectionStatusChanged,
               reinterpret_cast<void*>(SteamNetConnectionStatusChangedCallback));
    m_hListenSock = m_pInterface->CreateListenSocketIP(serverLocalAddr, 1, &opt);
    if (m_hListenSock == k_HSteamListenSocket_Invalid)
    {
        fprintf(stderr, "Failed to listen on port %d\n", nPort);
        return false;
    }

    m_hPollGroup = m_pInterface->CreatePollGroup();
    if (m_hPollGroup == k_HSteamNetPollGroup_Invalid)
    {
        fprintf(stderr, "Failed to listen on port %d\n", nPort);
        return false;
    }

    printf("Server listening on port %d\n", nPort);
    return true;
}

void GameServer::SetConnectionStatusChangedCallback(void (*cb)(uint32_t, uint32_t))
{
    m_ConnectionStatusCallback = cb;
}

void GameServer::OnConnectionStatusChanged(const SteamNetConnectionStatusChangedCallback_t* pInfo) const
{
    // What's the state of the connection?
    switch ( pInfo->m_info.m_eState )
    {
    case k_ESteamNetworkingConnectionState_None:
        // NOTE: We will get callbacks here when we destroy connections.  You can ignore these.
        break;

    case k_ESteamNetworkingConnectionState_ClosedByPeer:
    case k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
    {
        // Ignore if they were not previously connected.  (If they disconnected
        // before we accepted the connection.)
        if ( pInfo->m_eOldState == k_ESteamNetworkingConnectionState_Connected )
        {
            if (m_ConnectionStatusCallback != nullptr)
            {
                m_ConnectionStatusCallback(pInfo->m_info.m_eState, pInfo->m_hConn);
            }
        }
        else
        {
            assert( pInfo->m_eOldState == k_ESteamNetworkingConnectionState_Connecting );
        }

        // Clean up the connection.  This is important!
        // The connection is "closed" in the network sense, but
        // it has not been destroyed.  We must close it on our end, too
        // to finish up.  The reason information do not matter in this case,
        // and we cannot linger because it's already closed on the other end,
        // so we just pass 0's.
        m_pInterface->CloseConnection( pInfo->m_hConn, 0, nullptr, false );
        break;
    }

    case k_ESteamNetworkingConnectionState_Connecting:
    {
        // This must be a new connection
        //assert( m_mapClients.find( pInfo->m_hConn ) == m_mapClients.end() );

        printf("Connection request from %s\n", pInfo->m_info.m_szConnectionDescription);

        // A client is attempting to connect
        // Try to accept the connection.
        if ( m_pInterface->AcceptConnection( pInfo->m_hConn ) != k_EResultOK )
        {
            // This could fail.  If the remote host tried to connect, but then
            // disconnected, the connection may already be half closed.  Just
            // destroy whatever we have on our side.
            m_pInterface->CloseConnection( pInfo->m_hConn, 0, nullptr, false );
            printf( "Can't accept connection.  (It was already closed?)\n" );
            break;
        }

        // Assign the poll group
        if (!m_pInterface->SetConnectionPollGroup(pInfo->m_hConn, m_hPollGroup))
        {
            m_pInterface->CloseConnection( pInfo->m_hConn, 0, nullptr, false );
            printf( "Failed to set poll group?\n" );
            break;
        }

        if (m_ConnectionStatusCallback != nullptr)
        {
            m_ConnectionStatusCallback(pInfo->m_info.m_eState, pInfo->m_hConn);
        }
        break;
    }
    case k_ESteamNetworkingConnectionState_Connected:
        // We will get a callback immediately after accepting the connection.
        // Since we are the server, we can ignore this, it's not news to us.
        break;

    default:
        // Silences -Wswitch
        break;
    }
}

void GameServer::PollConnectionStateChanges() const
{
    if (m_pInterface != nullptr)
    {
        m_pInterface->RunCallbacks();
    }
}

template<typename T>
bool GameServer::EnqueueMessage(HSteamNetConnection connection, uint8_t channel_id, T content)
{
    auto frame = MessageFrame {};
    frame.channel_id = channel_id;
    content.FillMessageFrame(frame);

    auto [data, out] = zpp::bits::data_out();

    auto zpp_result = out(frame);
    if (zpp::bits::failure(zpp_result))
    {
        // Failed to serialize the frame(!)
        return false;
    }

    zpp_result = out(content);
    if (zpp::bits::failure(zpp_result))
    {
        // Failed to serialize the content
        return false;
    }

    // TODO: derive the send flags from the channel id, i.e. lookup registered channels.
    assert(data.size() < std::numeric_limits<uint32_t>::max());

    const auto result = m_pInterface->SendMessageToConnection(connection, data.data(),
        static_cast<uint32_t>(data.size()), k_nSteamNetworkingSend_Reliable, nullptr);

    if (result == k_EResultOK)
    {
        return true;
    }

    fprintf(stderr, "GameServer::EnqueueMessage(%d) => Error %d\n", channel_id, result);
    return false;
}

void GameServer::PollIncomingMessages()
{
    // TODO: More reasonable quit condition
    while (true)
    {
        ISteamNetworkingMessage* pIncomingMsg = nullptr;
        const auto numMsgs = m_pInterface->ReceiveMessagesOnPollGroup(m_hPollGroup, &pIncomingMsg, 1);

        if (numMsgs == 0)
        {
            return;
        }

        if (numMsgs < 0)
        {
            fprintf(stderr, "Error polling messages: %d\n", numMsgs);
            return;
        }

        auto [data, in] = zpp::bits::data_in();
        const auto begin = (std::byte*)pIncomingMsg->GetData();
        data.assign(begin, begin + pIncomingMsg->GetSize());

        MessageFrame frame = {};
        if (zpp::bits::failure(in(frame)))
        {
            fprintf(stderr, "Faulty packet\n");
            pIncomingMsg->Release();
            continue;
        }

        // TODO: This should both be more generic probably _AND_ we need to consider how we want to hand this off to C#,
        // given that they may want to control _all_ packet logic. This however depends on how flexible and moddable we
        // want our, e.g. auth handling, to be.
        // Another problem is that we have to deserialize the packets anyway, we could use macros for that at least,
        // but it still requires native changes to add a new packet type. Especially for packets that aren't represented
        // like that on the CSharp side (e.g. strings)
        switch (frame.message_type)
        {
            case EINIT_AUTH:
            {
                auto init_auth = InitAuthServerBound();
                if (zpp::bits::failure(in(init_auth)))
                {
                    fprintf(stderr, "Faulty packet: InitAuthServerBound\n");
                    pIncomingMsg->Release();
                    continue;
                }

                // extra pain due to C# ABI
                auto init_auth_abi = new InitAuthServerBoundCSharp(init_auth);
                AddToRecvQueue(frame.message_type, pIncomingMsg->m_conn, frame.channel_id, reinterpret_cast<uintptr_t>(init_auth_abi));
            }
            break;

            DESERIALIZE_RECV_QUEUE(EPLAYER_JOIN_WORLD, PlayerJoinWorld)
            DESERIALIZE_RECV_QUEUE(ePlayerActionTracked, PlayerActionTracked)
            DESERIALIZE_RECV_QUEUE(ePlayerPositionUpdate, PlayerPositionUpdate)
            DESERIALIZE_RECV_QUEUE(ePlayerSpawnCar, PlayerSpawnCar)
            DESERIALIZE_RECV_QUEUE(ePlayerUnmountCar, PlayerUnmountCar)
            DESERIALIZE_RECV_QUEUE(ePlayerEquipItem, PlayerEquipItem)
            DESERIALIZE_RECV_QUEUE(ePlayerShoot, PlayerShoot)
            default:
                printf("Unknown Message Type: %d\n", frame.message_type);
                break;
        }

        pIncomingMsg->Release();
    }
}

void GameServer::Update(float deltaTime)
{
    ProcessSendQueue();
    PollIncomingMessages();
    PollConnectionStateChanges();
}

void GameServer::RunBlocking()
{
    const auto g_bQuit = false;
    while ( !g_bQuit )
    {
        Update(0.1f);
        std::this_thread::sleep_for( std::chrono::milliseconds( 100 ) );
    }
}

void GameServer::Destroy()
{
    GameNetworkingSockets_Kill();
}

Message GameServer::PollRecvQueue() const
{
    if (dll_recv_queue->empty())
    {
        return {};
    }

    const auto val = dll_recv_queue->front();
    dll_recv_queue->pop();
    return val;
}

template<typename T>
void GameServer::AddToRecvQueue(const uint16_t messageType, const uint32 connectionId, const uint8_t channelId,
                                const T& data) const
{
    // Helper template as we can only deserialize into stack based structs, and we then copy the packet into a long-lived ptr.
    const auto buf = malloc(sizeof(T));
    memcpy(buf, (void*)&data, sizeof(T));
    AddToRecvQueue(messageType, connectionId, channelId, reinterpret_cast<uintptr_t>(buf));
}

void GameServer::AddToRecvQueue(const uint16_t messageType, const uint32 connectionId, const uint8_t channelId,
                                const uintptr_t data) const
{
    const auto message = Message{channelId, messageType, connectionId, data};
    dll_recv_queue->emplace(message);
}

void GameServer::EnqueueSendQueue(Message msg) const
{
    dll_send_queue->emplace(msg);
}

void GameServer::ProcessSendQueue()
{
    while (!dll_send_queue->empty())
    {
        const auto val = dll_send_queue->front();
        dll_send_queue->pop();

        switch (val.messageType)
        {
            SERIALIZE_SEND_QUEUE(EINIT_AUTH_RESULT, AuthResultClientBound)
            SERIALIZE_SEND_QUEUE(eSpawnEntity, SpawnEntity)
            SERIALIZE_SEND_QUEUE(eTeleportEntity, TeleportEntity)
            SERIALIZE_SEND_QUEUE(eDestroyEntity, DestroyEntity)
            SERIALIZE_SEND_QUEUE(eEquipItemEntity, EquipItemEntity)
            default:
                printf("Unknown messageType: %d\n", val.messageType);
        }

        free(reinterpret_cast<void*>(val.data));
    }
}

GameServer *SINGLETON_GAMESERVER = nullptr;
