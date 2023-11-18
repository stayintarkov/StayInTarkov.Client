
<div align=center style="text-align: center">
<h1 style="text-align: center"> SIT.Core </h1>
SPT-Akiサーバー基盤の「オフライン」協力プレイを目標としたEscape From TarkovのBepInExモジュール
</div>

---

<div align=center>

![GitHub all releases](https://img.shields.io/github/downloads/paulov-t/SIT.Core/total) ![GitHub release (latest by date)](https://img.shields.io/github/downloads/paulov-t/SIT.Core/latest/total)

[English](README.md) **|** [Deutsch](README_DE.md) **|** [简体中文](README_CN.md) **|** [Português-Brasil](README_PO.md) **|** [日本語](README_JA.md)
</div>

---

## SITの進行状況

** タルコフの0.13.5.*とSPT-Akiサーバの3.7.0について **
* 現在、SPT-Akiの側は不完全な状況で作業を続いています。SPT-AkiのDiscordにて一日的に作業を続いております。
* SITの開発の進捗は少しずつ伸びています。

--- 

## このプロジェクトについて

ステイ・イン・タルコフ（Stay In Tarkov）プロジェクトはBattlestate Gamesが普段のPvE（プレイヤー対環境）バージョンのタルコフにすることを向かないから誕生しました。
このプロジェクトの目標は単純です。プレイヤーと協力するPvEの経験を目指すことになります。
もし、BSGがライブサーバーでこの機能が追加することを決めたことになったらこのプロジェクトは直ちに終了することになります。

## 免責条項

* 使用する為、このゲームを購入する必要があります。ここで購入できます。[https://www.escapefromtarkov.com](https://www.escapefromtarkov.com)
* このプロジェクトは決して不正行為（チート）を為に設計されたことじゃありません。（このプロジェクトはチートがライブサーバーの経験を破壊したので作られました）
* このプロジェクトは決してゲームを無断にダウンロードする為のことではありません。（そして、無断ダウンロードの防止もあります！）
* これはただの教育をする目的です。（Unityとルバースエンジニアリングおよびネットワーキングを習う為に使用しています）
* 私はBSGと他の人（RedditやDiscordなど）から進行すると主張するプロジェクトと少しも関われていません。

## サポート

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/N4N2IQ7YJ)
* 注意してください。このKo-Fiのリンクは私にコーヒーを買ってくれる為に使います。その以外はなにもないです！
* 私が作成したコードはすべてここにあります。
* 助けや解決先を為にお金は払わないことにして下さい。
* これはただ趣味で、楽で作成するプロジェクトです。軽く受け入れて下さい。
* 私はコミュニティに騙すことはしません。これが半分失敗したこととしても私は精一杯で直していこうと思います。

## SPT-AKI 要件
* Stay in Tarkovは[AKIサーバー最新版](https://dev.sp-tarkov.com/SPT-AKI/Server)を使用して動作できます。SPT-Akiについては[こっち](https://www.sp-tarkov.com/)で詳しく調べます。
* このプロジェクトをSPT-AkiのClientにインストールしないようにご注意下さい！サーバーにインストールして下さい！

## [Wiki](https://github.com/paulov-t/SIT.Core/wiki)
**Wikiは様々な人によって作成されてますので壊れてしまう可能性もあります！そして全てのWikiソースはDirectoryに保管されます。**
  - ### [Setup Manuals](https://github.com/paulov-t/SIT.Core/wiki/Guides-English)
  - ### [FAQs](https://github.com/paulov-t/SIT.Core/wiki/FAQs-English)

## 協力プレイ

### Coop Summary
**下記の事項はご注意ください**
* 協力プレイ機能はまだ開発中です。
* ほとんどの機能の利用は可能ですが、問題点とバグが多くあり、完璧ではなりません。
* こっちの人達から全ての地域をテストしましたが、最もいい作動はFACTORYとTHE LABでした。CPUとネットワークからのリソースが消耗しやすいですがこっちらに影響を与えるのは他の人の繋がりとAIの数でした。
* ホストをする方法と協力プレイをする方法はこっちらから英語ですが詳しく読めます。 [HOSTING.md Document](https://github.com/paulov-t/SIT.Core/wiki/HOSTING-English.md)

### 要件
まず、進むためには [SPT-Aki mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) が要件となります。このモジュールをサーバに適用してから作動します。協力プレイのモジュールが必要がなければBepInExのコンフィグのファイルで直接修正する必要があります。

### 協力プレイはBSGのコードを使いますか？
いいえ、BSGサーバーのコードは明確な理由でクライアントから隠されてあります。
で、BSGの協力プレイサーバーの動作形はオンラインサーバーのPvPvEと同じようにできています。
これを見たり、使用したりは出来ません。

### コードの説明
- このプロジェクトは目的を達成するため、Unityのコンポーネントと結合された複数方法のBepInExのHarmonyパッチを使用します。
- クライアント＞サーバー＞クライアント（動きと回転と視線など）間に送信を引き続いて必要な要素を使用してデータを送信します。（AIのコードはUpdateとLateUpdateの命令および機能のチェックのたびに表現を実行されますのでネットワークにフラッド*が発生します。）
*フラッドはネットワークに大量のデータを運ぶ時に発生することです。DDoSによく使われます。
- 機能とメソッドは簡単に「複製」できるModuleReplicationPatchクラスを使用します。
- JSON TCP HttpとWeb Socketを通じた全てのサーバーの通信は「バックエンド」の作業を処理するために[SPT-Akiが開発した "Web Server"](https://dev.sp-tarkov.com/SPT-AKI/Server)でこの[typescript mod](https://github.com/paulov-t/SIT.Aki-Server-Mod)を使用しました。
- 協力プレイの準備が始まるとCoopGameComponentがゲーム内のワールドに追加し、PlayerReplicatedComponentにポーリングされます。

## SPT-Aki

### Akiモジュール互換性
次のAkiモジュールが互換できます。
- aki-core
- Aki.Common
- Aki.Reflection
- 半分のSPT-AKI基板のMOD。これはPatchがどのくらいよくできたことによってかわります。GCLASSXXXやPUBLIC/PRIVATEを直接ターゲットにしたらほぼ作動しません。

### なぜAkiのDLLモジュールを使いませんか？
SPT-AkiのDLLは作者によって独自の解読技術で作成され、私の技術で今はAkiモジュールによく動作しません。
だからSPT-Akiの多くの機能をこのモジュールに移植しました。私の最終の目標はSPT-Akiに依存し、これがSITの機能だけに集中にすることです。

## コンパイルする方法 
[Compiling Document](COMPILE.md)

# BepInExをインストールする方法
[https://docs.bepinex.dev/articles/user_guide/installation/index.html](https://docs.bepinex.dev/articles/user_guide/installation/index.html)

## タルコフにインストール
BepInEx 5は必ずインストールし、設定が終わったあとになるようにします。（BepInExをインストールする方法を見ましょう）
ビルドされた.dllファイルをBepInExのpluginsフォルダーに置きます。

## タルコフでのテスト
- まず、BepInExがインストールされたタルコフのフォルダーに進みます。
- configに進みます。
- BepInEx.cfgを開きます。
- [Logging.Console]の設定値をTrueにします。
- configファイルを保存します。
- タルコフをランチャーや次のようにbatファイルで開きます（tokenは自分のIDに変えてください）
```
start ./Clients/EmuTarkov/EscapeFromTarkov.exe -token=pmc062158106353313252 -config={"BackendUrl":"http://127.0.0.1:6969","Version":"live"}
```
- BepInExをコンソルと実行したらモジュールの「plugin」が表示されます。


## 感謝リスト
- SPT-Aki team
- MTGA team
- SPT-Aki Modding Community
- DrakiaXYZ ([BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain))
- Dvize ([NoBushESP](https://github.com/dvize/NoBushESP))

## ライセンス

- DrakiaXYZのプロジェクトはMITライセンスを含んでます。
- 95%の機能はSPT-Akiチームが完成しました。そっちのソースには関連されたライセンスがある可能性があるかもしれません。
- 私の作業はライセンスなどはありません。ただ楽しめるためのプロジェクトであります。あなたがこれで何をしても構わないです。
