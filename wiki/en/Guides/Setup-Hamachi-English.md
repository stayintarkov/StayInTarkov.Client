# Installation guide

## This is simple guide how to get [SIT.Core](https://github.com/paulov-t/SIT.Core) up and running with friends using [Hamachi](https://www.vpn.net/) (or similiar program) and [SIT.Launcher](https://github.com/paulov-t/SIT.Launcher).

### READ THIS BEFORE YOU START!
You need to purchase and have an active install of [Escape From Tarkov](https://www.escapefromtarkov.com/) for this to work.

You need to install [Hamachi](https://www.vpn.net/) or something similiar. In this guide I am going to use Hamachi.

This guide has been tested for SIT.Core.Release-64 and SIT-Launcher.Release-71

This is the way I got this mod to work, there might be some steps that are unnecessary and this guide may get outdated really fast as the mod gets updated!

You can also apply this guide to scenarios where host has ports forwarded correctly, just skip the Hamachi parts and use hosts public Ipv4.

# Server install guide 
### Skip if your friend is hosting server.
1. Create same kinda folder structure as described in the client install guide.
But this time also create folder called "Server"\
SITCOOP/Server
2. Download and extract latest [SPT-AKI Stable Release](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases) to the Server folder.
3. Run the Aki.Server.exe, you can close it once it says "Happy playing".
4. Download [SIT.Aki server mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) and install that as you would install any other mod to AKI Server.\
The folder path to the mod should look like this:\
C:\SITCOOP\Server\user\mods\SIT.Aki-Server-Mod-master\
To make sure you dont have extra folders the SIT.Aki-Server-Mod-master should have package.json inside it.
5. Configure http.json and coopconfig.json as described in [Paulov-t's HOSTING.md](https://github.com/paulov-t/SIT.Core/wiki/Hosting-English)
If you are using Hamachi, use Hamachi Ipv4 for both http.json and coopconfig.json.\
*And still dont use 127.0.0.1 or Localhost!*
6. Start the Server (As Administator) and go to client install section.

# Installing SIT.Launcher and SIT.Core as a client.

1. Navigate to desired folder where you want to make the installation, for me I decided to install it to "C:\SITCOOP" folder.
2. Create "Game" and "Launcher" folders inside "SITCOOP" folder.
3. Download and extract SIT.Launcher to the "Launcher" folder.
4. Run SIT.Launcher.exe and setup the game to the "Game" folder we made earlier.
5. Once the install is complete you need to close the launcher.
6. Create folder called "AkiSupport" inside "Launcher" folder.
7. Create folders inside "AkiSupport" so that paths look like this:\
C:\SPCOOP\Launcher\AkiSupport\Bepinex\Patchers\
C:\SPCOOP\Launcher\AkiSupport\Bepinex\Plugins\
C:\SPCOOP\Launcher\AkiSupport\Managed\
6. If your friend is hosting the server copy their Hamachi Ipv4 from the Hamachi client, if you are the host copy your own Hamachi Ipv4, **127.0.0.1 wont work!**
7. Start the launcher and change the server field to have your friends Ip in it.\
Example: http://100.100.100.100:6969
8. Input desired username and password. 

**The username and password is stored as a plain text on the host's computer, do not use any username / password you use somewhere else or do not wish for the host to see!**

9. Go to launchers settings and make sure you have "Automatically Install SIT", "Force Install Latest SIT..." and "Automatically Install AKi Support" enabled.

**Congratulations you now have a fresh install of SIT.**

**Enjoy**

Created by ppyLEK *(small edits by SlejmUr)*