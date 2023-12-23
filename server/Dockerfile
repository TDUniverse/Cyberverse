FROM kuyoh/vcpkg:latest AS base
WORKDIR /workspace

ADD Native /workspace
ADD protocol /workspace/protocol

# generate and build
RUN vcpkg install --triplet=x64-linux
RUN sed -i 's\../../shared/protocol\protocol\g' /workspace/CMakeLists.txt
# TODO: Use CMakePresets instead.
RUN cmake -DCMAKE_TOOLCHAIN_FILE:STRING=${VCPKG_ROOT}/scripts/buildsystems/vcpkg.cmake \
    -DCMAKE_EXPORT_COMPILE_COMMANDS:BOOL=TRUE -DCMAKE_BUILD_TYPE:STRING=Debug \
    -H/workspace -B/workspace/build -G Ninja
RUN cmake --build /workspace/build --config Debug --target all --

FROM mcr.microsoft.com/dotnet/sdk:8.0 as sdk
WORKDIR /source

ADD Managed /source
RUN dotnet restore
RUN dotnet publish --no-restore -o /app

FROM mcr.microsoft.com/dotnet/runtime:8.0
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