# Guia passo a passo de instalação do Stay In Tarkov

# Pré-requisitos

Antes de começar, verifique se a versão mais recente do Escape From Tarkov foi baixada e instalada usando o Battlestate Games Launcher. O Stay In Tarkov não funcionará com uma cópia desatualizada ou ilegítima do jogo.

Ao longo do guia, faremos referência a `SIT_DIR` como o diretório raiz para instalar o Stay In Tarkov. Neste diretório, criaremos três pastas separadas para manter as coisas organizadas:

- Uma pasta `server` para o servidor SPT-AKI.
- Uma pasta `launcher` para o SIT Launcher.
- Uma pasta `game` para os arquivos do jogo Escape From Tarkov.

*Considere usar uma ferramenta como o [7zip](https://7-zip.org/) ou o WinRAR para descompactar arquivos compactados.*

# Instalação

## 1. [SIT Launcher](https://github.com/paulov-t/SIT.Launcher/releases) (usando instalação automática)

1. Baixe a versão mais recente do `SIT Launcher` na página de [Releases](https://github.com/paulov-t/SIT.Launcher/releases).
2. Descompacte o arquivo e extraia o conteúdo para `SIT_DIR/launcher`.
3. Execute `SIT.Launcher.exe`.
4. Na primeira vez em que você executar o launcher, ele solicitará uma instalação: 

    *“Nenhuma instalação OFFLINE encontrada. Deseja instalar agora?”*

    Clique em "Sim".

5. Selecione `SIT_DIR/game` como diretório de instalação.
6. Deixe o launcher copiar seus arquivos do jogo, isso pode levar alguns minutos.

## 2. [SPT-AKI Server](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases)

1. Baixe a versão mais recente do `SPT-AKI Server` na página de [Releases](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases).
2. Descompacte o arquivo e extraia o conteúdo para `SIT_DIR/server`.

## 3. [SIT Server Mod](https://github.com/paulov-t/SIT.Aki-Server-Mod)
1. Baixe o arquivo zip do mod do servidor no [GitHub](https://github.com/paulov-t/SIT.Aki-Server-Mod) (procure por ele sob o grande botão verde: *Code > Download Zip*).
2. Descompacte o arquivo e extraia o conteúdo para `SIT_DIR/server/user/mods`.

    *O diretório `user/mods` é criado automaticamente quando o servidor é executado pela primeira vez. Execute `Aki.Server.exe` para criar a pasta. Pare e feche o servidor assim que o diretório for criado para continuar o processo de instalação.*

# Configurando o servidor

## Hospedado no localhost (para teste)

### Servidor
1. Abra o arquivo de configuração do servidor coop em `SIT_DIR/server/user/mods/SIT.Aki Server-Mod/config/coopConfig.json`.

    *O arquivo `coopConfig.json` é criado automaticamente quando o mod do servidor é executado pela primeira vez. Execute `Aki.Server.exe` para criar o arquivo. Pare e feche o servidor assim que o arquivo for criado para continuar o processo de instalação.*

    *Observação: Faça as edições no arquivo usando o Bloco de Notas ou um editor de texto que não introduza formatação. Não use o Microsoft Word.*
2. Defina `externalIP` como `http://127.0.0.1:6969`.
3. Defina `useExternalIPFinder` como `false`.
4. Opcionalmente, defina `logRequests` como `false` em `SIT_DIR/server/Aki_Data/Server/configs/http.json` para evitar registros excessivos.

### Launcher
Conecte-se usando `http://127.0.0.1:6969` como servidor.

*Você não poderá convidar outras pessoas para se juntarem ao seu jogo usando localhost, mas pode ser útil para depurar problemas de conexão. Use isso para confirmar se o jogo e os mods estão instalados corretamente.*

## Hospedado com encaminhamento de porta

### Servidor
Seu endereço IP externo deve ser detectado automaticamente, não sendo necessária nenhuma outra configuração.
Verifique os logs do servidor em busca de `COOP: Auto-External-IP-Finder` com o seu endereço IP.

Opcionalmente, defina `logRequests` como `false` em `SIT_DIR/server/Aki_Data/Server/configs/http.json` para evitar registros excessivos.

### Launcher
Use o IP mostrado no log `COOP: Auto-External-IP-Finder` do servidor, ou use o IP encontrado em https://www.whatismyip.com para se conectar (eles devem ser iguais).

## Hospedado com a VPN Hamachi

### Servidor
1. Execute o Hamachi.
2. Encontre o endereço IPv4 mostrado no widget do LogMeIn Hamachi e copie-o. Usaremos `100.10.1.10` como exemplo de IP neste guia.
3. Abra o arquivo de configuração do servidor coop em `SIT_DIR/server/user/mods/SIT.Aki Server-Mod/config/coopConfig.json`.

    *O arquivo `coopConfig.json` é criado automaticamente quando o mod do servidor é executado pela primeira vez. Execute `Aki.Server.exe` para criar o arquivo. Pare e feche o servidor assim que o arquivo for criado para continuar o processo de instalação.*

    *Observação: Faça as edições no arquivo usando o Bloco de Notas ou um editor de texto que não introduza formatação. Não use o Microsoft Word.*
4. Defina `externalIP` como o IP copiado do LogMeIn Hamachi: `http://100.10.1.10:6969`.
5. Defina `useExternalIPFinder` como `false`.
6. Abra o arquivo de conexão do servidor do SPT-AKI em `SIT_DIR/server/Aki_Data/Server/configs/http.json`.
    
    *Observação: Faça as edições no arquivo usando o Bloco de Notas ou um editor de texto que não introduza formatação. Não use o Microsoft Word.*
7. Defina `ip` como `100.10.1.10`.
8. Opcionalmente, defina `logRequests` como `false` para evitar registros excessivos.

### Launcher
Conecte-se usando o endereço IPv4 mostrado no widget do LogMeIn Hamachi. Em nosso exemplo, usaremos `http://100.10.1.10:6969` como servidor.

# Iniciando um jogo

## 1. Inicie o servidor

Execute `Aki.Server.exe`

## 2. Inicie o jogo

Inicie o jogo através do `SIT Launcher`.

*Na primeira vez em que você tentar conectar com novas credenciais, será solicitado que você crie a conta, clique em "Sim" (as senhas são armazenadas em texto simples, não reutilize senhas). Você também pode receber a solicitação de Alt+F4 após o lançamento do jogo; se isso acontecer, feche o jogo e reinicie-o através do SIT Launcher.*

## 3. Crie um lobby

Consulte [Como se juntar ao jogo de outras pessoas](https://github.com/paulov-t/SIT.Core/wiki/HOSTING.md#how-to-join-each-others-match) para obter instruções no jogo.
