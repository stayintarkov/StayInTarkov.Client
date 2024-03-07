# Network Packets

# Explanation

- All network packets for Stay In Tarkov should be kept in the Network Packet folder
- Network packets must inherit ISITPacket
- 99% of your network packets will inherit BasePlayerPacket or BasePacket which have good helpers for Serialize and Deserialize of packets

# BasePlayerPacket

Inherit BasePlayerPacket for any packet that you wish to be replicated by a Coop Player. Stay in Tarkov's networking has a system which can process this packet and the Process(CoopPlayerClient) is overridable to use.