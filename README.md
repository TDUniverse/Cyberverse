# CyberVerse - The Cyberpunk 2077 Multiplayer Framework

## What is Cyberverse exactly?
Cyberverse is a very much in progress implementation of a multiplayer into Cyberpunk 2077 v2.1.  
It being a "framework" additionally means that the server side is flexibly extended by Plugins that provide additional behavior. As such, it will be a good choice for Roleplaying Servers.

Besides being extendable, it also features a working sandbox mode, that already handles most of the hard topics
in Networking (such as entity sync) for you. You should be able to replace every component at will, though.

## Code Structure
CyberVerse actually consists of 4 projects: Two client side projects and two server side projects, or put
differently: Two c++ projects that mostly handle the network serialization and two application layers that
handle server side logic and replicating in game.  
Since it has different projects, people with multiple skillsets are welcome to collaborate, but please contact
us before implementing a bigger feature so we can talk about the concepts relevant to that.
Additionally, there's `shared\protocol` that handles the serialization objects.

### Server.Native
This is the C++ support DLL of the game server, it really exclusively handles the lowest levels of networking,
including serialization, which is why you (currently, sadly) need to add switch-case statements when implementing
a new packet in protocol.

### Server.Managed
The main logic of the game server, also needs to know about the new enum and structs for new packets. When 
adhereing to a C-ABI, they can comfortably marshalled via P/Invoke. It will then perform high level logic, 
bookkeeping, call potential plugins and then enqueue packets on it's own again.

### Client.RED4extModule
This is the C++ side in the game that handles networking and bridges between the redscript utility functions
and implements packets that are sent from the server (clientbound) as well as sending packets when a certain
action has occured.

### Client.Redscript
This is the layer that is closest to the game logic, it's job is basically to listen for events and apply server
side changes into the game. Future use cases will also be providing a server browser and chat UI and maybe
interfacing with custom redscript plugins, written by the related server owners (such as custom packets/payloads,
triggering packets, callbacks on packets).

## Required Client dependencies
- RED4ext
- Redscript
- Codeware