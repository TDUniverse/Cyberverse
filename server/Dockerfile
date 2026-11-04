# Name the OS explicitly (the plain date tags follow whichever Ubuntu kuyoh builds by default), and keep it on the same
# release as the runtime stage below: the runtime's glibc has to be >= the one the natives were built with.
# The image's vcpkg release must be at least as new as the builtin-baseline in Native/vcpkg.json, otherwise its version
# database doesn't know the pinned ports and `vcpkg install` fails. Bump the tag when a baseline update needs it.
FROM kuyoh/vcpkg:2025.10.17-ubuntu24.04 AS base
WORKDIR /workspace

# Only the manifest first, so the slow vcpkg install stays cached until the dependencies change.
ADD Native/vcpkg.json /workspace/vcpkg.json
RUN vcpkg install --triplet=x64-linux

ADD Native /workspace
ADD protocol /workspace/protocol

# generate and build
RUN sed -i 's\../../shared/protocol\protocol\g' /workspace/CMakeLists.txt
# TODO: Use CMakePresets instead.
RUN cmake -DCMAKE_TOOLCHAIN_FILE:STRING=${VCPKG_ROOT}/scripts/buildsystems/vcpkg.cmake \
    -DCMAKE_EXPORT_COMPILE_COMMANDS:BOOL=TRUE -DCMAKE_BUILD_TYPE:STRING=Debug \
    -H/workspace -B/workspace/build -G Ninja
RUN cmake --build /workspace/build --config Debug --target all --

FROM mcr.microsoft.com/dotnet/sdk:10.0 as sdk
WORKDIR /source

ADD Managed /source
RUN dotnet restore
RUN dotnet publish --no-restore -o /app

# Ubuntu 24.04 (noble) to match the build stage; the default Debian 12 image has glibc 2.36, but the natives need 2.38+.
FROM mcr.microsoft.com/dotnet/runtime:10.0-noble
WORKDIR /app
COPY --from=base /workspace/build/src/*.so .
COPY --from=base /workspace/build/vcpkg_installed/x64-linux/debug/lib/*.so .
COPY --from=sdk /app .
# TODO: When trying to resolve libGameNetworkingSockets, ldd uses system locations and libCyberM.Server.Native's RPATH (/workspace/build/vcpkg_installed/x64-linux/)
# but the .so is actually in the PWD, so we set the LD_LIBRARY_PATH (that was previously empty) to that. We could/should migrate to CMAKE using the correct
# linker flags to set RPATH to "." (who cares about /workspace/build), but that probably needs OS specific defines to only run on linux
ENV LD_LIBRARY_PATH .
USER $APP_UID
# TODO: Use a non-cringe port.
EXPOSE 1337
ENTRYPOINT ["./Cyberverse.Server"]