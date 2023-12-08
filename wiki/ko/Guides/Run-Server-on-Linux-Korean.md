# 리눅스에서 SPT-Aki 서버 사용하기

### 단계 1: 서버 빌드하기
SPT-Aki의 [Server](https://dev.sp-tarkov.com/SPT-AKI/Server) 저장소의 readme를 따르세요. 일반적으로 다음과 같습니다:
```bash
git clone https://dev.sp-tarkov.com/SPT-AKI/Server.git
cd Server/project
git fetch
# 다른 브랜치로 전환해야 하는 경우, 예를 들어 0.13.5.0
# git checkout 0.13.5.0
# 특정 0.13.5.0 브랜치로 클론하고 싶은경우.
# git clone -b 0.13.5.0 https://dev.sp-tarkov.com/SPT-AKI/Server.git
git lfs fetch
git lfs pull
npm install
npm run build:release # 또는 build:debug
# 서버는 ./build 디렉토리에 빌드됩니다.
```
**서버 빌드를 다른 곳으로 복사하거나 이동하세요! 빌드 디렉토리에서 직접 서버를 실행하지 마세요! 빌드 디렉토리는 빌드 중에 삭제되고 다시 생성됩니다.**

### 단계 2: SIT 서버 모드 설치하기
이제부터는 Windows에서 호스팅하는 것과 기본적으로 동일합니다:
- 최신 서버 모드 다운로드 및 설치
- 필요한 경우 설정 파일 변경
- `/path/to/Aki.Server.exe` 로 서버 실행
