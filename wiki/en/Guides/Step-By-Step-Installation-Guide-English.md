# Step by Step Stay In Tarkov Installation Guide

# Prerequisites

Before we begin, please make sure the latest version of Escape From Tarkov has been downloaded and installed using the Battlestate Games Launcher. Stay In Tarkov will not work with an outdated or illegitimate copy of the game.

Throughout the guide, we will refer to `SIT_DIR` as the root directory for installing Stay In Tarkov. In this directory, we’ll create three separate folders to keep things organized:

- A `server` folder for the SPT-AKI server.
- A `launcher` folder for the SIT Launcher.
- A `game` folder for the Escape From Tarkov game files.

*Consider using a tool like [7zip](https://7-zip.org/) or WinRAR to unzip compressed files.*

# Installation

## 1. [SIT Launcher](https://github.com/paulov-t/SIT.Launcher/releases) (using auto install)

1. Download the latest release of the `SIT Launcher` from the [Releases](https://github.com/paulov-t/SIT.Launcher/releases) page.
2. Unzip file and extract contents to `SIT_DIR/launcher`.
3. Run `SIT.Launcher.exe`.
4. The first time you run the launcher, it will prompt you for an installation:

    *“No OFFLINE install found. Would you like to install now?”*

    Click “Yes”.

5. Select `SIT_DIR/game` as the installation directory.
6. Let the launcher copy your game files, this can take a few minutes.

## 2. [SPT-AKI Server](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases)

1. Download the latest release of the `SPT-AKI Server` from the [Releases](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases) page.
2. Unzip file and extract contents to `SIT_DIR/server`.

## 3. [SIT Server Mod](https://github.com/paulov-t/SIT.Aki-Server-Mod)
1. Download the server mod’s zip file from [GitHub](https://github.com/paulov-t/SIT.Aki-Server-Mod) (look for it under the big green button: *Code > Download Zip*).
2. Unzip file and extract contents to `SIT_DIR/server/user/mods`.

    *The `user/mods` directory is automatically created when the server is run the first time. Run `Aki.Server.exe` to create the folder. Stop and close the server once the directory has been created so we can continue the installation process.*

# Configuring the server

## Hosted on localhost (for testing)

### Server
1. Open the coop server configuration file in `SIT_DIR/server/user/mods/SIT.Aki Server-Mod/config/coopConfig.json`.

    *The `coopConfig.json` file is automatically created when the server mod is run the first time. Run `Aki.Server.exe` to create the file. Stop and close the server once the file has been created so we can continue the installation process.*

    *Note: Make edits to the file using Notepad or a text editor that won't introduce formatting. Do not use Microsoft Word.*
2. Set `externalIP` to `127.0.0.1`.
3. Set `useExternalIPFinder` to `false`.
4. Optionally, set `logRequests` to `false` in `SIT_DIR/server/Aki_Data/Server/configs/http.json` to prevent log spam.

### Launcher
Connect using `http://127.0.0.1:6969` as the server.

*You won't be able to invite others to join your game using localhost, but it can be useful when debugging connection issues. Use this to confirm the game and mods are installed correctly.*

## Hosted with port forwarding

### Setup
Port forwarding allows you to use your local computer as a server for a publicly-facing service. In short, your router has a static (unchanging) _external_ IP address which you can see by going to https://www.whatismyip.com. This is the IP address that the world sees as 'you' despite that many devices are on the network (e.g. if you went to whatismyip on your phone while on Wi-Fi, you'd see the same IP as when you go there on your computer). In order to use your computer for external traffic, and to allow your friends to connect to a server running on your machine, you need to do a few things:
1. **Find your MAC address**: find your machine's MAC address, which you will need in order to identify your computer for step 3. You can find this by opening a command prompt and typing `ipconfig /all` and looking for the line that says "Physical Address" under your network adapter (e.g. Ethernet adapter Ethernet). It will look something like `00-00-00-00-00-00`.
2. **Open your router settings page**: go into your router settings webpage (often accessible at http://192.168.1.1, but not for every router--check the label on your router, or look up the manual for its specific model number)
3. **Give your computer a static IP**: assign your machine (the device with your MAC address) a static local IP address. There are a lot of things you could choose, one of the conventions being something in the 192.168.0.0 – 192.168.255.255 range (make sure it is different than your router IP though).
4. **Set up port forwarding**: in your router settings, find Port Forwarding, then forward ports `6969` and `6970` (which can usually be written as `6969,6970`) to the static local address you just assigned to your computer. This step will ensure that any traffic coming in on those ports will be directed to your computer (by default, those ports are blocked by your router).
5. **Set up your firewall**: open (or _allow_) incoming TCP traffic ports `6969` and `6970` in Windows firewall settings (or whatever firewall you use). This step will ensure that your computer will accept traffic on those ports.
6. **Secure your ports**: this step is not necessary, but recommended for security: whitelist your friends' IP addresses (found using whatismyip) in your router settings. Depending on your router, this may need to be done on the same screen as step 4, or it may be on a separate screen; for example, on ASUS routers, you must set the `source IP` to your friend's address on the port forwarding screen. You will need to do this for each friend you want to play with. If a friend is unable to connect in the future, you may have forgotten to do this step for their IP. This step will ensure that only your friends can connect to your server, and not just anyone on the internet. If you don't do this step, anyone on the internet will be able to connect to your server, which is not recommended. **Note:** you will need to whitelist your own internal IP address as well, or you won't be able to connect to your own server!
7. **Update HTTP config**: go to `SIT_DIR\server\Aki_Data\Server\configs` and open `http.json` in a text editor. Change the `ip` value to the static local IP you assigned to your machine. This step will allow you to connect to your server from your own computer
8. **Update co-op config**: go to `SIT_DIR\server\user\mods\SIT.Aki-Server-Mod-master\config` and open `coopConfig.json` in a text editor. Change the `externalIP` value to the IP given by whatismyip. This step will allow your friends to connect to your server.

Now your Server is all set up! To connect to your own server, run the SIT launcher and enter your local static IP address, like `http://{ your local static IP }:6969`. Your friends will connect using the IP given by whatismyip, like `http://{ your external IP }:6969`.
Optionally, set `logRequests` to `false` in `SIT_DIR/server/Aki_Data/Server/configs/http.json` to prevent log spam.

## Hosted with Hamachi VPN

### Server
1. Run Hamachi.
2. Find the IPv4 address shown in the LogMeIn Hamachi widget and copy it. We will use `100.10.1.10` as an example IP for this guide.
3. Open the coop server configuration file in `SIT_DIR/server/user/mods/SIT.Aki Server-Mod/config/coopConfig.json`.

    *The `coopConfig.json` file is automatically created when the server mod is run the first time. Run `Aki.Server.exe` to create the file. Stop and close the server once the file has been created so we can continue the installation process.*

    *Note: Make edits to the file using Notepad or a text editor that won't introduce formatting. Do not use Microsoft Word.*
4. Set `externalIP` to the IP we copied from LogMeIn Hamachi: `100.10.1.10`.
5. Set `useExternalIPFinder` to `false`.
6. Open SPT-AKI's server connection configuration file in `SIT_DIR/server/Aki_Data/Server/configs/http.json`.

    *Note: Make edits to the file using Notepad or a text editor that won't introduce formatting. Do not use Microsoft Word.*
7. Set `ip` to `100.10.1.10`.
8. Optionally, set `logRequests` to `false` to prevent log spam.

### Launcher
Connect using the IPv4 address shown in the LogMeIn Hamachi widget. Our example would use `http://100.10.1.10:6969` as the server.

# Starting a game

## 1. Start the server

Run `Aki.Server.exe` from `SIT_DIR/server`.

## 2. Start the game

Launch the game via the `SIT Launcher`.

*The first time you try to connect with new credentials, you will be prompted to create the account, click “Yes” (passwords are stored in plain text, do not reuse passwords). You may also be prompted to Alt+F4 after the game launches, if so, close the game and relaunch through SIT Launcher.*

## 3. Create a Lobby

See the HOSTING.md for your language to learn how to create a lobby.
HOSTING guides can be found here: https://github.com/paulov-t/SIT.Core/tree/master/wiki

## Additional Notes
1. your friends do not need to set up the server at all. They only need to install SIT using the launcher and connect to your server.
2. it is recommended that all players, both you and your friends, use the same version of SIT. This can be done by checking the "Force install latest SIT" option in the settings menu in the launcher.
3. if you leave your server running all the time (which requires you to turn off sleep mode on your machine), your friends will be able to connect any time. They can edit their loadouts, use the flea market, enter their hideout, etc. and even play solo raids without you being present. Note that if two raids are going on at the same time, both raids will end when one of the raid ends.
4. you can find additional config options in `SIT_DIR\game\BepInEx\config\SIT.Core.cfg`, where you can, for instance, disable the spawn/kill feed in the bottom right corner of the screen while in raid. As these are client (as opposed to server) options, it is strongly recommended that all players use the same config options.
