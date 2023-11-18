# Using the SPT-Aki Server in Linux

### Step 1 Build the Server
Follow the readme on SPT-Aki's [Server](https://dev.sp-tarkov.com/SPT-AKI/Server) repo. Generally:
```bash
git clone https://dev.sp-tarkov.com/SPT-AKI/Server.git
cd Server/project
git fetch
# If you need to switch to a different branch, for example, 0.13.5.0
# git checkout 0.13.5.0
git lfs fetch
git lfs pull
npm install
npm run build:release # or build:debug
# The server will be built in ./build
```
**Copy or move the server build to somewhere else! DON'T run the server directly from the build directory! The build directory will be deleted and recreated again during a build.**

### Step 2 Install the SIT Server Mod
From here on, it is basically the same as hosting on Windows:
- Download and install the latest server mod
- Change the config files if needed
- Run the server by `/path/to/Aki.Server.exe`
