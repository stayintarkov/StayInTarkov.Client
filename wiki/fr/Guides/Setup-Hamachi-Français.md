# Guide d'installation

## Voici un guide simple pour installer [SIT.Core](https://github.com/stayintarkov/StayInTarkov.Client) et le faire fonctionner avec des amis en utilisant [Hamachi](https://www.vpn.net/) (ou un programme similaire) et [SIT.Launcher](https://github.com/stayintarkov/SIT.Launcher.Classic).

### LISEZ CECI AVANT DE COMMENCER !
Vous devez acheter et avoir une installation active de [Escape From Tarkov](https://www.escapefromtarkov.com/) pour que cela fonctionne.

Vous devez installer [Hamachi](https://www.vpn.net/) ou quelque chose de similaire. Dans ce guide, je vais utiliser Hamachi.

Ce guide a été testé pour SIT.Core.Release-64 et SIT-Launcher.Release-71.

C'est la façon dont j'ai fait fonctionner ce mod, il peut y avoir des étapes qui ne sont pas nécessaires et ce guide peut devenir obsolète très rapidement au fur et à mesure que le mod est mis à jour !

Vous pouvez également appliquer ce guide à des scénarios où l'hôte a des ports transférés correctement, il suffit de sauter la partie Hamachi et d'utiliser l'Ipv4 public de l'hôte.

# Guide d'installation du serveur 
### Passez si c'est votre ami qui héberge le serveur.
1. Créez la même structure de dossiers que celle décrite dans le guide d'installation du client.
Mais cette fois-ci, créez également un dossier appelé "Server"\
SITCOOP/Serveur
2. Télécharger et extraire la dernière version de [SPT-AKI Stable Release](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases) dans le dossier Serveur.
3. Lancez Aki.Server.exe, vous pouvez le fermer une fois qu'il dit "Happy playing".
4. Téléchargez [SIT.Aki server mod](https://github.com/stayintarkov/SIT.Aki-Server-Mod) et installez-le comme vous le feriez pour n'importe quel autre mod sur AKI Server.
Le chemin du dossier du mod devrait ressembler à ceci:\
C:\SITCOOP\Server\user\mods\SIT.Aki-Server-Mod-master\
Pour être sûr de ne pas avoir de dossiers supplémentaires, le dossier SIT.Aki-Server-Mod-master doit contenir le fichier package.json.
5. Configurez http.json et coopconfig.json comme décrit dans [Paulov-t's HOSTING.md](https://github.com/stayintarkov/StayInTarkov.Client/wiki/Hosting-French).
Si vous utilisez Hamachi, utilisez Hamachi Ipv4 pour http.json et coopconfig.json.
*n'utilisez jamais 127.0.0.1 ou Localhost!*
6. Démarrez le serveur (en tant qu'administrateur) et allez à la section d'installation du client.

# Installation de SIT.Launcher et SIT.Core en tant que client.

1. Naviguez vers le dossier désiré où vous voulez faire l'installation, pour moi j'ai décidé de l'installer dans le dossier "C:\SITCOOP".
2. Créez les dossiers "Game" et "Launcher" dans le dossier "SITCOOP".
3. Téléchargez et extrayez SIT.Launcher dans le dossier "Launcher".
4. Exécuter SIT.Launcher.exe et installer le jeu dans le dossier "Game" que nous avons créé précédemment.
5. Une fois l'installation terminée, vous devez fermer le lanceur.
6. Créer un dossier appelé "AkiSupport" dans le dossier "Launcher".
7. Créer des dossiers dans "AkiSupport" de façon à ce que les chemins d'accès ressemblent à ceci:\
C:\SPCOOP\Launcher\AkiSupport\Bepinex\Patchers\
C:\SPCOOP\Launcher\AkiSupport\Bepinex\Plugins\
C:\SPCOOP\Launcher\AkiSupport\Managed\
6. Si votre ami héberge le serveur, copiez son Ipv4 Hamachi à partir du client Hamachi, si vous êtes l'hôte, copiez votre propre Ipv4 Hamachi, **127.0.0.1 ne fonctionnera pas!**.
7. Démarrez le lanceur et changez le champ du serveur pour qu'il contienne l'adresse IP de votre ami.
Example: http://100.100.100.100:6969
8. Saisissez le nom d'utilisateur et le mot de passe souhaités. 

**Le nom d'utilisateur et le mot de passe sont stockés en texte brut sur l'ordinateur de l'hôte, n'utilisez pas un nom d'utilisateur / mot de passe que vous utilisez ailleurs ou que vous ne souhaitez pas que l'hôte voie!**.

9. Allez dans les paramètres du lanceur et assurez-vous que les options "Automatically Install SIT", "Force Install Latest SIT..." et "Automatically Install AKi Support" sont activées.

**Félicitations, vous avez maintenant une nouvelle installation de SIT.**

**enjoy**

Créé par ppyLEK *(petites modifications par SlejmUr)* Traduction par cocorico8