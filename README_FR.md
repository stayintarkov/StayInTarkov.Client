
<div align=center style="text-align: center">
<h1 style="text-align: center"> SIT.Core </h1>
Un module BepInEx Escape From Tarkov conçu pour être utilisé avec le serveur SPT-Aki dans le but ultime d'une Coop "hors ligne". 
</div>

---

<div align=center>

![GitHub all releases](https://img.shields.io/github/downloads/paulov-t/SIT.Core/total) ![GitHub release (latest by date)](https://img.shields.io/github/downloads/paulov-t/SIT.Core/latest/total)

[English-Anglais](README.md) **|** [简体中文-Chinois Simplifié](README_CN.md) **|** [Deutsch-Allemand](README_DE.md) **|** [Portugais-Brésilien](README_PO.md) **|** [日本語-Japonais](README_JA.md) **|** [한국어-Coréen](README_KO.md) **|** [Français](README_FR.md)
</div>

---

## État de Stay In Tarkov

* SPT-Aki 3.7.1 est disponible sur leur site internet
* Stay In Tarkov has entered a state of limbo development
* Il y a des bugs que je ne peux pas résoudre ou qui nécessitent des réécritures significatives du code BSG et BSG change son code avec presque chaque patch.
* Je ne joue plus hors ligne car ce projet a été fait pour mon groupe Tarkov mais ils ont décidé de ne plus y jouer ou de ne pas jouer au Live (ils se sont lassés de Tarkov en général).
* Je vais probablement continuer à mettre à jour ce projet à chaque mise à jour de BSG et qui sait, nous pourrons peut-être obtenir un support pour Arena, sa serait sympa !
* Les Pull Requests et les contributions seront toujours acceptées (si elles fonctionnent !).

--- 

## à propos

Le projet Stay in Tarkov est né de la réticence de Battlestate Games (BSG) à créer une version purement PvE de Escape from Tarkov. 
L'objectif du projet est simple : créer une expérience de coopération PvE qui conserve la progression. 
Si BSG décide de créer la possibilité de faire cela en direct OU si je reçois une demande de la DCMA, ce projet sera arrêté immédiatement.

## Disclaimer

* Vous devez acheter le jeu pour l'utiliser. Vous pouvez l'obtenir ici. [https://www.escapefromtarkov.com](https://www.escapefromtarkov.com). 
* Ce projet n'est en aucun cas conçu pour les cheaters (il a été créé parce que les cheaters ont détruit l'expérience Live).
* Ce n'est en aucun cas conçu pour télécharger illégalement le jeu (Le projet comporte des blocages pour le piratage !).
* Il s'agit d'un projet purement éducatif. Je l'utilise pour apprendre Unity et les réseaux TCP/UDP/Web Socket et j'ai beaucoup appris de BattleState Games \o/.
* Je ne suis pas affilié à BSG ou à d'autres personnes (sur Reddit ou Discord) qui prétendent travailler sur un projet. Ne contactez PAS le subreddit SPTarkov ou le Discord à propos de ce projet.
* Ce projet n'est pas affilié à SPTarkov mais utilise son excellent serveur.
* Je n'offre pas de soutien. Ce projet est livré "tel quel". Il peut fonctionner pour vous ou pas.
## Support

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/N4N2IQ7YJ)
* **Je n'offre pas de soutien. Tous les tutoriels écrits par moi ou par d'autres contributeurs sont de bonne foi. Si vous n'arrivez pas à le faire fonctionner, je vous suggère de revenir sur la version Live !**
* Le lien Ko-Fi me permet de m'offrir (ou d'offrir à ma femme) un café, rien d'autre !
* Les Pull Requests sont encouragés. Merci à tous les contributeurs !
* Ne donnez pas d'argent en espérant obtenir de l'aide ou une solution.
* Il s'agit d'un hobby, pour le plaisir. Ne le prenez pas au sérieux.
* Je sais qu'il s'agit là d'une tentative à moitié ratée, mais je vais essayer de la réparer du mieux que je peux.
* Un [Discord SIT non officiel](https://discord.gg/VengzHxNmZ) est disponible. La communauté s'est associée pour s'entraider et créer des serveurs communautaires. **Je ne fais pas partie de ce Discord**.

## Configuration requise pour SPT-AKI
* Stay in Tarkov nécessite le [dernier serveur AKI](https://dev.sp-tarkov.com/SPT-AKI/Server) pour fonctionner. Vous pouvez en savoir plus sur SPT-Aki [ici](https://www.sp-tarkov.com/).
* N'INSTALLEZ PAS CELA SUR LE CLIENT SPT-AKI ! INSTALLEZ LE UNIQUEMENT SUR LE SERVEUR !

## [Wiki](https://github.com/paulov-t/SIT.Core/wiki)
**Le Wiki a été construit par différents contributeurs. Toutes les instructions sont également conservées dans les sources, dans le répertoire wiki.**
  - ### [Setup Manuel](https://github.com/paulov-t/SIT.Core/wiki/Guides-English)
  - ### [FAQs](https://github.com/paulov-t/SIT.Core/wiki/FAQs-English)

## Coop

### Résumé de la coop
**ATTENTION**
* la Coop en est aux premiers stades de développement. 
* La plupart des fonctionnalités fonctionnent (à peu près) et le jeu est "jouable (à peu près) avec des bugs probables". "Jouable" et parfait sont deux choses très différentes. Attendez-vous à des lag (désynchronisation), et à des bugs.
* L'hôte et le serveur doivent disposer d'une bonne connexion stable avec une vitesse d'envoi d'au moins 5-10mbps. L'IA nécessite beaucoup de CPU et de bande passante pour fonctionner.
* Même si beaucoup de gens disent le contraire. Vous pouvez jouer avec des gens du monde entier (pas seulement en LAN). J'ai joué avec des gens qui avaient plus de 200 ping. Ils ont un lag similaire à celui du live, juste montré d'une manière différente.
* Malgré les affirmations selon lesquelles les "VPN" tels que HAMACHI/RADMIN fonctionnent. Je vous recommande vivement de ne pas les utiliser. Ils ont des connexions très lentes. Essayez toujours de trouver un moyen d'héberger direct OU de payer un serveur cheap pour héberger le serveur Aki.

### PRÉREQUIS
Vous devez avoir le [mod SPT-Aki ](https://github.com/paulov-t/SIT.Aki-Server-Mod) installé dans votre serveur pour que ce module fonctionne. Si vous ne souhaitez pas utiliser le module Coop, vous devez le désactiver dans le fichier de configuration de BepInEx.

### La Coop peut-il utiliser le code Coop de BSG ?
Le code du serveur BSG est caché du client pour des raisons évidentes. Donc l'implémentation de la Coop de BSG utilise les mêmes serveurs en ligne que le PvPvE. Nous ne pouvons pas voir le code, donc nous ne pouvons pas l'utiliser.

### Explication du code
- Le projet utilise plusieurs méthodes de patchs BepInEx Harmony couplés à des composants Unity pour atteindre ses objectifs.
- Les fonctions/méthodes qui nécessitent une interrogation constante entre Client->Serveur->Client (Déplacer, Tourner, Regarder, etc.) utilisent des composants pour envoyer des données (le code AI exécute la commande Update/LateUpdate et la fonction à chaque tick, ce qui provoque une inondation du réseau).
- Les fonctionnalités/méthodes qui peuvent être facilement "répliquées" utilisent la classe abstraite ModuleReplicationPatch pour contourner facilement l'appel.
- Toutes les communications du serveur se font via des appels JSON TCP Http et Web Socket au ["Web Serveur " développé par SPT-Aki](https://dev.sp-tarkov.com/SPT-AKI/Server) en utilisant un [mod typecript](https://github.com/paulov-t/SIT.Aki-Server-Mod) pour gérer le travail de "backend".
- Le CoopGameComponent est attaché à l'objet GameWorld lorsqu'un jeu prêt pour la coopération est lancé (tout jeu qui n'est pas Hideout). CoopGameComponent interroge le serveur pour obtenir des informations et transmet les données au PlayerReplicatedComponent.

## SPT-Aki

### Les modules Aki BepInEx (modules clients) sont-ils pris en charge ?
Les modules Aki suivants sont supportés.
- aki-core
- Aki.Common
- Aki.Reflection
- Les mods SPT-AKI Client fonctionnent-ils ? Cela dépend de la qualité de l'écriture des correctifs. S'ils ciblent directement GCLASSXXX ou PUBLIC/PRIVATE, ils échoueront la plupart du temps.

### Pourquoi n'utilisez-vous pas les DLL du module Aki ?
Les DLL SPT-Aki sont écrites spécifiquement pour leur propre technique de désobfuscation et ma propre technique ne fonctionne pas bien avec les modules Aki pour le moment.
J'ai donc porté de nombreuses fonctionnalités de SPT-Aki dans ce module. Mon objectif final est de m'appuyer sur SPT-Aki et que ce module soit uniquement axé sur les fonctionnalités SIT.

## Comment compiler ? 
[Document de Compilation](COMPILE.md)

## Liste de remerciement
- L'Équipe d'SPT-Aki
- L'Équipe MTGA
- La Communauté de modding SPT-Aki
- DrakiaXYZ ([BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain))
- Dvize ([NoBushESP](https://github.com/dvize/NoBushESP))
- Contributeur SIT

## License

- Les projets DrakiaXYZ contiennent la licence MIT.
- 95% du noyau original et des fonctionnalités pour le jeu en solo ont été réalisés par les équipes de SPT-Aki. Il peut y avoir des licences les concernant dans cette source.
- Aucun de mes travaux n'est sous licence. Il s'agit uniquement d'un projet pour le plaisir. Je me fiche de ce que vous en ferez.
