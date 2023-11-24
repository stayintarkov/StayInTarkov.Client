# Guide d'installation étapes par étapes de Stay In Tarkov


# Conditions préalables


Avant de commencer, assurez-vous que la dernière version d'Escape From Tarkov a été téléchargée et installée à l'aide du Battlestate Games Launcher. Stay In Tarkov ne fonctionnera pas avec une copie périmée ou illégitime du jeu.


Tout au long de ce guide, nous ferons référence à `SIT_DIR` comme répertoire racine pour l'installation de Stay In Tarkov. Dans ce répertoire, nous créerons trois dossiers séparés pour garder les choses organisées :


- Un dossier `server` pour le serveur SPT-AKI.
- Un dossier `launcher` pour le SIT Launcher.
- Un dossier `game` pour les fichiers du jeu Escape From Tarkov.


*Envisagez d'utiliser un outil comme [7zip](https://7-zip.org/) ou WinRAR pour décompresser les fichiers compressés.*


# Installation


## 1. [SIT Launcher](https://github.com/stayintarkov/SIT.Launcher.Classic/releases) (en utilisant l'installation automatique)


1. Téléchargez la dernière version du "SIT Launcher" depuis la page [Releases](https://github.com/stayintarkov/SIT.Launcher.Classic/releases).
2. Dézippez le fichier et extrayez le contenu dans `SIT_DIR/launcher`.
3. Exécutez `SIT.Launcher.exe`.
4. La première fois que vous lancez le lanceur, il vous demandera d'effectuer une installation :


    *"No OFFLINE install found. Would you like to install now ? "*


    Cliquez sur "Oui".


5. Sélectionnez `SIT_DIR/game` comme répertoire d'installation.
6. Laissez le lanceur copier vos fichiers de jeu, cela peut prendre quelques minutes.


## 2. [SPT-AKI Server] (https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases)


1. Téléchargez la dernière version du `SPT-AKI Server` depuis la page [Releases](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases).
2. Dézippez le fichier et extrayez le contenu dans `SIT_DIR/server`.


## 3. [SIT Server Mod](https://github.com/stayintarkov/SIT.Aki-Server-Mod)
1. Téléchargez le fichier zip du mod serveur depuis [GitHub](https://github.com/stayintarkov/SIT.Aki-Server-Mod) (cherchez-le sous le gros bouton vert : *Code > Download Zip*).
2. Dézippez le fichier et extrayez le contenu dans `SIT_DIR/server/user/mods`.


    *Le répertoire `user/mods` est automatiquement créé lorsque le serveur est lancé pour la première fois. Exécutez `Aki.Server.exe` pour créer le répertoire. Arrêtez et fermez le serveur une fois que le répertoire a été créé afin que nous puissions continuer le processus d'installation.*


# Configuration du serveur


## Hébergé sur localhost (pour les tests)


### Serveur
1. Ouvrez le fichier de configuration du serveur coop dans `SIT_DIR/server/user/mods/SIT.Aki Server-Mod/config/coopConfig.json`.


    *Le fichier `coopConfig.json` est automatiquement créé lorsque le mod serveur est lancé pour la première fois. Lancez `Aki.Server.exe` pour créer le fichier. Arrêtez et fermez le serveur une fois que le fichier a été créé afin que nous puissions continuer le processus d'installation.*


    *Note : Faites des modifications au fichier en utilisant Notepad ou un éditeur de texte qui n'introduira pas de formatage. N'utilisez pas Microsoft Word.*
2. Définissez `externalIP` à `127.0.0.1`.
3. Définissez `useExternalIPFinder` à `false`.
4. Optionnellement, mettez `logRequests` à `false` dans `SIT_DIR/server/Aki_Data/Server/configs/http.json` pour empêcher le spam des logs.


### Launcher
Connectez-vous en utilisant `http://127.0.0.1:6969` comme serveur.


*Vous ne pourrez pas inviter d'autres personnes à rejoindre votre jeu en utilisant localhost, mais cela peut être utile pour déboguer des problèmes de connexion. Utilisez ceci pour confirmer que le jeu et les mods sont installés correctement.*



## Hoster avec redirection de port


### Configuration
La redirection de port vous permet d'utiliser votre ordinateur local comme serveur pour un service public. En bref, votre routeur a une adresse IP _externe_ statique (immuable) que vous pouvez voir en allant sur https://www.whatismyip.com. Il s'agit de l'adresse IP que le monde voit comme étant la vôtre, même si de nombreux appareils se trouvent sur le réseau (par exemple, si vous allez sur whatismyip sur votre téléphone alors que vous êtes en Wi-Fi, vous verrez la même adresse IP que lorsque vous y allez sur votre ordinateur). Afin d'utiliser votre ordinateur pour le trafic externe et de permettre à vos amis de se connecter à un serveur fonctionnant sur votre machine, vous devez faire plusieurs choses :
1. **Ouvrez la page des paramètres de votre routeur** : allez sur la page web des paramètres de votre routeur (souvent accessible à l'adresse http://192.168.1.1, mais pas pour tous les routeurs - vérifiez l'étiquette de votre routeur, ou consultez le manuel pour connaître son numéro de modèle spécifique).
2. **Attribuez à votre ordinateur une adresse IP statique** : attribuez à votre machine une adresse IP locale statique. Il existe de nombreuses possibilités, l'une des conventions étant une adresse comprise entre 192.168.0.0 et 192.168.255.255 (assurez-vous toutefois qu'elle est différente de l'adresse IP de votre routeur).
3. **Mise en place de la redirection de port** : dans les paramètres de votre routeur, trouvez le parametre pour la redirection de port, puis rediriger les ports `6969` et `6970` (qui peuvent généralement être écrits comme `6969,6970`) vers l'adresse locale statique de votre ordinateur. Cette étape assurera que tout le trafic entrant sur ces ports sera dirigé vers votre ordinateur (par défaut, ces ports sont bloqués par votre routeur).
4. **Configurez votre pare-feu** : ouvrez (ou _autorisez_) les ports de trafic TCP entrant `6969` et `6970` dans les paramètres du pare-feu Windows (ou tout autre pare-feu que vous utilisez(Au premier lancement du server vous aurez normalement un prompt vous demandant si vous vouler ouvrir les port pour ceette application et vous cochez toutes les cases)). Cette étape assurera que votre ordinateur acceptera le trafic sur ces ports.
5. **Sécurisez vos ports** : cette étape n'est pas nécessaire, mais elle est recommandée pour la sécurité : mettez les adresses IP de vos amis (trouvées en utilisant whatismyip) sur liste blanche dans les paramètres de votre routeur. En fonction de votre routeur, cela peut être fait sur le même écran que l'étape 4, ou sur un écran séparé ; par exemple, sur les routeurs ASUS, vous devez définir l'"IP source" à l'adresse de votre ami sur l'écran de redirection de port. Vous devrez faire cela pour chaque ami avec lequel vous voulez jouer. Si un ami ne parvient pas à se connecter à l'avenir, il se peut que vous ayez oublié d'effectuer cette étape pour son adresse IP. Cette étape permet de s'assurer que seuls vos amis peuvent se connecter à votre serveur, et non n'importe qui sur Internet. Si vous ne faites pas cette étape, n'importe qui sur Internet pourra se connecter à votre serveur, ce qui n'est pas recommandé. **Note:** vous devrez également mettre sur liste blanche votre propre adresse IP interne, ou vous ne pourrez pas vous connecter à votre propre serveur !
6. **Mise à jour de la configuration HTTP** : allez dans `SIT_DIR\server\Aki_Data\Server\configs` et ouvrez `http.json` dans un éditeur de texte. Changez la valeur `ip` pour l'IP locale statique que vous avez assignée à votre machine. Cette étape vous permettra de vous connecter à votre serveur depuis votre propre ordinateur.
7. **Mise à jour de la configuration de la coop** : allez dans `SIT_DIR\server\user\mods\SIT.Aki-Server-Mod-master\config` et ouvrez `coopConfig.json` dans un éditeur de texte. Changez la valeur `externalIP` pour l'IP donnée par whatismyip. Cette étape permettra à vos amis de se connecter à votre serveur.


Maintenant votre serveur est prêt ! Pour vous connecter à votre propre serveur, lancez le launcher SIT et entrez votre adresse IP statique locale, comme `http://{ votre IP statique locale }:6969`. Vos amis se connecteront en utilisant l'IP donnée par whatismyip, comme `http://{ votre IP externe }:6969`.
Optionnellement, mettez `logRequests` à `false` dans `SIT_DIR/server/Aki_Data/Server/configs/http.json` pour éviter le spam des logs.


## Hébergé avec Hamachi VPN

### Serveur
1. Lancez Hamachi.
2. Trouvez l'adresse IPv4 indiquée dans le widget LogMeIn Hamachi et copiez-la. Nous utiliserons `100.10.1.10` comme exemple d'adresse IP pour ce guide.
3. Ouvrez le fichier de configuration du serveur coop dans `SIT_DIR/server/user/mods/SIT.Aki Server-Mod/config/coopConfig.json`.

    *Le fichier `coopConfig.json` est automatiquement créé lorsque le mod serveur est lancé pour la première fois. Lancez `Aki.Server.exe` pour créer le fichier. Arrêtez et fermez le serveur une fois que le fichier a été créé afin que nous puissions continuer le processus d'installation.*

    *Note : Faites des modifications au fichier en utilisant Bloc-notes ou un éditeur de texte qui n'introduira pas de formatage. N'utilisez pas Microsoft Word.*
4. Définissez `externalIP` à l'IP que nous avons copié de LogMeIn Hamachi : `100.10.1.10`.
5. Mettez `useExternalIPFinder` à `false`.
6. Ouvrez le fichier de configuration de la connexion au serveur de SPT-AKI dans `SIT_DIR/server/Aki_Data/Server/configs/http.json`.

     *Note : Modifiez le fichier en utilisant le Bloc-notes ou un éditeur de texte qui n'introduit pas de formatage. N'utilisez pas Microsoft Word.*
7. Définissez `ip` à `100.10.1.10`.
8. Optionnellement, mettez `logRequests` à `false` pour éviter le spam des logs.


### Launcher
Connectez-vous en utilisant l'adresse IPv4 indiquée dans le widget LogMeIn Hamachi.Notre exemple utilise `http://100.10.1.10:6969` comme serveur.


# Démarrer une partie


## 1. Démarrer le serveur


Lancez `Aki.Server.exe` depuis `SIT_DIR/server`.


## 2. Démarrer le jeu


Lancez le jeu via le `SIT Launcher`.


*La première fois que vous essayez de vous connecter avec de nouvelles informations d'identification, il vous sera demandé de créer un compte, cliquez sur "Oui" (les mots de passe sont stockés en texte brut, n'utiliser pas vos vrai mot de passe). Vous pouvez également être invité à Alt+F4 après le lancement du jeu, si c'est le cas, fermez le jeu et relancez-le via SIT Launcher.*


## 3. Créer un lobby


Consultez le fichier HOSTING.md de votre langue pour savoir comment créer un lobby.
Les guides HOSTING sont disponibles ici : https://github.com/stayintarkov/StayInTarkov.Client/tree/master/wiki


## Notes supplémentaires
1. Vos amis n'ont pas besoin de configurer le serveur. Il leur suffit d'installer SIT à l'aide du lanceur et de se connecter à votre serveur.
2. il est recommandé que tous les joueurs, vous et vos amis, utilisent la même version du SIT. Pour ce faire, cochez l'option "Forcer l'installation de la dernière version du SIT" dans le menu des paramètres du lanceur.
3. si vous laissez votre serveur ouvert en permanence (ce qui nécessite de désactiver le mode veille sur votre machine), vos amis pourront se connecter à tout moment. Ils peuvent éditer leurs chargements, utiliser le marché, entrer dans leur cachette, etc. et même jouer des raids en solo sans que vous soyez présent. Notez que si deux raids sont en cours en même temps, les deux raids se termineront lorsque l'un d'entre eux se terminera.
4. vous pouvez trouver des options de configuration supplémentaires dans `SIT_DIR\game\BepInEx\config\SIT.Core.cfg`, où vous pouvez, par exemple, désactiver le flux de spawn/kill dans le coin inférieur droit de l'écran pendant un raid. Comme il s'agit d'options client (et non serveur), il est fortement recommandé que tous les joueurs utilisent les mêmes options de configuration.
