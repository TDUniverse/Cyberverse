#add_library(${CMAKE_PROJECT_NAME} SHARED "")
add_executable(${CMAKE_PROJECT_NAME})

set(PROJECT_HEADER_FILES
  Main.h
  NetworkGameSystem.h
        GameServer.h
        AuthController.h
)

set(PROJECT_SRC_FILES
  Main.cpp
  NetworkGameSystem.cpp
        GameServer.cpp
        AuthController.cpp
)

source_group(TREE "${CMAKE_CURRENT_SOURCE_DIR}" FILES ${PROJECT_HEADER_FILES} ${PROJECT_SRC_FILES})

target_include_directories(${CMAKE_PROJECT_NAME} PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})
target_sources(${CMAKE_PROJECT_NAME} PRIVATE ${PROJECT_HEADER_FILES} ${PROJECT_SRC_FILES})

target_link_libraries(${CMAKE_PROJECT_NAME} PRIVATE zpp_bits)
target_link_libraries(${CMAKE_PROJECT_NAME} PRIVATE GameNetworkingSockets::shared)
target_link_libraries(${CMAKE_PROJECT_NAME} PRIVATE protocol)
