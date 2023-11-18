# 如何在Linux上运行服务器
### 步骤 1 从源代码编译Aki服务器
详见 SPT-Aki 服务器仓库中的说明 [Server](https://dev.sp-tarkov.com/SPT-AKI/Server)。 大致操作:
```bash
git clone https://dev.sp-tarkov.com/SPT-AKI/Server.git
cd Server/project
git fetch
# 如果需要切换至其他分支, 比如 0.13.5.0
# git checkout 0.13.5.0
git lfs fetch
git lfs pull
npm install
npm run build:release # 或者 build:debug
# 服务器会被编译进 ./build 文件夹
```
**一定要把编译好的服务器移动或者复制到别的位置！ 不要直接从编译文件夹运行服务器！编译文件夹会在每次编译中删除并重建！**

### 步骤 2 安装SIT的服务器Mod
从这里开始，就和在Windows上开服操作一致了:
- 下载并安装最新的服务器Mod
- 如有需要更改服务器设置文件
- 通过 `/path/to/Aki.Server.exe` 运行服务器
