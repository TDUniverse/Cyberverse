#include "GameServer.h"
#include "NetworkGameSystem.h"

#include <cstdio>

// TODO: This is only for debugging purposes, perspectively we will call into the DLL with a C ABI
int main()
{
    printf("Starting CyberM Server 0.0.1 (c) 2023 MeFisto94\n");

    if (!SINGLETON_GAMESERVER.Initialize())
    {
        printf("Error when initializing server!\n");
        return 1;
    }

    SINGLETON_GAMESERVER.ListenOn(1337);
    SINGLETON_GAMESERVER.RunBlocking();
    SINGLETON_GAMESERVER.Destroy();
}