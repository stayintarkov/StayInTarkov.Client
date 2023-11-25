# Usando o Server SPT-Aki no Linux

### Passo 1: Construir o Servidor
Siga o Leia-me no [Repositório do Servidor SPT-Aki](https://dev.sp-tarkov.com/SPT-AKI/Server). Geralmente:
```bash
git clone https://dev.sp-tarkov.com/SPT-AKI/Server.git
cd Server/project
git fetch
# Se você precisar mudar para uma branch diferente, por exemplo, 0.13.5.0
# git checkout 0.13.5.0
git lfs fetch++
git lfs pull
npm install
npm run build:release # ou build:debug
# O servidor vai ser construido em ./build
```
**Copie ou mova a build do servidor para outro lugar! NÃO rode o servidor diretamente do diretório da build! O diretorio será deletado e recriado durante a build!**

### Passo 2: Instale o Mod Servidor do SIT
A partir daqui, é basicamente a mesma coisa que hospedar no Windows.
- Baixe e instale o ultimo Mod Servidor
- Mude as configurações se precisar
- Abra o servidor no `/caminho/para/Aki.Server.exe`