## Tela de "Carregando dados do perfil..." infinita

Causado por:
- Uma instalação quebrada.
- Um problema com a conexão do servidor, relacionado às configurações do seu IP.
- Proxy do Windows barrando a conexão

Certifique-se de que o redirecionamento de porta esteja configurado corretamente para que você possa se conectar ao seu IP externo.

Se você estiver jogando no modo Single Player, não use nenhuma dessas opções e deixe como 127.0.0.1 e desative o Localizador de IP Externo.
---

## Onde instalo os mods?

### Mods do Cliente
Instale os mods do cliente em `<pasta do jogo>/BepInEx/plugins/`.

### Mods do Servidor
Instale os mods do servidor em `<pasta do servidor>/user/mods/`.
---

## Configuração DDNS. Se você não possui um endereço IP público estático e deseja usar um nome de domínio para se conectar ao servidor.

### Passo 1
Substitua `"ip": "127.0.0.1"` nesses dois arquivos pelo endereço IP da interface de rede do seu computador (NÃO O SEU ENDEREÇO IP PÚBLICO) 
ou use 0.0.0.0 em seu lugar.

`<pasta do servidor>/Aki_Data/Server/configs/http.json`

`<pasta do servidor>/Aki_Data/Server/database/server.json`

e verifique se esses dois arquivos têm o mesmo endereço IP.

### Passo 2
Localize o arquivo `coopConfig.json` do SIT.Aki-Server-Mod no diretório `<pasta do servidor>/user/mods/SIT.Aki-Server-Mod/config/`.

Altere "externalIP": "http://127.0.0.1:6969" para o seu nome de domínio. Por exemplo: "externalIP": "http://seudominio.com:6969".

__Defina useExternalIPFinder como false__

__Agora você está pronto para começar. Divirta-se!__

---

### Usando o Servidor SPT-Aki no Linux
[Versão em Português](../po/Guides/Run-Server-on-Linux-Portuguese.md)
