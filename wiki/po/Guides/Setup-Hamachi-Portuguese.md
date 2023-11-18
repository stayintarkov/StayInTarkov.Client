# Guia de instalação

## Este é um guia simples sobre como instalar o [SIT.Core](https://github.com/paulov-t/SIT.Core) e executá-lo com amigos usando [Hamachi](https://www.vpn.net/) (ou programa similar) e [SIT.Launcher](https://github.com/paulov-t/SIT.Launcher).

### LEIA ISSO ANTES DE COMEÇAR!
Você precisa comprar e ter uma instalação ativa do [Escape From Tarkov](https://www.escapefromtarkov.com/) para que isso funcione.

Você precisa instalar o [Hamachi](https://www.vpn.net/) ou algo similar. Neste guia, vou usar o Hamachi.

Este guia foi testado com o SIT.Core.Release-64 e SIT-Launcher.Release-71

Essa é a maneira como consegui fazer esse mod funcionar, pode haver etapas desnecessárias e este guia pode ficar desatualizado rapidamente à medida que o mod for atualizado!

Você também pode aplicar este guia a cenários em que o host tenha as portas encaminhadas corretamente, basta pular as partes do Hamachi e usar o IPv4 público do host.

# Guia de instalação do servidor
### Pule se seu amigo estiver hospedando o servidor.
1. Crie uma estrutura de pastas semelhante à descrita no guia de instalação do cliente.
Mas desta vez, crie também uma pasta chamada "Server"\
SITCOOP/Server
2. Baixe e extraia a versão mais recente do [SPT-AKI Stable Release](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases) na pasta Server.
3. Execute o Aki.Server.exe, você pode fechá-lo assim que disser "Happy playing".
4. Baixe o [SIT.Aki server mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) e instale-o como você instalaria qualquer outro mod no AKI Server.\
O caminho da pasta do mod deve ser assim:\
C:\SITCOOP\Server\user\mods\SIT.Aki-Server-Mod-master\
Para ter certeza de que não há pastas extras, o SIT.Aki-Server-Mod-master deve ter o arquivo package.json dentro dela.
5. Configure http.json e coopconfig.json conforme descrito em [Hospedagem](https://github.com/paulov-t/SIT.Core/wiki/Hosting-Portuguese) de Paulov-t.
Se você estiver usando o Hamachi, use o IPv4 do Hamachi para ambos http.json e coopconfig.json.\
*E não use 127.0.0.1 ou Localhost!*
6. Inicie o servidor (como administrador) e vá para a seção de instalação do cliente.

# Instalando o SIT.Launcher e o SIT.Core como cliente.

1. Navegue até a pasta desejada onde você deseja fazer a instalação, para mim decidi instalá-lo na pasta "C:\SITCOOP".
2. Crie as pastas "Game" e "Launcher" dentro da pasta "SITCOOP".
3. Baixe e extraia o SIT.Launcher para a pasta "Launcher".
4. Execute o SIT.Launcher.exe e configure o jogo para a pasta "Game" que criamos anteriormente.
5. Assim que a instalação estiver concluída, você precisa fechar o launcher.
6. Crie uma pasta chamada "AkiSupport" dentro da pasta "Launcher".
7. Crie pastas dentro de "AkiSupport" para que os caminhos fiquem assim:\
C:\SPCOOP\Launcher\AkiSupport\Bepinex\Patchers\
C:\SPCOOP\Launcher\AkiSupport\Bepinex\Plugins\
C:\SPCOOP\Launcher\AkiSupport\Managed\
6. Se seu amigo estiver hospedando o servidor, copie o IPv4 do Hamachi dele no cliente do Hamachi. Se você for o host, copie seu próprio IPv4 do Hamachi, **127.0.0.1 não funcionará!**
7. Inicie o launcher e altere o campo do servidor para o IP de seus amigos.\
Exemplo: http://100.100.100.100:6969
8. Insira o nome de usuário e senha desejados.

**O nome de usuário e senha são armazenados em texto simples no computador do host, não use qualquer nome de usuário/senha que você use em outro lugar ou que não deseje que o host veja!**

9. Vá para as configurações do launcher e verifique se você tem "Automatically Install SIT", "Force Install Latest SIT..." e "Automatically Install AKi Support" habilitados.

**Parabéns, agora você tem uma nova instalação do SIT.**

**Divirta-se**

Criado por ppyLEK *(pequenas edições por SlejmUr)*