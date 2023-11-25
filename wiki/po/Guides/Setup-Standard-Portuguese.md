# Guia de instalação

* Este guia pressupõe que os usuários tenham acesso às configurações de encaminhamento de porta do roteador ISP e ao firewall e como mudá-las
* Esse guia pressupõe que voce tem uma versão Oficial do Escape From Tarkov instalada usando o Launcher da BattleState Games

## HOST

1. Baixe [SPT Aki](https://www.sp-tarkov.com/) e extraia Aki.Server e Aki_Data para qualquer pasta desejada
2. Siga as instruçoes para [Instalar o SIT Coop Mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) no servidor e configurar corretamente
3. Exclua todas as cópias existentes do EFT Offline.
4. Baixe a [ultima versão](https://github.com/stayintarkov/SIT.Launcher.Classic) do [SIT-Launcher](https://github.com/stayintarkov/SIT.Launcher.Classic), e extraia o arquivo .zip em qualquer lugar.
5. Abra o SIT.Launcher.exe
6. Siga as instruções do SIT-Launcher para criar uma copia do seu EFT Live e instalar a ultima versão do SIT automaticamente
7. Certifique-se de que o SIT-Launcher instale o SIT / Assemblies na aba de configurações
8. Certifique-se que você seguiu as instruções para [instalar o SIT Coop Mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) e colocou o seu http.json do Servidor Aki para seu IP Local da rede e o coopConfig.json do Coop Mod para seu [IP Externo](https://www.whatismyip.com/)
9. Abra o Servidor e compartilhe com seus amigos seu IP Externo
10. Abra as portas 6969, 6970 no seu roteador (exemplo: 192.1.2.3)
11. Crie as regras de Firewall para as portas 6969,6970 no seu servidor e roteador
12. Teste sua conexão colocando seu IP Externo no launcher e apertando em "Launch"!
12. Tudo Pronto!

## CLIENTE

1. Exclua todas as cópias existentes do EFT Offline.
2. Baixe a [ultima versão](https://github.com/stayintarkov/SIT.Launcher.Classic) do [SIT-Launcher](https://github.com/stayintarkov/SIT.Launcher.Classic), e extraia o arquivo .zip em qualquer lugar.
3. Abra o SIT.Launcher.exe
4. Siga as instruções do SIT-Launcher para criar uma copia do seu EFT Live e instalar a ultima versão do SIT automaticamente
5. Certifique-se de que o SIT-Launcher instale o SIT / Assemblies na aba de configurações
6. Conecte ao IP e Porta enviadas pelo HOST (Exemplo: http://111.222.255.255:6969)
7. NÃO USE NOMES DE USUÁRIO E SENHAS DE OUTRAS FONTES, TODAS AS SENHAS SÃO SALVAS EM TEXTO PLANO!