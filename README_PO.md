<div align=center style="text-align: center">
<h1 style="text-align: center"> StayInTarkov.Client </h1>
Um módulo BepInEx de Escape From Tarkov projetado para ser usado com o servidor SPT-Aki com o objetivo final de "Coop Offline"
</div>

---

<div align=center>

![GitHub (todas as versões)](https://img.shields.io/github/downloads/stayintarkov/StayInTarkov.Client/total) ![GitHub lançamento (mais recente por data)](https://img.shields.io/github/downloads/stayintarkov/StayInTarkov.Client/latest/total)

[English](README.md) **|** [简体中文](README_CN.md) **|** [Deutsch](README_DE.md) **|** [Português-Brasil](README_PO.md) **|** [日本語](README_JA.md) **|** [한국어-Korean](README_KO.md) **|** [Français](README_FR.md)

</div>

---

## Estado do Stay in Tarkov

* Stay in Tarkov está em desenvolvimento ativo pelos desenvolvedores do time SIT
* Pull Requests e Contribuidores são sempre aceitos (se funcionarem!)

## Sobre

O projeto Stay in Tarkov nasceu devido à relutância da Battlestate Games (BSG) em criar uma versão puramente PvE do Escape from Tarkov.
O objetivo do projeto é simples: criar uma experiência de coop PvE que mantenha a progressão. Se a BSG decidir criar a capacidade de fazer isso na versão Live, este projeto será encerrado imediatamente.

## Aviso Legal

* Você deve comprar o jogo para usar isso. Você pode adquiri-lo aqui: [https://www.escapefromtarkov.com](https://www.escapefromtarkov.com).
* Este projeto não foi desenvolvido para ser usado como trapaça (ele foi criado porque as trapaças arruinaram a experiência ao vivo).
* Este projeto não foi desenvolvido para fazer o download ilegal do jogo (e possui bloqueios para pessoas que o fazem!).
* Este projeto é puramente para fins educacionais. Estou usando para aprender Unity e Redes TCP/UPD/Web Socket e eu aprendi bastante da BattleState Games \o/.
* Não tenho nenhuma afiliação com a BSG ou outros (no Reddit ou Discord) que afirmam estar trabalhando em um projeto similar. NÃO entre em contato com o Subreddit ou Discord do SPTarkov sobre este projeto.
* Este projeto não é afiliado ao SPTarkov (SPT-Aki), mas utiliza o seu excelente server.
* Este projeto não é afiliado a nenhum outro emulador de Tarkov.
* Este projeto vem "como está". Ou ele funciona pra você, ou não funciona.

## Suporte

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/N4N2IQ7YJ)
* O link do Ko-Fi compra um cafézinho pro Paulov!
* Pull Requests são bem vindos! Obrigado a todos os contribuidores!
* Por favor, não entregue dinheiro esperando ajuda ou uma solução.
* Isso é um hobby, para diversão, um projeto. Por favor, não o leve a sério.
* Não engano a comunidade. Eu sei que esta é uma tentativa semi-funcional, mas vou tentar corrigir da melhor forma possível.
* O Discord da comunidade SIT(https://discord.gg/f4CN4n3nP2) está disponivel. A comunidade se uniu para ajudar uns aos outros e criar servidores da comunidade.

## Requisitos do SPT-AKI
* Stay in Tarkov funciona com o [último AKI Server](https://dev.sp-tarkov.com/SPT-AKI/Server) para ser executado. Você pode saber mais sobre o SPT-Aki [aqui](https://www.sp-tarkov.com/).
* NÃO INSTALE ISSO NO CLIENTE DO SPT-Aki! INSTALE APENAS NO SERVIDOR!

## [Wiki](https://github.com/stayintarkov/StayInTarkov.Client/wiki)
**A Wiki está em construção por vários colaboradores. Ela pode estar incompleta! Todas as instruções também estão disponíveis no código-fonte, no diretório wiki.**
  - ### [Manuais de Configuração](https://github.com/stayintarkov/StayInTarkov.Client/wiki/Guides-Portuguese)
  - ### [Perguntas Frequentes](https://github.com/stayintarkov/StayInTarkov.Client/wiki/FAQs-Portuguese)

## Coop

### PRÉ-REQUISITO
Você deve ter o mod [SPT-Aki](https://github.com/stayintarkov/SIT.Aki-Server-Mod) instalado em seu servidor para que este módulo funcione. Se você não deseja usar o módulo Coop, você deve desativá-lo no arquivo de configuração do BepInEx.

### O Coop pode usar o código da BSG?
Não. O código do servidor da BSG é oculto do cliente por razões óbvias. Portanto, a implementação da BSG do coop usa os mesmos servidores online do PvPvE. Nós não temos acesso a isso, então não podemos usar isso.

## SPT-Aki

### Os módulos Aki são suportados?
Os seguintes módulos Aki são suportados.
- aki-core
- Aki.Common
- Aki.Reflection
- Os mods clientes do SPT-AKI funcionam? Isso depende do quão bem escrito os patches são. Se eles dependem diretamente do alvo GCLASSXXX ou PUBLIC/PRIVATE então provavelmente vão falhar.

### Por que você não usa os DLLs do Módulo Aki?
As DLLs do SPT-Aki são escritas especificamente para sua própria técnica de desofuscação, e a técnica do Paulov não está funcionando bem com os Módulos Aki neste momento.
Então, muitos recursos do SPT-Aki para este módulo foram portados. Meu objetivo final seria depender do SPT-Aki e focar exclusivamente em recursos exclusivos do SIT.

## Como compilar?
[Documento de Compilação](COMPILE.md)

## Lista de Agradecimentos
- Equipe SPT-Aki (Créditos disponibilizados em cada arquivo com código usado e muito amor ao time de Dev's pelo suporte deles)
- Comunidade de Modding do SPT-Aki
- DrakiaXYZ ([Big Brain](https://github.com/DrakiaXYZ/SPT-BigBrain) & [Waypoints](https://github.com/DrakiaXYZ/SPT-Waypoints) estão integrados neste projeto)
- Time SIT e os contribuidores originais

## Licença

- O projeto do DrakiaXYZ contém a licença do MIT.
- 99% do núcleo original e funcionalidade single-player foi desenvolvido pelas equipes do SPT-Aki. Pode haver licenças relacionadas a eles dentro deste código-fonte.
- Nenhum do meu próprio trabalho tem licença. Este é apenas um projeto para me divertir. Não me importo com o que você faz com ele.