# Utilisation du serveur SPT-Aki sous Linux


### Étape 1 Construire le serveur
Suivez le readme sur le repo [Server](https://dev.sp-tarkov.com/SPT-AKI/Server) de SPT-Aki. Généralement :
```bash
git clone https://dev.sp-tarkov.com/SPT-AKI/Server.git
cd Serveur/projet
git fetch
# Si vous avez besoin de passer à une branche différente, par exemple, 0.13.5.0
# git checkout 0.13.5.0
git lfs fetch
git lfs pull
npm install
npm run build:release # ou build:debug
# Le serveur sera construit dans ./build
```
**Copiez ou déplacez la compilation du serveur vers un autre endroit ! NE PAS exécuter le serveur directement à partir du répertoire de construction ! Le répertoire de compilation sera supprimé et recréé lors d'une compilation.**


### Étape 2 Installer le mod SIT Server
A partir de maintenant, c'est essentiellement la même chose que pour l'hébergement sous Windows :
- Télécharger et installer la dernière version du mod serveur
- Modifier les fichiers de configuration si nécessaire
- Lancer le serveur par `/path/to/Aki.Server.exe`