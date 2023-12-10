#include "GameServer.h"

#include <cassert>
#include <corecrt_wstdio.h>
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

static void SteamNetConnectionStatusChangedCallback( SteamNetConnectionStatusChangedCallback_t *pInfo)
{
    printf("Connection Status Changed (%d): %s\n", pInfo->m_info.m_eState, pInfo->m_info.m_szEndDebug);
    SINGLETON_GAMESERVER.OnConnectionStatusChanged(pInfo);
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

void GameServer::OnConnectionStatusChanged(const SteamNetConnectionStatusChangedCallback_t* pInfo)
{
    char temp[1024];

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

            // Locate the client.  Note that it should have been found, because this
            // is the only codepath where we remove clients (except on shutdown),
            // and connection change callbacks are dispatched in queue order.
            // auto itClient = m_mapClients.find( pInfo->m_hConn );
            // assert( itClient != m_mapClients.end() );
            //
            // // Select appropriate log messages
            // const char *pszDebugLogAction;
            // if ( pInfo->m_info.m_eState == k_ESteamNetworkingConnectionState_ProblemDetectedLocally )
            // {
            //     pszDebugLogAction = "problem detected locally";
            //     sprintf( temp, "Alas, %s hath fallen into shadow.  (%s)", itClient->second.m_sNick.c_str(), pInfo->m_info.m_szEndDebug );
            // }
            // else
            // {
            //     // Note that here we could check the reason code to see if
            //     // it was a "usual" connection or an "unusual" one.
            //     pszDebugLogAction = "closed by peer";
            //     sprintf( temp, "%s hath departed", itClient->second.m_sNick.c_str() );
            // }
            //
            // // Spew something to our own log.  Note that because we put their nick
            // // as the connection description, it will show up, along with their
            // // transport-specific data (e.g. their IP address)
            // Printf( "Connection %s %s, reason %d: %s\n",
            //     pInfo->m_info.m_szConnectionDescription,
            //     pszDebugLogAction,
            //     pInfo->m_info.m_eEndReason,
            //     pInfo->m_info.m_szEndDebug
            // );
            //
            // m_mapClients.erase( itClient );
            //
            // // Send a message so everybody else knows what happened
            // SendStringToAllClients( temp );
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

        // Generate a random nick.  A random temporary nick
        // is really dumb and not how you would write a real chat server.
        // You would want them to have some sort of signon message,
        // and you would keep their client in a state of limbo (connected,
        // but not logged on) until them.  I'm trying to keep this example
        // code really simple.
        // char nick[ 64 ];
        // snprintf( nick, 64, "BraveWarrior%d", 10000 + ( rand() % 100000 ) );

        // Send them a welcome message
        // snprintf(temp, 1024, "Welcome, stranger.  Thou art known to us for now as '%s'; upon thine command '/nick' we shall know thee otherwise.\n", nick);
        // printf(temp);
        // SendStringToClient( pInfo->m_hConn, temp );
        //
        // // Also send them a list of everybody who is already connected
        // if ( m_mapClients.empty() )
        // {
        //     SendStringToClient( pInfo->m_hConn, "Thou art utterly alone." );
        // }
        // else
        // {
        //     sprintf( temp, "%d companions greet you:", (int)m_mapClients.size() );
        //     for ( auto &c: m_mapClients )
        //         SendStringToClient( pInfo->m_hConn, c.second.m_sNick.c_str() );
        // }

        // Let everybody else know who they are for now
        // snprintf( temp, 1024, "Hark!  A stranger hath joined this merry host.  For now we shall call them '%s'\n", nick );
        // printf(temp);
        // SendStringToAllClients( temp, pInfo->m_hConn );
        //
        // // Add them to the client list, using std::map wacky syntax
        // m_mapClients[ pInfo->m_hConn ];
        // SetClientNick( pInfo->m_hConn, nick );
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
    // TODO: More resonable quit condition
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

        // We ignore the movement packet. This is not the best logging anyway.
        if (pIncomingMsg->m_cbSize != 20)
        {
            printf("Received a packet with %d bytes\n", pIncomingMsg->m_cbSize);
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

        // TODO: This should both be more generic probably _AND_ we need to consider how we want to hand this off to C#, given that they may want to control _all_ packet logic.
        // This however depends on how flexible and moddable we want our, e.g. auth handling, to be.
        switch (frame.message_type)
        {
            case EINIT_AUTH:
            {
                InitAuthServerBound init_auth = {};
                if (zpp::bits::failure(in(init_auth)))
                {
                    fprintf(stderr, "Faulty packet: InitAuthServerBound\n");
                    pIncomingMsg->Release();
                    continue;
                }
                printf("Auth request from %s for protocol %d\n", init_auth.username.c_str(), init_auth.protocol_version);

                AuthResultClientBound result_packet = {};

                if (init_auth.protocol_version != PROTOCOL_VERSION_CURRENT)
                {
                    result_packet.auth_result = EAuthResult_VersionMismatch;
                } else
                {
                    const auto validation = m_authController.ValidateUser(init_auth.username);
                    result_packet.auth_result = validation ? EAuthResult_Ok : EAuthResult_ValidationFailed;
                }

                EnqueueMessage(pIncomingMsg->m_conn, 0, result_packet);
            }
            break;

            case EPLAYER_JOIN_WORLD:
            {
                PlayerJoinWorld player_join = {};
                if (zpp::bits::failure(in(player_join)))
                {
                    fprintf(stderr, "Faulty packet: PlayerJoinWorld\n");
                    pIncomingMsg->Release();
                    continue;
                }
                printf("Player joined the world at (%f, %f, %f)\n", player_join.position_x, player_join.position_y, player_join.position_z);
            }
            break;

            case ePlayerActionTracked:
            {
                PlayerActionTracked action_tracked = {};
                if (zpp::bits::failure(in(action_tracked)))
                {
                    fprintf(stderr, "Faulty packet: PlayerActionTracked\n");
                    pIncomingMsg->Release();
                    continue;
                }

                if (action_tracked.action == eACTION_JUMP)
                {
                    // Character.Panam
                    const SpawnEntity spawn_entity = { 0, "Character.Judy", action_tracked.worldTransform };
                    EnqueueMessage(pIncomingMsg->m_conn, 1, spawn_entity);
                }
            }
            break;

            case ePlayerPositionUpdate:
            {
                PlayerPositionUpdate position_update = {};
                if (zpp::bits::failure(in(position_update)))
                {
                    fprintf(stderr, "Faulty packet: PlayerPositionUpdate\n");
                    pIncomingMsg->Release();
                    continue;
                }

                // TODO: Implement differently, don't teleport.
                const TeleportEntity teleport_entity = { 0, position_update.worldTransform, position_update.yaw };
                EnqueueMessage(pIncomingMsg->m_conn, 1, teleport_entity);
            }
            break;

            default:
                printf("Message Type: %d\n", frame.message_type);
                break;
        }

        pIncomingMsg->Release();
    }
}

void GameServer::RunBlocking()
{
    const auto g_bQuit = false;
    while ( !g_bQuit )
    {
        PollIncomingMessages();
        PollConnectionStateChanges();
        //PollLocalUserInput();
        std::this_thread::sleep_for( std::chrono::milliseconds( 100 ) );
    }
}
void GameServer::Destroy()
{
    GameNetworkingSockets_Kill();
}

GameServer SINGLETON_GAMESERVER = {};
