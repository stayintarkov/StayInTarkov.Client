# Installation guide 

* This guide expects users to be able to have access to their ISP Router Port Forwarding and Firewall settings and knows how to change them
* This guide assumes you already have an official version of Escape from Tarkov installed using BattleState Games' own Launcher

## HOST

1. Download [SPT Aki](https://www.sp-tarkov.com/) and extract Aki.Server and Aki_Data to any desired folder
2. Follow instructions to [Install SIT Coop Mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) into the Server and configure correctly
3. Delete all your current copies of Offline EFT
4. Download [SIT-Launcher](https://github.com/stayintarkov/SIT.Launcher.Classic) [latest release](https://github.com/stayintarkov/SIT.Launcher.Classic/releases) .zip file and extract anywhere
5. Run the SIT.Launcher.exe
6. Follow SIT-Launcher instructions to create a copy of your Live EFT and install latest version of SIT automatically
7. Ensure that SIT-Launcher installs SIT / Assemblies in the Settings tab
8. Ensure that you followed instructions to [Install SIT Coop Mod](https://github.com/stayintarkov/SIT.Aki-Server-Mod) and set your Aki Server http.json to your Network card's Internal IP and Coop Mod's coopConfig.json to your [External IP](https://www.whatismyip.com/)
9. Launch Server and share with your friends your External IP address
10. Port Forward your Router Port 6969,6970 to your Server's Local Network card's IP address (e.g. 192.1.2.3)
11. Open Firewall to Port 6969,6970 on your Server and Router
12. Test your connection by entering your External IP and port into the Launcher and click Launch
12. Good to go!

## CLIENT

1. Delete all your current copies of Offline EFT
2. Download [SIT-Launcher](https://github.com/stayintarkov/SIT.Launcher.Classic) [latest release](https://github.com/stayintarkov/SIT.Launcher.Classic/releases) .zip file and extract anywhere
3. Run the SIT.Launcher.exe
4. Follow SIT-Launcher instructions to create a copy of your Live EFT and install latest version of SIT automatically
5. Ensure that SIT-Launcher installs SIT / Assemblies in the Settings tab
6. Connect to IP and port given by the HOST (e.g. http://111.222.255.255:6969)
7. DO NOT USE USERNAME AND PASSWORDS FROM OTHER SOURCES, ALL PASSWORDS ARE SAVED IN PLAIN TEXT!

