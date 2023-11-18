<div align=center style="text-align: center">
<h1 style="text-align: center"> SIT.Core </h1>
Um módulo BepInEx de Escape From Tarkov projetado para ser usado com o servidor SPT-Aki com o objetivo final de "Coop Offline"
</div>

---

<div align=center>

![GitHub (todas as versões)](https://img.shields.io/github/downloads/paulov-t/SIT.Core/total) ![GitHub lançamento (mais recente por data)](https://img.shields.io/github/downloads/paulov-t/SIT.Core/latest/total)

[English](README.md) **|** [简体中文](README_CN.md) **|** [Português-Brasil](README_PT_BR.md)

</div>

---

## Sobre

O projeto Stay in Tarkov nasceu devido à relutância da Battlestate Games (BSG) em criar uma versão puramente PvE do Escape from Tarkov.
O objetivo do projeto é simples: criar uma experiência de coop PvE que mantenha a progressão. Se a BSG decidir criar a capacidade de fazer isso na versão Live, este projeto será encerrado imediatamente.

## Aviso Legal

* Você deve comprar o jogo para usar isso. Você pode adquiri-lo aqui: [https://www.escapefromtarkov.com](https://www.escapefromtarkov.com).
* Este projeto não foi desenvolvido para ser usado como trapaça (ele foi criado porque as trapaças arruinaram a experiência ao vivo).
* Este projeto não foi desenvolvido para fazer o download ilegal do jogo (e possui bloqueios para pessoas que o fazem!).
* Este projeto é puramente para fins educacionais (estou usando isso para aprender Unity, Engenharia Reversa e Networking).
* Não tenho nenhuma afiliação com a BSG ou outros (no Reddit ou Discord) que afirmam estar trabalhando em um projeto similar.

## Suporte

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/N4N2IQ7YJ)
* Por favor, esteja ciente. O link do Ko-Fi é para me comprar um café, nada mais!
* Eu não tenho um conjunto especial de código que faz funcionar além do que está aqui no GitHub.
* Por favor, não entregue dinheiro esperando ajuda ou uma solução.
* Isso é um hobby, para diversão, um projeto. Por favor, não o leve a sério.
* Não engano a comunidade. Eu sei que esta é uma tentativa semi-funcional, mas vou tentar corrigir da melhor forma possível.
* Pull Requests são encorajados!

## Requisitos do SPT-AKI
* Stay in Tarkov funciona com o [último AKI Server](https://dev.sp-tarkov.com/SPT-AKI/Server) para ser executado. Você pode saber mais sobre o SPT-Aki [aqui](https://www.sp-tarkov.com/).
* NÃO INSTALE ISSO NO CLIENTE DO SPT-Aki! INSTALE APENAS NO SERVIDOR!

## [Wiki](https://github.com/paulov-t/SIT.Core/wiki)
**A Wiki está em construção por vários colaboradores. Ela pode estar incompleta! Todas as instruções também estão disponíveis no código-fonte, no diretório wiki.**
  - ### [Manuais de Configuração](https://github.com/paulov-t/SIT.Core/wiki/Guides-Portuguese)
  - ### [Perguntas Frequentes](https://github.com/paulov-t/SIT.Core/wiki/FAQs-Portuguese)

## Coop

### Resumo do Coop
**ATENÇÃO**
* O coop está em estágios iniciais de desenvolvimento.
* A maioria dos recursos funciona (mais ou menos) e é "jogável (mais ou menos) com prováveis bugs". "Jogável" e perfeito são duas coisas muito diferentes. Espere lag (dessincronia), problemas e bugs.
* Meus testes incluíram todos os mapas. Os mapas que funcionam melhor são Factory e Labs. O desempenho depende muito da CPU/Internet do servidor e dos clientes e do número de IAs no servidor.
* Mais informações sobre HOSPEDAGEM & COOP estão no [Documento HOSTING.md](https://github.com/paulov-t/SIT.Core/wiki/en/Guides/HOSTING-Portuguese.md)

### PRÉ-REQUISITO
Você deve ter o mod [SPT-Aki](https://github.com/paulov-t/SIT.Aki-Server-Mod) instalado em seu servidor para que este módulo funcione. Se você não deseja usar o módulo Coop, você deve desativá-lo no arquivo de configuração do BepInEx.

### O Coop pode usar o código da BSG?
Não. O código do servidor da BSG é oculto do cliente por razões óbvias. Portanto, a implementação da BSG do coop usa os mesmos servidores online do PvPvE. Nós não temos acesso a isso, então não podemos usar isso.

### Explicação de Codificação
- O projeto usa vários métodos de patches Harmony do BepInEx em conjunto com Componentes Unity para alcançar seus objetivos.
- Recursos/Métodos que requerem sondagem constante entre Cliente->Servidor->Cliente (Mover, Girar, Olhar, etc) usam Componentes para enviar dados (o código da IA executa os comandos Update/LateUpdate e a função a cada tick, causando assim inundação de rede).
- Recursos/Métodos que podem ser facilmente "replicados" usam a classe abstrata ModuleReplicationPatch para fazer uma chamada de ida e volta facilmente.
- Toda a comunicação do servidor é feita por meio de chamadas JSON TCP Http e Web Socket para o ["Servidor Web" desenvolvido por SPT-Aki](https://dev.sp-tarkov.com/SPT-AKI/Server), usando um [mod typescript](https://github.com/paulov-t/SIT.Aki-Server-Mod) para lidar com o trabalho "em segundo plano".
- CoopGameComponent é anexado ao objeto GameWorld quando um jogo pronto para Coop é iniciado (qualquer jogo que não seja Hideout). CoopGameComponent verifica o servidor em busca de informações e passa os dados para o PlayerReplicatedComponent.

## SPT-Aki

### Os módulos Aki são suportados?
Os seguintes módulos Aki são suportados.
- aki-core
- Aki.Common
- Aki.Reflection
- 50/50 nos mods do SPT-AKI Client. Isso depende de como os patches foram escritos. Se eles direcionam diretamente GCLASSXXX ou PUBLIC/PRIVATE, provavelmente falharão.

### Por que você não usa os DLLs do Módulo Aki?
As DLLs do SPT-Aki são escritas especificamente para sua própria técnica de desofuscação, e minha própria técnica não está funcionando bem com os Módulos Aki neste momento.
Então, eu portei muitos recursos do SPT-Aki para este módulo. Meu objetivo final seria depender do SPT-Aki e focar exclusivamente em recursos exclusivos do SIT.

## Como compilar?
[Documento de Compilação](COMPILE.md)

# Como instalar o BepInEx
[https://docs.bepinex.dev/articles/user_guide/installation/index.html](https://docs.bepinex.dev/articles/user_guide/installation/index.html)

## Instalar no Tarkov
O BepInEx 5 deve ser instalado e configurado primeiro (consulte Como instalar o BepInEx)
Coloque o arquivo .dll compilado na pasta de plugins do BepInEx

## Testar no Tarkov
- Navegue até onde o BepInEx está instalado dentro da sua pasta do Tarkov
- Abra a pasta config
- Abra o arquivo BepInEx.cfg
- Altere a configuração a seguir [Logging.Console] Enabled para True
- Salve o arquivo de configuração
- Execute o Tarkov por meio de um lançador ou arquivo .bat como este (substituindo o token pelo seu ID)
```
start ./Clients/EmuTarkov/EscapeFromTarkov.exe -token=pmc062158106353313252 -config={"BackendUrl":"http://127.0.0.1:6969","Version":"live"}
```
- Se o BepInEx estiver funcionando, um console deverá abrir e exibir o módulo "plugin" como iniciado

## Lista de Agradecimentos
- Equipe SPT-Aki
- Equipe MTGA
- Comunidade de Modding do SPT-Aki
- Props (AIBushPatch, AIAwakeOrSleepPatch - Atualmente não utilizados)

## Licença

- 95% do núcleo original e funcionalidade single-player foi desenvolvido pelas equipes do SPT-Aki. Pode haver licenças relacionadas a eles dentro deste código-fonte.
- Nenhum do meu próprio trabalho tem licença. Este é apenas um projeto para me divertir. Não me importo com o que você faz com ele.
