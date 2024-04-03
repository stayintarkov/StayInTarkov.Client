
<div align=center style="text-align: center">
<h1 style="text-align: center">StayInTarkov.Client</h1>

此 BepInEx 模组适用于逃离塔科夫, 与 [SIT.Aki-Server-Mod](https://github.com/stayintarkov/SIT.Aki-Server-Mod) 一同使用, 以在"离线模式"内进行联机
</div>

---

<div align=center>

![GitHub all releases](https://img.shields.io/github/downloads/stayintarkov/StayInTarkov.Client/total) ![GitHub release (latest by date)](https://img.shields.io/github/downloads/stayintarkov/StayInTarkov.Client/latest/total)

[English](README.md) **|** [简体中文](README_CN.md) **|** [Deutsch](README_DE.md) **|** [Português-Brasil](README_PO.md) **|** [日本語](README_JA.md) **|** [한국어-Korean](README_KO.md) **|** [Français](README_FR.md)
</div>

---

## Stay In Tarkov 的当前状态

* Stay In Tarkov 正在由"SIT项目组"有序开发中
* 如您希望对本项目做出贡献, 欢迎发起"合并请求", 待项目组确认贡献有效后, 您的合并请求将会被批准并加入项目中

--- 

## 关于

Stay In Tarkov 项目的诞生是因为 Battlestate Games' (BSG) 迟迟不愿推出只有 PvE 的逃离塔科夫.
本项目的目标非常单纯, 即创造一个 PvE 的逃离塔科夫.
如果 BSG 推出了"只有 PvE 的逃离塔科夫", 或本项目收到了《数字千年版权法》的侵权通知, 本项目将立即终止.


## 免责声明

* 您必须拥有一份正版的逃离塔科夫才可使用本项目, 您可前往他们的官网进行购买. [https://www.escapefromtarkov.com](https://www.escapefromtarkov.com)
* 本项目绝对不是为了方便作弊者 (本项目的诞生反而是因为作弊者破坏了正版的游戏体验)
* 本项目绝对不是为了允许用户持有盗版的逃离塔科夫 (并且会阻止盗版用户使用本项目!)
* 本项目仅供学习交流使用. 我通过本项目学习如何善用 Unity 引擎及 TCP / UDP / WebSocket 基础知识, 并且从 BattleState Games 那里学到了很多东西 \o/.
* 我不隶属于 BSG 或其他声称正在进行开发的项目, 请勿在 SPTarkov 的 Reddit 子板块或他们的 Discord 群组谈起本项目.
* 本项目不隶属于 SPTarkov (或称 SPT-Aki, 即"逃离塔科夫离线版"), 但是会使用他们所制作的服务端.
* 本项目不隶属于任何第三方"逃离塔科夫离线版"项目.
* 本项目"按原样"进行提供. 无法做出"绝对能用"的保证.

## 支持

* 欢迎发起合并请求改进这个项目！感谢所有贡献者的付出！
* [SIT Discord 群](https://discord.gg/f4CN4n3nP2) 现已开放. 我们的社区群体会在这里互相帮助.

## SPT-Aki 说明
* Stay In Tarkov 需要配合 [最新的 Aki 服务端](https://dev.sp-tarkov.com/SPT-AKI/Server) 以进行联机. 您可以前往 [他们的官网](https://www.sp-tarkov.com/) 来了解 SPT-Aki 的更多信息.
* __**注意:**__ **请勿**将 SIT 客户端模组安装到 *SPT-Aki 的客户端* —— 它应被安装到一个纯净的逃离塔科夫. 有关更多信息, 请参阅本自述文件的 [安装教程->客户端](#%E5%AE%A2%E6%88%B7%E7%AB%AF) 部分.

## [中文维基](https://github.com/stayintarkov/StayInTarkov.Client/wiki/%E4%BB%8B%E7%BB%8D(Intro)-Home)
**维基均由不同的贡献者创造. 所有说明也都保存在维基目录中.**
  - ### [部署教程](https://github.com/stayintarkov/StayInTarkov.Client/wiki/%E9%83%A8%E7%BD%B2%E6%95%99%E7%A8%8B-Guides)
  - ### [疑难解答](https://github.com/stayintarkov/StayInTarkov.Client/wiki/%E7%96%91%E9%9A%BE%E8%A7%A3%E7%AD%94-FAQs)


## 安装教程

### 概述
SIT 由2个主要部分和启动器组成:
- [SIT 适用于 SPT-Aki 的服务端模组](https://github.com/stayintarkov/SIT.Aki-Server-Mod)
- SIT 客户端模组 (本项目), 需要安装在游戏本体内
- 启动器: [SIT Manager](https://github.com/stayintarkov/SIT.Manager.Avalonia) (<s>[SIT Manager](https://github.com/stayintarkov/SIT.Manager) 或 [SIT Launcher Classic](https://github.com/stayintarkov/SIT.Launcher.Classic)</s> 均已存档)
  - <s>您应优先使用 SIT Manager。提及经典启动器仅为向现有的经典启动器用户澄清。</s>

建议使用以下结构命名文件夹, 这将方便你区分它们的不同部分.
```
SIT/
├── server/      # SPT-Aki 服务端
├── game/        # 逃离塔科夫本体
└── launcher/    # 启动器 "SIT Manager" 或 "SIT Classic Launcher"
```

### 服务端
- 请参考 SIT.Aki-Server-Mod 的 [自述文件](https://github.com/stayintarkov/SIT.Aki-Server-Mod#readme) 了解如何安装服务端模组.
- 只有服主需要运行服务端, 而服主需要进行端口转发, 或者使用 Hamachi, Radmin 等软件搭建虚拟局域网进行联机. 如果您感到困惑, 请加入我们的 Discord 获取更多帮助.
  - 您可能已经习惯了游玩 SPT-Aki 时连接自己运行的服务端, 但这会导致问题. 只有一个人需要运行服务端, 而其他人则需要连接到此服务端进行联机.

### 启动器
- 请参考 [SIT.Manager 的自述文件](https://github.com/stayintarkov/SIT.Manager#readme) 或 [SIT.Launcher.Classic 的自述文件](https://github.com/stayintarkov/SIT.Launcher.Classic#readme) 了解如何安装启动器.

### 客户端
- **所有人**都必须安装 SIT 客户端模组. 您可以使用启动器进行快速安装, 或前往 [发行版页面](https://github.com/stayintarkov/StayInTarkov.Client/releases/latest) 手动下载安装.
- **如果你已有 SPT-Aki 的客户端:** **请勿**将 SIT 安装在 Aki 的客户端内. SIT 客户端模组与 SPT-Aki 不兼容, 你需要使用一份纯净的逃离塔科夫.

### 实际游玩
- **服主**: 搭建好可以正常运行的 SPT-Aki 服务端, 并已完成端口转发或连接上虚拟局域网
  - 确保已按照 SIT 服务端模组的自述文件修改了 IP 地址!
- **玩家**: 打开启动器, 输入服主所提供的 IP 地址及端口并连接.
  - 任何人均可以成为"房主", 只需选择好地图, 点击"主持战局", 即可创建新战局, 其他人可以直接加入战局.


## 疑难解答 (FAQ)

### SIT 用的是正版的"练习组队模式"的代码吗?
不是. 出于显而易见的原因, BSG 的服务器代码在客户端中是找不到的. 因此, BSG 的"练习组队模式"是直接使用了他们的服务器. 我们触碰不到代码, 因此无法使用.

### SIT 兼容哪些 SPT-Aki 的模块?
以下 Aki 模块已完成兼容.
- aki-core
- Aki.Common
- Aki.Reflection

### SPT-Aki 的那些客户端模组是否可以直接使用?
这取决于模组的具体编写方式. 如果它们是直接使用代码中的 "GClass***" 或 "public" / "private" 变量那大概率是无法使用的.

### 为什么不直接使用 SPT-Aki 的模块?
Aki 的模块是专门为其自身而设计的, 它们所使用的反混淆技术与 Paulov 的技术之间无法兼容.

## 如何编译 SIT ?
请参阅 [编译手册](COMPILE.md)

## 致谢名单
- [Paulov](https://github.com/paulov-t) (Stay in Tarkov 的原作者)
- SPT-Aki 项目组 (每一个所使用的 Aki 代码均保留了 Aki 的注释信息, 非常感谢每位开发者的支持)
- SPT-Aki 模组社群
- DrakiaXYZ ([BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain) 与 [Waypoints](https://github.com/DrakiaXYZ/SPT-Waypoints) 模组已内置进本项目)
- SIT 项目组与所有曾经为 SIT.Core 做出过贡献的贡献者

## 许可
- DrakiaXYZ 的项目均为 MIT 许可证
- 几乎所有的单人游戏与其核心代码为 SPT-Aki 编写. 本项目使用与其相同的开源许可: NCSA 开源许可证
- Paulov 所编写的代码没有许可. 但是"没有许可"并不意味着你可以随意使用它
- SIT 项目组所编写的代码为 MIT 许可证
