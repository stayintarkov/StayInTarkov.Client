## 「プロファイルデータを読み込み中」画面が無限にされます

下記の問題点がなるかもしれません。
- 間違えたインストール
- サーバー連結の問題でIP設定を確認してください。
- Windowsのプロキシが連結を防げてます。


ポートフォワーディングの設定を正確にすることにします。外部IPに繋ぐための設定です。

シングルプレイヤーでする際にはExternal IP Finderを消し、127.0.0.1のままで設定を残してください。
---

## MODはどこでインストールしますか？

### ゲーム側のMOD
`<ゲームのフォルダ>/BepInEx/plugins/`にゲーム側のMODを置きます。

### サーバー側のMOD
`<サーバーのフォルダ>/user/mods/`にサーバー側のMODを置きます
---

## DDNS設定の段階です。固定パブリックIPアドレスがなくてドメインを使用してサーバーに連結する際

### １
下記のファイル２つの `"ip": "127.0.0.1"`部分をパソコンのNICでIPアドレス（__パブリックIPアドレスじゅありません！__）に変えます。および0.0.0.0を代わりに使います。 

`<サーバーのフォルダ>/Aki_Data/Server/configs/http.json`

`<サーバーのフォルダ>/Aki_Data/Server/database/server.json`

そして、このファイルのIPアドレスは同じくします。

### ２
SIT.Aki-Server-Modに進みます`<Server folder>/user/mods/SIT.Aki-Server-Mod/config/`にある`coopConfig.json`です。

"externalIP": "http://127.0.0.1:6969"をいま使おうとするドメイン名を入力します。例: "externalIP": "http://yourdomain.com:6969"

__useExternalIPFinderはfalseにします。__

__以上です。あとは楽しめることですね！__

---

### LinuxでSPT-Akiをご利用する方へ

[The English Version](../en/Guides/Run-Server-on-Linux-English.md)
