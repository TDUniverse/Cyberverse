cmake_minimum_required(VERSION 3.8)

# Change the project name here!
project(RED4ext.Example.CMake LANGUAGES CXX)

# Properties.
set_property(GLOBAL PROPERTY USE_FOLDERS ON)

# Variables.
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED YES)

set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/libs")
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin")

foreach(configuration ${CMAKE_CONFIGURATION_TYPES})
    string(TOLOWER ${configuration} configuration_lower)
    string(TOUPPER ${configuration} configuration_upper)

    set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_${configuration_upper} "${CMAKE_BINARY_DIR}/${configuration_lower}/libs")
    set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_${configuration_upper} "${CMAKE_BINARY_DIR}/${configuration_lower}/bin")
endforeach()

add_definitions(-D_CRT_SECURE_NO_WARNINGS)
include_directories("deps/red4ext.sdk/include")

add_subdirectory(src)
