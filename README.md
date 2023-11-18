
<div align=center style="text-align: center">
<h1 style="text-align: center"> SIT.Core </h1>
An Escape From Tarkov BepInEx module designed to be used with SPT-Aki Server with the ultimate goal of "Offline" Coop 
</div>

---

<div align=center>

![GitHub all releases](https://img.shields.io/github/downloads/paulov-t/SIT.Core/total) ![GitHub release (latest by date)](https://img.shields.io/github/downloads/paulov-t/SIT.Core/latest/total)

[English](README.md) **|** [简体中文](README_CN.md) **|** [Deutsch](README_DE.md) **|** [Português-Brasil](README_PO.md) **|** [日本語](README_JA.md) **|** [한국어-Korean](README_KO.md) **|** [Français](README_FR.md)
</div>

---

## State of Stay In Tarkov

* Stay In Tarkov is in active development by the SIT team
* Pull Requests and Contributions will always be accepted (if they work!)

--- 

## About

The Stay in Tarkov project was born due to Battlestate Games' (BSG) reluctance to create the pure PvE version of Escape from Tarkov. 
The project's aim is simple, create a Cooperation PvE experience that retains progression. 
If BSG decide to create the ability to do this on live OR I receive a DCMA request, this project will be shut down immediately.

## Disclaimer

* You must buy the game to use this. You can obtain it here. [https://www.escapefromtarkov.com](https://www.escapefromtarkov.com). 
* This is by no means designed for cheats (this project was made because cheats have destroyed the Live experience)
* This is by no means designed for illegally downloading the game (and has blocks for people that do!)
* This is purely for educational purposes. I am using this to learn Unity and TCP/UDP/Web Socket Networking and I learnt a lot from BattleState Games \o/.
* I am not affiliated with BSG or others (on Reddit or Discord) claiming to be working on a project. Do NOT contact SPTarkov subreddit or Discord about this project.
* This project is not affiliated with SPTarkov (SPT-Aki) but uses its excellent Server.
* This project is not affiliated with any other Escape from Tarkov emulator.
* This project comes "as-is". It either works for you or it doesn't.

## Support

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/N4N2IQ7YJ)
* The Ko-Fi link is buying Paulov a coffee.
* Pull Requests are encouraged. Thanks to all contributors!
* Please do not hand over money expecting help or a solution. 
* This is a hobby, for fun, project. Please don't treat it seriously. 
* Paulov: I know this is a semi-broken attempt but will try to fix as best I can. SIT Contributors: "We can do it!"

## SPT-AKI Requirement
* Stay in Tarkov works requires the [latest AKI Server](https://dev.sp-tarkov.com/SPT-AKI/Server) to run. You can learn about SPT-Aki [here](https://www.sp-tarkov.com/).
* DO NOT INSTALL THIS ON TO SPT-Aki CLIENT! ONLY INSTALL THE SERVER!

## [Wiki](https://github.com/paulov-t/SIT.Core/wiki)
**The Wiki is has been constructed by various contributors. All instructions are also kept within the source in the wiki directory.**
  - ### [Setup Manuals](https://github.com/paulov-t/SIT.Core/wiki/Guides-English)
  - ### [FAQs](https://github.com/paulov-t/SIT.Core/wiki/FAQs-English)

## Coop

### PREREQUISITE
You must have the [SPT-Aki mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) installed in your Server for this module to work. If you do not wish to use the Coop module, you must disable it in the BepInEx config file.

### Can Coop use BSG's Coop code?
No. BSG server code is hidden from the client for obvious reasons. So BSG's implementation of Coop use the same online servers as PvPvE. We don't see this, so we cannot use this.

## SPT-Aki

### Are Aki BepInEx (Client mods) Modules supported?
The following Aki Modules are supported.
- aki-core
- Aki.Common
- Aki.Reflection
- Do SPT-AKI Client mods work? This is dependant on how well written the patches are. If they directly target GCLASSXXX or PUBLIC/PRIVATE then they will likely fail.

### Why don't you use Aki Module DLLs?
SPT-Aki DLLs are written specifically for their own Deobfuscation technique and Paulov's own technique is not working well with Aki Modules at this moment in time.
So I ported many of SPT-Aki features into this module. The end-goal would be to rely on SPT-Aki and for this to be solely focused on SIT only features.

## How to compile? 
[Compiling Document](COMPILE.md)

## Thanks List
- SPT-Aki team (Credits provided on each code file used and much love to their Dev team for their support)
- SPT-Aki Modding Community
- DrakiaXYZ ([BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain) & [Waypoints](https://github.com/DrakiaXYZ/SPT-Waypoints) are integrated with this project)
- SIT team and it's original contributors

## License

- DrakiaXYZ projects contain the MIT License
- 99% of the original core and single-player functionality completed by SPT-Aki teams. There are licenses pertaining to them within this source.
- None of my own work is Licensed. This is solely a just for fun project. I don't care what you do with it.
