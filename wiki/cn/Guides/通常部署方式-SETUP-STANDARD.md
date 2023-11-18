# 安装教程

** 这个教程仅在用户可以配置运营商的光猫/路由器防火墙端口转发并且拥有公网IP的情况下适用 **

** 如果用户网络无公网IP但是NAT类型为Fullcone可以尝试使用NAT打洞软件进行端口转发 **

** 部署完成后可参考[如何加入其他人的对战](https://github.com/paulov-t/SIT.Core/wiki/%E5%BC%80%E5%A7%8B%E4%B8%80%E5%9C%BA%E6%B8%B8%E6%88%8F-HOSTING#%E5%A6%82%E4%BD%95%E5%8A%A0%E5%85%A5%E5%85%B6%E4%BB%96%E4%BA%BA%E7%9A%84%E5%AF%B9%E6%88%98)开始游戏 **

## 主机/服务器

1. 下载并解压最新的SPT-AKI服务端，请前往DEV-SPTARKOV下载，点击[这里](dev.sp-tarkov.com)跳转到官方Relese（现有的最新版本是3.5.7）
2. 安装[SIT.Aki-Server-Mod](https://github.com/paulov-t/SIT.Aki-Server-Mod)到服务端
4. 删除你已有的所有的离线塔科夫文件
5. 下载最新的 [SIT-Launcher](https://github.com/paulov-t/SIT.Launcher/releases)
6. 使用 SIT-Launcher 从你的在线版本塔科夫创建一个副本 (或者自行复制一份出来，取决于你自己)
7. 使用 SIT-Launcher 安装 SIT.COre 与 Assemblies文件
8. 配置服务端的 http.json 与 server.json为你的网卡地址，coopConfig.json为你的公网IP地址，如果想要使用域名链接参照FAQ中的[DDNS配置](https://github.com/paulov-t/SIT.Core/wiki/%E7%96%91%E9%9A%BE%E8%A7%A3%E7%AD%94-FAQs#%E6%AD%A4%E6%AD%A5%E9%AA%A4%E4%B8%BAddns%E9%85%8D%E7%BD%AE%E5%A6%82%E6%9E%9C%E4%BD%A0%E6%B2%A1%E6%9C%89%E9%9D%99%E6%80%81%E5%85%AC%E7%BD%91ip%E5%B9%B6%E4%B8%94%E6%83%B3%E7%94%A8%E5%9F%9F%E5%90%8D%E8%BF%9E%E6%8E%A5%E8%87%B3%E6%9C%8D%E5%8A%A1%E5%99%A8)
9. 启动服务器 (不要修改SPT-Aki服务端的任何其他地方)
10. 在你的光猫/路由器上转发配置的SPT-Aki服务器端口与SIT.Aki-Server-Mod端口，默认为6969与6970，并确认服务端的防火墙已经放行配置的端口
12. 和你的小伙伴找点乐子吧

## 客户端

1. 删除你已有的所有的离线塔科夫文件
2. 下载最新的 [SIT-Launcher](https://github.com/paulov-t/SIT.Launcher/releases)
3. 让 SIT-Launcher 从你的在线版本塔科夫创建一个副本 (或者自行复制一份出来，取决于你自己)
4. 让 SIT-Launcher 安装 SIT.Core 与 Assemblies文件
5. 在SIT-Launcher中填写服务器的地址并登录/注册账号
6. __不要使用和其他任何服务相同的用户名与密码！！__，因为服务端与客户端之间为明文传输，用户名与密码也是明文存储在服务端的！！

