# RED4ext.Example using CMake

An example of a RED4ext plugin using CMake.

## Build instructions

### Requirements

* **[CMake 3.8+](https://cmake.org/)**.

### Steps

1. Download and install **[Visual Studio 2019 Community Edition](https://www.visualstudio.com/)** or a higher version.
2. Download and install the **[Requirements](#requirements)**.
3. Clone this repository.
4. Clone the dependencies (`git submodule update --init --recursive`).
5. Create a directory named `build` and generate **[CMake](https://cmake.org/)** files in it.
6. Open the solution (**RED4ext.Example.CMake.sln**) located in **build** directory.
7. Build the project (the artifacts are located in `build/{debug|release}` directory).

**Notes**:

* The plugin has to be a 64-bit library. That means all required libraries have to be compiled in 64-bit and the compiler has to support 64-bit.
* Make sure you have the latest SDK by updating it using the following commands:
  * `cd deps/red4ext.sdk`
  * `git pull` / `git fetch`
  * `git checkout master`
* You can also generate the projects from command line, see the **build.yml** in **.github/workflows/build.yml**.
