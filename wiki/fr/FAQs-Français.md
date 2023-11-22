## Ecran infini "Chargement des données de profil...".

Causé par :
- Une installation défectueuse.
- Un problème de connexion au serveur, lié à vos paramètres IP.
- Un proxy Windows qui bloque la connexion.

Assurez-vous que la redirection de port est correctement configurée afin que vous puissiez vous connecter à votre IP externe.

Si vous jouez en solo, n'utilisez aucune de ces options, laissez 127.0.0.1 et désactivez External IP Finder.
---

### Où dois-je installer les mods ?

### Mods clients
Installez les mods clients dans `<dossier du jeu>/BepInEx/plugins/`.

### Mods serveur
Installez les mods serveur dans `<dossier serveur>/users/mods/`.
---

## Étape de configuration du DDNS. Si vous n'avez pas d'adresse IP publique statique et que vous souhaitez utiliser un nom de domaine pour vous connecter au serveur.

### Etape 1
Remplacer `"ip" : "127.0.0.1"` dans ces deux fichiers par l'adresse IP de la carte réseau de votre ordinateur (__PAS VOTRE ADDRESSE IP PUBLIQUE__) 
ou utilisez 0.0.0.0 à la place.

`<Dossier du serveur>/Aki_Data/Server/configs/http.json`

`<Dossier serveur>/Aki_Data/Server/database/server.json``.

et assurez-vous que ces deux fichiers ont la même adresse IP.

### Étape 2
Localisez le SIT.Aki-Server-Mod `coopConfig.json` dans le répertoire `<Dossier serveur>/user/mods/SIT.Aki-Server-Mod/config/`.

Changez "externalIP" : "127.0.0.1" par votre nom de domaine. Par exemple, vous pouvez changer "externalIP" : "127.0.0.1" en votre nom de domaine : "externalIP" : "votredomaine.com".

__Définissez useExternalIPFinder à false__

__Maintenant, vous êtes prêt à partir. Amusez-vous bien.

---
