## 无限卡在'正在加载配置文件'界面

可能的原因:

- 游戏文件有问题.
- 与服务器连接有问题,与你的网络设置相关.
- 检查是不是开了梯子

请正确地设置好端口转发,否则无法连接至你的服务器.

如果你只是一个人玩单机,请勿调整相关设置并关闭External IP Finder.


详见:
- [Discussion#139](https://github.com/paulov-t/SIT.Core/discussions/139)
- [Discussion#24](https://github.com/paulov-t/SIT.Core/discussions/24)
- [Issue#115](https://github.com/paulov-t/SIT.Core/issues/115)
- [Issue#60](https://github.com/paulov-t/SIT.Core/issues/60#issuecomment-1560461446)

---

## 我该在哪装mod啊?

### 客户端mod

安装在 `<游戏目录>/BepInEx/plugins/`.

### 服务器mod

安装在 `<服务器目录>/user/mods/`

详见:
- [Discussion#111](https://github.com/paulov-t/SIT.Core/discussions/111)
- [Discussion#134](https://github.com/paulov-t/SIT.Core/discussions/134)

---

## 此步骤为DDNS配置.如果你没有静态公网IP并且想用域名连接至服务器.

### 步骤 1

将这2个文件中的 `"ip": "127.0.0.1"` 替换为你电脑的网卡地址(__不是你的公网IP或域名__) 或者直接使用0.0.0.0

`<Server folder>/Aki_Data/Server/configs/http.json`

`<Server folder>/Aki_Data/Server/database/server.json`

并请确认这2个文件有着相同的IP地址.

### 步骤 2
在`<Server folder>/user/mods/SIT.Aki-Server-Mod/config/`目录下找到SIT.Aki-Server-Mod `coopConfig.json`

将"externalIP": "127.0.0.1" 替换为你的域名.

例如: "externalIP": "yourdomain.com"

__将useExternalIPFinder设置为false__

__现在你就整好了.开玩!__

---
