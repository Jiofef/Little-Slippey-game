import os
import platform
import subprocess

env = Environment()

env.Append(CPPFLAGS=["-std=c++17"])
env.Append(CPPPATH=["#"])
env.Append(CPPPATH=["godot-cpp/"])

env.Append(LIBS=["godot-cpp/bin/libgodot-cpp"])

target = "bin/copy_getnode_plugin"
if platform.system() == "Windows":
    target += ".dll"
elif platform.system() == "Darwin":
    target += ".dylib"
else:
    target += ".so"

env.SharedLibrary(target=target, source=["copy_getnode_plugin.cpp", "register_types.cpp"])
