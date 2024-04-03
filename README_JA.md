
<div align=center style="text-align: center">
<h1 style="text-align: center">StayInTarkov.Client</h1>
SPT-Akiサーバー基盤の「オフライン」協力プレイを目標としたEscape From TarkovのBepInExモジュール
</div>

---

<div align=center>

![GitHub all releases](https://img.shields.io/github/downloads/stayintarkov/StayInTarkov.Client/total) ![GitHub release (latest by date)](https://img.shields.io/github/downloads/stayintarkov/StayInTarkov.Client/latest/total)

[English](README.md) **|** [Deutsch](README_DE.md) **|** [简体中文](README_CN.md) **|** [Português-Brasil](README_PO.md) **|** [日本語](README_JA.md) **|** [한국어-Korean](README_KO.md) **|** [Français](README_FR.md)
</div>

---

## Stay In Tarkovの進行状況

* Stay In Tarkovの続きは、SIT開発チームによる開発が今も続いています。
* プルリクはいつでも開いていますので草生やしにご協力お願いします！

--- 

## このプロジェクトについて

ステイ・イン・タルコフ（Stay In Tarkov）プロジェクトはBattlestate Gamesが普段のPvE（プレイヤー対環境）バージョンのタルコフにすることを向かないから誕生しました。
このプロジェクトの目標は単純です。プレイヤーと協力するPvEの経験を目指すことになります。
もし、BSGがライブサーバーでこの機能が追加することを決めたことになったらこのプロジェクトは直ちに終了することになります。

## 免責条項

* 使用する為、このゲームを購入する必要があります。ここで購入できます。[https://www.escapefromtarkov.com](https://www.escapefromtarkov.com)
* このプロジェクトは決して不正行為（チート）の為に設計されたことじゃありません。（このプロジェクトはチートがライブサーバーの経験を破壊したので作られました）
* このプロジェクトは決してゲームを無断にダウンロードする為のことではありません。（そして、無断ダウンロードの防止もあります！）
* これはただの教育をする目的です。（Unityとルバースエンジニアリングおよびネットワーキングを習う為に使用しています）
* 私はBSGと他の人（RedditやDiscordなど）から進行すると主張するプロジェクトと少しも関われていません。
* このプロジェクトはSPT-Akiと提携などが行われていませんが、非常に良いサーバーの利用を頂いています。
* このプロジェクトはEFTについ、他のエミュレーターにも提携などが行われていません。
* This project comes "as-is". It either works for you or it doesn't.

## サポート

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/N4N2IQ7YJ)
* 注意してください。このKo-Fiのリンクは私にコーヒーを買ってくれる為に使います。その以外はなにもないです！
* 私が作成したコードはすべてここにあります。
* 助けや解決先を為にお金は払わないことにして下さい。
* これはただ趣味で、楽で作成するプロジェクトです。軽く受け入れて下さい。
* 私はコミュニティに騙すことはしません。これが半分失敗したこととしても私は精一杯で直していこうと思います。
* [SIT Discord](https://discord.gg/f4CN4n3nP2) がご利用になれます！現在、コミュニティはチームを組んだり、お互いに助け合ったり、コミュニティサーバーを作成しております！
* 日本語読者の皆さん！SITでのプロジェクトの文書などはまだ英語のままで作成されております。日本語の翻訳への一歩踏み出しはいかがでしょうか？

## SPT-AKI 要件
* Stay in Tarkovは[AKIサーバー最新版](https://dev.sp-tarkov.com/SPT-AKI/Server)を使用して動作できます。SPT-Akiについては[こっち](https://www.sp-tarkov.com/)で詳しく調べます。
* このプロジェクトをSPT-AkiのClientにインストールしないようにご注意下さい！サーバーにインストールして下さい！

## [Wiki](https://github.com/stayintarkov/StayInTarkov.Client/wiki/Home-Japanese)
**Wikiは様々な人によって作成されてますので壊れてしまう可能性もあります！そして全てのWikiソースはDirectoryに保管されます。**
  - ### [セットアップのマニュアル](https://github.com/stayintarkov/StayInTarkov.Client/wiki/Guides-Japanese)
  - ### [FAQs](https://github.com/stayintarkov/StayInTarkov.Client/wiki/FAQs-Japanese)


## インストール

### 概要
SITは２つのパーツとランチャーで構成されています。
- [SIT SPT-Akiのサーバー MOD](https://github.com/stayintarkov/SIT.Aki-Server-Mod)
- SIT Client モジュール （このリポジトリ！）、インストールされた
- [SIT Manager](https://github.com/stayintarkov/SIT.Manager.Avalonia) (<s>[SIT Manager](https://github.com/stayintarkov/SIT.Manager) および [SIT Launcher Classic](https://github.com/stayintarkov/SIT.Launcher.Classic)</s> 両方ともアーカイブされました)
  - <s>SIT Managerの方が使い安く、MODの管理やサーバーの管理も気楽でオールインワンです。SIT Launcher Classicの方は元々のランチャーです。</s>

下記のインストールの仕組みをおすすめします。この構成は見やすいでファイルへの接近も用意でしょう
```
SIT/
├── server/      # SPT-Aki サーバーのMOD
├── game/        # EFT Client
└── launcher/    # SIT Manager および Classic Launcher
```

### サーバーのインストール
- Follow the instructions in the [SIT SPT-Aki Server Mod repo](https://github.com/stayintarkov/SIT.Aki-Server-Mod) to install and configure the server into the `SIT/server` folder.
- Exactly *one* person needs to run the server for Coop. This person will need to port forward, or your group will have to connect via Hamachi or some other VPN solution. If you don't know how to do these things, you might see if someone in the SIT discord is willing to help.
  - In vanilla SPT, you're probably used to running your own local server, and then launching your client which connects to that server under the hood. With SIT, one person will run the modded server and everyone else will connect to that server over the internet.

### ランチャーのインストール
- Follow the instructions in the SIT Manager repo. Install into the `SIT/launcher` folder.

### クライアントのインストール
- **Everyone** must install the SIT Client Mod. You can install it using SIT Manager, or manually if desired.
- **IF YOU USE SPT ALREADY**: Do **NOT** install the SIT Client mod onto your existing SPT install. The SIT Client Mod is currently not compatible with the SPT-Aki client, so it needs to be installed on it's own copy of Tarkov.

#### SIT Manager Method
- Copy the contents of your live EFT installation into the currently-empty `SIT/game` folder
  - If you installed tarkov to the default location, it will be under `C:\Battlestate Games\EFT`.
- Launch `SIT/launcher/SIT.Manager.exe`
- Point the manager to the target game directory
  - Open `Settings` in the bottom left
  - Set `Install Path` to `X:\<Full_Path_To>\SIT\game`, or use the Change button and select the `SIT/game` folder 
    - Replace `X` and `<Full_Path_To>` with the path to the folder.
  - Close Settings
- Open the `Tools` menu on the left, and select `+ Install SIT`
- Select the desired SIT version (choose the latest if you don't know what you're doing)
- Click `Install`  

<details>
 <summary>Manual install method</summary>
Note that these are the same steps the SIT Manager performs. If you don't have any reason to, you should probably just use the SIT manager- it's so much quicker and easier. (Seriously, we're not hiding anything from you here. These steps are literally just a plain-english description of [the manager code](https://github.com/stayintarkov/SIT.Manager/blob/master/SIT.Manager/Classes/Utils.cs#L613))

- Copy the contents of your live EFT installation into the currently-empty `SIT/game` folder
  - If you installed tarkov to the default location, it will be under `C:\Battlestate Games\EFT`.
- Create the folowing directories in `SIT/game`:
  - `SITLauncher/`
  - `SITLauncher/CoreFiles/`
  - `SITLauncher/Backup/Corefiles/`
- Download the desired version of the client-mod from this repo's [releases page](https://github.com/stayintarkov/StayInTarkov.Client/releases) to `SIT/game/SITLauncher/CoreFiles` (choose the latest if you don't know what you're doing)
  - Create the folder `SIT/game/SITLauncher/CoreFiles/StayInTarkov-Release/`
  - Extract the contents of the release archive into that folder
- Clean up your `SIT/game` directory
  - Delete the following files & directories:
    - `BattleEye/` \*
    - `EscapeFromTarkov_BE.exe` \*
    - `cache/`
    - `ConsistencyInfo`
    - `Uninstall.exe`
    - `Logs/`
  - \* In case of concern, note that this is not a method that can be used to cheat in live tarkov. SPT (and SIT, by extension) don't use the BattleEye executables/files because the SPT-Aki server does not run battleeye.
    Please be careful, and don't delete these files from your live directory. At best you'll brick your install & won't be able to connect to live servers. At worst you'll trigger a BattleEye detection and get your Account/IP/HWID marked for doing.
- Downgrade your copied tarkov if necessary
  - If your live tarkov's version isn't the same as the SIT version you chose in step 3, you need to downgrade.
    - Your live tarkov's version is the 5-part number in the bottom right of the BSG launcher.
  - SIT does not maintain the tools to downgrade tarkov. You can find instructions on downgrading tarkov [here](https://hub.sp-tarkov.com/doc/entry/49-a-comprehensive-step-by-step-guide-to-installing-spt-aki-properly/)
    - Follow steps 7, 8, 9. Use any folder for the "DowngradePatchers" folder, and use the `SIT/game` folder for the "SPTARKOV" folder.
	- If you run into issues here, SIT does not maintain the DowngradePatcher. You can contact the SPT devs about it, but understand that they won't provide support for anything else than the patcher- Do **NOT** ask them for help with other SIT topics, they *will not* help you.
      - That said, if whatever issue you have is legitimate and not just a simple error, the SIT team has *probably* already noticed & reported it. The SIT Manager uses the Downgrade Patcher too.
- Install BepInEx v5.4.22
  - Download [the archive](https://github.com/BepInEx/BepInEx/releases/download/v5.4.22/BepInEx_x64_5.4.22.0.zip)
  - Extract the contents to `SIT/game`
    - Your `SIT/game` folder should now contain a `BepInEx` folder, a `doorstop_config.ini` file, a `changelog.txt` file, and a `winhttp.dll` file.
  - Make the `SIT/game/BepInEx/plugins` folder
- Install SIT Client DLLs
  - Assembly-CSharp.dll
    - Make a backup of your original `SIT/game/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll` to `SIT/game/SITLauncher/Backup/CoreFiles/`
    - Replace the original dll with `SIT/game/SITLauncher/CoreFiles/StayInTarkov-Release/Assembly-CSharp.dll` 
  - Copy `SIT/game/SITLauncher/CoreFiles/StayInTarkov-Release/StayInTarkov.dll` to `SIT/game/BepInEx/plugins/`
- Install Aki Client DLLs
  - Download the latest SPT-AKI release from the [SPT releases page](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases)
  - Extract two files `EscapeFromTarkov_Data/Managed/Aki.Common.dll` and `EscapeFromTarkov_Data/Managed/Aki.Reflection.dll` from the release, into `SIT/game/EscapeFromTarkov_Data/Managed/`
And with any luck, you're done. 
</details>

### Playing
- **Server Host Only**: Have the server-host start up the modded server (port forwarded / tunneled via VPN or Hamachi to the rest of your group)
  - Make sure you configured your IP address(es) per the servermod repo instructions!
- **Everyone**: Start up the SIT Manager, enter the host's IP & port, and click play!
  - Anyone can start a raid lobby, just select the location/time/insurance, click Host Raid, configure the exact number of players & desired settings, and start it up. Everyone else will see the lobby pop up after you start, and will join then. (The game won't start till everyone's loaded, just like regular tarkov)


## FAQ

### 協力プレイはBSGのコードを使いますか？
いいえ、BSGサーバーのコードは明確な理由でクライアントから隠されてあります。
で、BSGの協力プレイサーバーの動作形はオンラインサーバーのPvPvEと同じようにできています。
これを見たり、使用したりは出来ません。

### Akiモジュール互換性
次のAkiモジュールが互換できます。
- aki-core
- Aki.Common
- Aki.Reflection

### SPT-AKIクライアントのMODの互換は？
半分のSPT-AKI基板のMOD。これはPatchがどのくらいよくできたことによってかわります。GCLASSXXXやPUBLIC/PRIVATEを直接ターゲットにしたらほぼ作動しません。

### なぜAkiのDLLモジュールを使いませんか？
SPT-AkiのDLLは作者によって独自の解読技術で作成され、私の技術で今はAkiモジュールによく動作しません。
だからSPT-Akiの多くの機能をこのモジュールに移植しました。私の最終の目標はSPT-Akiに依存し、これがSITの機能だけに集中にすることです。

## コンパイルする方法 
[コンパイル文書 (英語)](COMPILE.md)

## 感謝リスト
- SPT-Aki team (使用されたコードファイルについての提供のクレジットと、そのサポートに対する開発チームへぼ多くの感謝)
- MTGA team
- SPT-Aki Modding コミュニティ
- DrakiaXYZ ([BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain) & [Waypoints](https://github.com/DrakiaXYZ/SPT-Waypoints) がこのプロジェクトに合流されました)
- SITチームとコントリビューター

## ライセンス

- DrakiaXYZのプロジェクトはMITライセンスを含んでます。
- 99%の機能はSPT-Akiチームが完成しました。そっちのソースには関連されたライセンスがある可能性があるかもしれません。
- 私の作業はライセンスなどはありません。ただ楽しめるためのプロジェクトであります。あなたがこれで何をしても構わないです。
