
<div align=center style="text-align: center">
<h1 style="text-align: center"> SIT.Core </h1>
逃离塔科夫的BepInEx MOD，为了配合SPT-Aki在逃离塔科夫离线模式下进行合作游戏。
</div>

---

<div align=center>

![GitHub all releases](https://img.shields.io/github/downloads/paulov-t/SIT.Core/total) ![GitHub release (latest by date)](https://img.shields.io/github/downloads/paulov-t/SIT.Core/latest/total)

[English](README.md) **|** [Deutsch](README_DE.md) **|** [Português-Brasil](README_PO.md) **|** [简体中文](README_CN.md)
</div>

---
## 关于这个项目

SIT(Stay in Tarkov) 项目是因为 Battlestate Games (BSG) 不愿推出纯PVE塔科夫版本，所以这个项目就诞生了。

该项目的目标很单纯，创建一个可以保存进度的塔科夫合作离线PVE模式，如果BSG官方决定在线版本中提供PVE合作模式**或**我收到了DMCA TAKEDOWN(DMCA删除请求)该项目会立刻关停。

## 免责声明

* 你必须购买一份游戏才能使用这个项目，你可以在这里购买游戏[https://www.escapefromtarkov.com](https://www.escapefromtarkov.com). 
* 该项目不是为了在游戏中作弊开发的 (正相反，因为正版多人环境中泛滥的作弊者催生了该项目)
* 这个项目也不是为了让人获取盗版/非法的副本存在的 (而且也会阻止非正规途径获取的游戏启动)
* 该项目仅供学习，我利用这个项目学习Unity与TCP/UDP/WebSocket开发，而且我在BattleState Games那边学到了许多\o/
* 我也与BSG或其他 (在贴吧/QQ/Bilibili等) 声称自己在进行的项目毫无关系，请不要去SPTarkov的Reddit板块与Discord服务器讨论SIT
* 该项目与SPTarkov无关，但使用了由他们开发的出色的服务端程序
* 2023年7月30日后，我将不再进行用户支持，这个项目转变为“现状”交付，这意味着 **你能用就用，用不了就算了**

## 支持我

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/N4N2IQ7YJ)
* 请看清楚，在ko-fi上给我打钱就只是请我喝杯咖啡，没别的！**我不会提供任何支持与帮助** 
* 请不要期待通过给我打钱的方式获得任何帮助或者解决方案 **我不会提供任何支持与帮助** 
* 这只是一个兴趣爱好的项目，不要太认真 **我不会提供任何支持与帮助**
* 我不会对社区撒谎，我知道目前这只是个半成品项目，但我会尽我所能的去修复完善它 **我不会提供任何支持与帮助**
* 2023年7月30日后，Issues(问题反馈) & Discussions(讨论区) 将被关闭
* __欢迎各位的PR，感谢提出Pull Requests的贡献者们__

## SPT-AKI 需求
* Stay in Tarkov 需要 [最新的SPT-AKI服务器](https://dev.sp-tarkov.com/SPT-AKI/Server) 来运行. 你可以通过这个链接来了解什么是 SPT-Aki [here](https://www.sp-tarkov.com/).
* 不要把SPT-AKI客户端模块与SIT同时安装! 只需要安装服务端模块即可!

## [中文Wiki](https://github.com/paulov-t/SIT.Core/wiki/%E4%BB%8B%E7%BB%8D(Intro)-Home)
  - ### [部署教程](https://github.com/paulov-t/SIT.Core/wiki/%E9%80%9A%E5%B8%B8%E9%83%A8%E7%BD%B2%E6%96%B9%E5%BC%8F-SETUP-STANDARD)
  - ### [疑难解答](https://github.com/paulov-t/SIT.Core/wiki/%E7%96%91%E9%9A%BE%E8%A7%A3%E7%AD%94-FAQs)

## 合作相关

### 合作模式目前的进度
**请注意**
* 合作模式目前处在极早期开发中. 
* 大部分的功能是可用的, 游戏在某种程度上是可以玩的。
* 但是“可以玩”与“完美运行”是完全不同的概念，玩家间的数据不同步，各种问题与bug是预期范围内的。
* 我测试了所有地图，目前效果最好的两张图是`工厂`与`实验室` 。
* 服务器与主机与客户端的网络状态/硬件配置，以及主机上的AI数量会剧烈影响游戏性能
* 更多有关运行服务器与游戏合作注意事项在 [如何开打](https://github.com/paulov-t/SIT.Core/wiki/%E5%BC%80%E5%A7%8B%E4%B8%80%E5%9C%BA%E6%B8%B8%E6%88%8F-HOSTING)这里

### 预先准备
你必须将 [SPT-Aki mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) 安装到你的SPT-AKi服务器上来让mod工作。

如果你不需要使用Coop功能，则你必须要在BepInEx配置文件中禁用掉


### 合作mod可以使用BSG官方的代码吗?
不行。因为BSG的离线训练模式与在线的PvPvE共享相同的服务器代码，而且BSG官方明显是不会开放任何代码的，所以我们用不了。

### 代码相关解释
- 项目使用多种BepInEx Harmony补丁对Unity组件修改来实现功能
- 那些需要不断在客户端与服务器之间同步的数据 (移动，视角变换等等) 使用组件来传输数据 (AI的代码在每一帧都执行一遍Update/LateUpdate命令和函数，从而导致大量的网络数据传输)
- 那些可以被轻松 "复现" 的功能与方法则使用 ModuleReplicationPatch 抽象类处理 以实现双向调用。
- 服务器所有的通信都通过JSON TCP Http 与 Web Socket 调用由SPT-Aki开发的 ["Web Server"](https://dev.sp-tarkov.com/SPT-AKI/Server) 来处理，使用[SIT.Aki-Server-Mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) 来处理后端相关工作
- 当一个合作游戏开始时（除了藏身处），CoopGameComponent会附加到GameWorld对象上，并轮询数据传递给PlayerReplicatedComponent



## SPT-Aki

### 有哪些Aki模块是可以一起工作的?
下列的Aki模块是支持与SIT一起工作的.
- aki-core
- Aki.Common
- Aki.Reflection
- 对于SPT-AKI客户端mod来说，取决于如何编写的patch，如果直接针对GCLASSXXX或PUBLIC/PRIVATE，那大概行不通

### 为什么你不直接使用Aki模块的DLL?
SPT-Aki 的DLL是针对他们自己的反混淆方式编写的，而我的反混淆方式与Aki的模块目前不太兼容，所以我目前移植了很多来自SPT-Aki模块的功能，但我的目标是依赖于SPT-Aki，这样我好专注于SIT本身


## 如何编译这个项目? 
[编译文档](COMPILE.md)

# 如何安装BepInEx
[https://docs.bepinex.dev/articles/user_guide/installation/index.html](https://docs.bepinex.dev/articles/user_guide/installation/index.html)

## 安装到塔科夫副本上
BepInEx 5 必须安装好并生成了配置文件 （详见如何安装BepInEx）
将编译好的.dll放置在 BepInEx plugins目录

## Test in Tarkov
- 打开你已经安装好BepInEx的塔科夫文件夹，打开BepInEx文件夹
- 打开config目录
- 打开BepInEx.cfg文件
- 将 [Logging.Console] 选项设置为 True
- 保存修改后的配置文件
- 通过启动器启动塔科夫或者使用bat启动塔科夫，如下所示 (记得替换为你自己的token)
```
start ./Clients/EmuTarkov/EscapeFromTarkov.exe -token=pmc062158106353313252 -config={"BackendUrl":"http://127.0.0.1:6969","Version":"live"}
```
- 如果BepInEx正确安装，应该会打开一个控制台窗口并显示插件已经启动了


## 感谢列表
- SPT-Aki team
- MTGA team
- SPT-Aki Modding Community
- Props (AIBushPatch, AIAwakeOrSleepPatch - Currently unused)
- kmyuhkyuk (GamePanulHUD - Unused)

## 关于授权

- 95% 的单人游戏与核心功能由SPT-Aki团队完成。 在他们的源码中可能涉及到相关的许可证
- 我所做的工作没有谁许可，就图一乐，你想做什么我不管
