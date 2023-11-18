
# 保姆级SIT安装教程

# 前置要求

在开始之前请确认,你逃离塔科夫游戏文件是经BSG启动器下载的最新版本.SIT无法在非最新或盗版来源的逃离塔科夫文件下运作.


此教程中的 `SIT_DIR` 指的均是安装SIT的根目录.此目录下请新建以下3个文件夹:

-`server` 用于SPT-AKI服务器

-`launcher` 用于SIT启动器

-`game` 用于逃离塔科夫游戏文件

*解压请使用[7zip](https://7-zip.org/)或WinRAR之类的工具.


# 安装


## 1. [SIT启动器](https://github.com/paulov-t/SIT.Launcher/releases) (自动安装)


1. 在[Releases](https://github.com/paulov-t/SIT.Launcher/releases) 下载最新版的`SIT Launcher`
2. 将文件解压缩至 `SIT_DIR/launcher`
3. 启动 `SIT.Launcher.exe`
4. 第1次启动时,会有以下安装提示跳出:
    
    *"No OFFLINE install found. Would you like to install now?"* 
    
    点击"Yes"
5. 将安装根目录设置为 `SIT_DIR/game`
6. 启动器会自动安装,请耐心等待

## 2. [SPT-AKI服务器](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases)

1. 在[Releases](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases) 页面下载最新版的 `SPT-AKI Server`.
2. 解压缩文件至 `SIT_DIR/server`.
## 3. [SIT服务器Mod](https://github.com/paulov-t/SIT.Aki-Server-Mod)
1. 从[GitHub](https://github.com/paulov-t/SIT.Aki-Server-Mod) 下载服务器mod的zip文件 (那个大绿按钮底下: *Code > Download Zip*).
2. 解压缩文件至 `SIT_DIR/server/user/mods`.
3. 
        *`user/mods` 目录会在服务器第1次运行时自动创建. 运行`Aki.Server.exe` 即可创建此文件夹. 在目录被创建后,请停止并关闭服务器,并继续进行安装.*

# 服务器设置

## 基于localhost (用于测试)

### 服务器
1. 打开位于 `SIT_DIR/server/user/mods/SIT.Aki Server-Mod/config/coopConfig.json` 的合作服务器调试文件.

    *`coopConfig.json` 文件会在服务器mod第1次运行时自动创建. 运行 `Aki.Server.exe` 以创建该文件. 在文件被创建后,请停止并关闭服务器,并继续进行安装.*

    *请注意:请不要使用Word编辑此文件,去用不会破坏格式的软件,比如Notepad.*
2. 将 `externalIP` 设置为 `127.0.0.1`.
3. 将 `useExternalIPFinder` 设置为 `false`.
4. *此条可选. 在 `SIT_DIR/server/Aki_Data/Server/configs/http.json`将 `logRequests` 设置为 `false` 以避免日志刷屏.

### 启动器
将服务器地址设置为 `http://127.0.0.1:6969` 并连接

*使用localhost将导致其他人无法加入你的游戏,但在检查连接问题时很有用.用这个方法来确认你游戏和mod装好没有.
## 用端口转发来联机

### 服务器
将 `useExternalIPFinder` 设置为 `false`
百度“IP地址”查询自己的公网IP，或是在ip.cn下找到自己的公网IP

将 `externalIP` 设置为你的公网IP地址，例如`"externalIP": "172.16.0.1"`.

*此条可选. 在 `SIT_DIR/server/Aki_Data/Server/configs/http.json` 将 `logRequests` 设置为 `false` 以避免日志刷屏.

### 启动器
用百度“IP地址”查询自己的IP，或是在ip.cn下找到自己的IP来进行连接，填写的IP地址应该与服务器中externalIP相同，如果觉得每次修改IP麻烦请参考[DDNS配置](https://github.com/paulov-t/SIT.Core/wiki/%E7%96%91%E9%9A%BE%E8%A7%A3%E7%AD%94-FAQs#%E6%AD%A4%E6%AD%A5%E9%AA%A4%E4%B8%BAddns%E9%85%8D%E7%BD%AE%E5%A6%82%E6%9E%9C%E4%BD%A0%E6%B2%A1%E6%9C%89%E9%9D%99%E6%80%81%E5%85%AC%E7%BD%91ip%E5%B9%B6%E4%B8%94%E6%83%B3%E7%94%A8%E5%9F%9F%E5%90%8D%E8%BF%9E%E6%8E%A5%E8%87%B3%E6%9C%8D%E5%8A%A1%E5%99%A8)

## 用Hamachi VPN来联机

### 服务器
1. 运行 Hamachi.
2. 找到显示在 Hamachi 小部件 LogMeIn 里的IPv4地址并复制.此次教程我将使用 `100.10.1.10` 为例子.
3. 打开位于 `SIT_DIR/server/user/mods/SIT.Aki Server-Mod/config/coopConfig.json` 的合作服务器调试文件.

    *`coopConfig.json` 文件会在服务器mod第1次运行时自动创建. 运行 `Aki.Server.exe` 以创建该文件. 在文件被创建后,请停止并关闭服务器,并继续进行安装.*
    
    *请注意:请不要使用Word编辑此文件,去用不会破坏格式的软件,比如Notepad.*
4. 将`externalIP` 设置为从 LogMeIn 复制的 `100.10.1.10`
5. 将 `useExternalIPFinder` 设置为 `false`.
6. 打开位于 `SIT_DIR/server/Aki_Data/Server/configs/http.json`的 SPT-AKI 服务器连接调试文件.
    *请注意:请不要使用Word编辑此文件,去用不会破坏格式的软件,比如Notepad.*
7. Set `ip` to `100.10.1.10`.

7. 将 `ip` 设置为 `100.10.1.10`(此处为例子,不是你的IP).
8.*此条可选. 将 `logRequests` 设置为 `false` 以避免日志刷屏.

### 启动器

找到显示在 Hamachi 小部件 LogMeIn 里的IPv4地址并复制.此次教程我将使用 `http://100.10.1.10:6969` 为例子.

# 如何开始玩

## 1. 开启服务器

运行 `Aki.Server.exe`

## 2. 打开游戏

用 `SIT Launcher` 开启游戏.

*第1次用新账号密码登录时,启动器会提示你新建账户,点击'Yes'(密码存储没有加密,不要用你以前用过的密码).游戏开启后有可能会提示你 Alt+F4, 倘若如此, 关闭游戏并重新从SIT启动器开始游戏.

## 3.创建战局

请查看[如何加入其他人的对战](https://github.com/paulov-t/SIT.Core/wiki/%E5%BC%80%E5%A7%8B%E4%B8%80%E5%9C%BA%E6%B8%B8%E6%88%8F-HOSTING#%E5%A6%82%E4%BD%95%E5%8A%A0%E5%85%A5%E5%85%B6%E4%BB%96%E4%BA%BA%E7%9A%84%E5%AF%B9%E6%88%98).
