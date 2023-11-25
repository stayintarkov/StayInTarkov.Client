# Guia passo a passo de instalação do Stay In Tarkov

# Pré-requisitos

Antes de começar, verifique se a versão mais recente do Escape From Tarkov foi baixada e instalada usando o Battlestate Games Launcher. O Stay In Tarkov não funcionará com uma cópia desatualizada ou ilegítima do jogo.

Ao longo do guia, faremos referência a `SIT_DIR` como o diretório raiz para instalar o Stay In Tarkov. Neste diretório, criaremos três pastas separadas para manter as coisas organizadas:

- Uma pasta `server` para o servidor SPT-AKI.
- Uma pasta `launcher` para o SIT Launcher.
- Uma pasta `game` para os arquivos do jogo Escape From Tarkov.

*Considere usar uma ferramenta como o [7zip](https://7-zip.org/) ou o WinRAR para descompactar arquivos compactados.*

# Instalação

## 1. [SIT Launcher](https://github.com/stayintarkov/SIT.Launcher.Classic/releases) (usando instalação automática)

1. Baixe a versão mais recente do `SIT Launcher` na página de [Releases](https://github.com/stayintarkov/SIT.Launcher.Classic/releases).
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

## 3. [SIT Server Mod](https://github.com/stayintarkov/SIT.Aki-Server-Mod)
1. Baixe o arquivo zip do mod do servidor no [GitHub](https://github.com/stayintarkov/SIT.Aki-Server-Mod) (procure por ele sob o grande botão verde: *Code > Download Zip*).
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

## Hospedado com Redirecionamento de porta

### Configuração

Redirecionamento de porta permite você usar seu computador local como servidor como um serviço de face publica. Em resumo, seu roteador tem um IP _externo_ estático (que não muda) que pode ser visto em https://www.whatismyip.com. Esse endereço de IP que o mundo vê como 'você' independente de quantos aparelhos tenha na rede (se você for no whatismyip em qualquer dispositivo na rede, verá que o IP é o mesmo). Para usar seu computador para trafego externo e permitir seus amigos conectar no servidor aberto em seu computador, primeiro vai ter que fazer algumas coisas:
1. **Achar seu endereço MAC**: ache o endereço MAC da sua maquina, no qual você precisa para identificar seu computador para o passo 3. Você pode achar isso indo no CMD e digitando `ipconfig /all` e procurar pela linha que diz "Endereço Fisico" abaixo do adaptador de rede (Exemplo: Adaptador Ethernet). Ele vai ser algo tipo `00-00-00-00-00-00`.
2. **Abra as configurações do seu Roteador**: vá na página da web do seu roteador (geralmente acessivel em http://192.168.1.1, mas nem todo roteador é este. Confira no seu roteador ou procure o manual do modelo)
3. **Atribua um IP estático ao seu computador**: atribua um endereço IP local estático à sua máquina (o dispositivo com seu endereço MAC). Existem muitas opções disponíveis, sendo uma das convenções algo dentro do intervalo de 192.168.0.0 a 192.168.255.255 (certifique-se de que é diferente do IP do seu roteador).
4. **Configure o redirecionamento de porta**: nas configurações do seu roteador, encontre o Redirecionamento de Porta e encaminhe as portas `6969` e `6970` (que geralmente podem ser escritas como `6969,6970`) para o endereço local estático que você acabou de atribuir ao seu computador. Este passo garantirá que qualquer tráfego que chegue por essas portas seja direcionado para o seu computador (por padrão, essas portas são bloqueadas pelo seu roteador).
5. **Configure seu firewall**: abra (ou permita) as portas de tráfego TCP de entrada `6969` e `6970` nas configurações do firewall do Windows (ou qualquer firewall que você esteja usando). Este passo garantirá que seu computador aceite tráfego nessas portas.
6. **Proteja suas portas**: este passo não é obrigatório, mas recomendado por motivos de segurança: inclua os endereços IP de seus amigos (encontrados usando o whatismyip) nas configurações do seu roteador. Dependendo do seu roteador, isso pode precisar ser feito na mesma tela do passo 4 ou em uma tela separada; por exemplo, em roteadores ASUS, você deve definir o `IP de origem` como o endereço do seu amigo na tela de redirecionamento de porta. Você precisará fazer isso para cada amigo com quem deseja jogar. Se um amigo não conseguir se conectar no futuro, você pode ter esquecido de fazer este passo para o IP deles. Este passo garantirá que apenas seus amigos possam se conectar ao seu servidor, e não qualquer pessoa na Internet. Se você não fizer este passo, qualquer pessoa na Internet poderá se conectar ao seu servidor, o que não é recomendado. Nota: você precisará incluir o endereço IP interno do seu próprio computador também, ou você não conseguirá se conectar ao seu próprio servidor!
7. **Atualize a configuração HTTP**: vá para `SIT_DIR\server\Aki_Data\Server\configs` e abra `http.json` em um editor de texto. Altere o valor `ip` para o IP local estático que você atribuiu à sua máquina. Este passo permitirá que você se conecte ao seu servidor a partir do seu próprio computador.
8. **Atualize a configuração de co-op**: vá para `SIT_DIR\server\user\mods\SIT.Aki-Server-Mod-master\config` e abra `coopConfig.json` em um editor de texto. Altere o valor `externalIP` para o IP fornecido pelo whatismyip. Este passo permitirá que seus amigos se conectem ao seu servidor.

Agora seu servidor está configurado! Para conectar no seu próprio servidor, abra o SIT Launcher e coloque seu endereço de IP estatico, como `http://{ seu endereço de ip local }:6969`. Seus amigos vai conectar usando o IP dado pelo whatismyip, como `http://{ seu endereço de ip externo }:6969`.
Opcinal: coloque `logrequests` para `false` em `SIT_DIR/server/Aki_Data/Server/configs/http.json` para previnir spam de log.

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

Veja o HOSTING.md de sua lingua para aprender a criar um lobby.
Guias de HOSPEDAGEM podem ser encontrados aqui: https://github.com/stayintarkov/StayInTarkov.Client/tree/master/wiki

## Notas Adicionais

1. Seus amigos não precisam configurar o servidor em nenhum momento. Eles só precisam instalar o SIT usando o iniciador e se conectar ao seu servidor.
2. Recomenda-se que todos os jogadores, tanto você quanto seus amigos, usem a mesma versão do SIT. Isso pode ser feito marcando a opção "Forçar instalação do último SIT" no menu de configurações do iniciador.
3. Se você deixar o servidor rodando o tempo todo (o que requer desativar o modo de suspensão em sua máquina), seus amigos poderão se conectar a qualquer momento. Eles podem editar seus equipamentos, usar o Flea Market, entrar em seus esconderijos, etc., e até mesmo jogar raids solo sem que você esteja presente. Note que se duas raids estiverem acontecendo ao mesmo tempo, ambas terminarão quando uma delas acabar.
4. Você pode encontrar opções de configuração adicionais em `SIT_DIR\game\BepInEx\config\SIT.Core.cfg`, onde você pode, por exemplo, desativar o feed de spawn/morte no canto inferior direito da tela durante a raid. Como essas são opções do cliente (ao contrário das opções do servidor), é altamente recomendável que todos os jogadores usem as mesmas opções de configuração.