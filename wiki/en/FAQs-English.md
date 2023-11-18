## Infinite "Loading profile data..." screen

Caused by:
- A broken installation.
- An issue with the server connection, related to your IP settings.
- Windows proxy blocking connection.

Make sure port forwarding is setup correctly so that you can connect to your external IP.

If you are playing Single Player, do not use any of these options and leave it as 127.0.0.1 and turn the External IP Finder off.

See: 
- [Discussion#139](https://github.com/paulov-t/SIT.Core/discussions/139)
- [Discussion#24](https://github.com/paulov-t/SIT.Core/discussions/24)
- [Issue#115](https://github.com/paulov-t/SIT.Core/issues/115)
- [Issue#60](https://github.com/paulov-t/SIT.Core/issues/60#issuecomment-1560461446)

---

## Where do I install mods?

### Client mods
Install client mods in `<game folder>/BepInEx/plugins/`.

### Server mods
Install server mods in `<server folder>/user/mods/`.

See:
- [Discussion#111](https://github.com/paulov-t/SIT.Core/discussions/111)
- [Discussion#134](https://github.com/paulov-t/SIT.Core/discussions/134)

---

## DDNS Setup step. If you don't have a static public IP address and you want to use a domain name to connect to the server.

### Step 1
Replace `"ip": "127.0.0.1"` in these two files with the IP Address of your computer's NIC address(__NOT YOUR PUBLIC IP ADDRESS__) 
or use 0.0.0.0 instead.

`<Server folder>/Aki_Data/Server/configs/http.json`

`<Server folder>/Aki_Data/Server/database/server.json`

and make sure these two file have same IP Address.

### Step 2
Locate the SIT.Aki-Server-Mod `coopConfig.json` in the `<Server folder>/user/mods/SIT.Aki-Server-Mod/config/` directory.

Change "externalIP": "127.0.0.1" to your domain name. For example: "externalIP": "yourdomain.com".

__Set useExternalIPFinder to false__

__Now you are good to go. Have some fun~__

---
