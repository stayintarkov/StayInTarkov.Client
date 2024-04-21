# Network Packets

## Explanation

- All network packets for Stay In Tarkov should be kept in the Network Packet folder
- Network packets must inherit ISITPacket
- Most of your network packets will inherit BasePlayerPacket or BasePacket which have good helpers for Serialize and Deserialize of packets
- There is a performance-first option available for performance-critical packets like player states sent every tick, or player spawn that can be quite voluminous and happen in bursts. It uses FlatBuffers, which requires building a schema beforehand.

## BasePlayerPacket

Inherit BasePlayerPacket for any packet that you wish to be replicated by a Coop Player. Stay in Tarkov's networking has a system which can process this packet and the Process(CoopPlayerClient) is overridable to use.

## FlatBuffers

For performance-critical packets where we're fine trading dev pace for runtime performance, we use [FlatBuffers](https://flatbuffers.dev/flatbuffers_guide_tutorial.html) to have zero (de)serialization overhead and still a compact on-the-wire format.
This requires compiling `.fbs` files to generate C# classes. This can be done with the `flatc` compiler, which needs to be built separately.

```
# From Visual Studio, Tools -> Command Line -> Developer PowerShell
vcpkg install
.\vcpkg_installed\x64-windows\tools\flatbuffers\flatc --csharp <path/to/file.fbs>
```