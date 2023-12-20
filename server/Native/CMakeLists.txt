cmake_minimum_required(VERSION 3.12)

# Change the project name here!
project(Cyberverse.Server.Native LANGUAGES CXX)

# Set this option to ON to be able to use folders / filters in your project when using Visual Studio.
set_property(GLOBAL PROPERTY USE_FOLDERS ON)

# Set the C++ standard to 20 (C++20).
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED YES)

# Include zpp_bits
add_library(zpp_bits INTERFACE)
target_include_directories(zpp_bits INTERFACE "deps/zpp_bits")

# Include Valve's GameNetworkingSockets
find_package(GameNetworkingSockets CONFIG REQUIRED)

# Include our protocol headers (shared with the client)
add_library(protocol INTERFACE)
target_include_directories(protocol INTERFACE "../../shared/protocol")

add_compile_definitions(CYBERM_COMPILING)

# Include the main project.
add_subdirectory(src)
