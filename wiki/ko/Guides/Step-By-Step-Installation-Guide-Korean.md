# 스텝 바이 스텝 Stay In Tarkov 설치 가이드     

# 전제 조건

시작하기 전에, 최신 버전의 Escape From Tarkov가 Battlestate Games 런처를 사용하여 다운로드 및 설치되었는지 확인해주세요. Stay In Tarkov는 오래된 버전이나 불법 복제본의 게임과는 호환되지 않습니다.

이 가이드에서는 Stay In Tarkov를 설치할 때 `SIT_DIR` 를 루트 디렉토리로 참조합니다. 이 디렉토리에서 세 개의 별도 폴더를 만들어 작업을 정리합니다:

- SPT-AKI 서버를 위한 `server` 폴더
- SIT 런처를 위한 `launcher` 폴더
- Escape From Tarkov 게임 파일을 위한 `game` 폴더

*압축 파일을 해제하기 위해 [7zip](https://7-zip.org/) 또는 WinRAR과 같은 도구를 사용할 수 있습니다.*

# 설치

## 1. [SIT Launcher](https://github.com/stayintarkov/SIT.Launcher.Classic/releases) (자동 설치 사용)

1. [릴리즈](https://github.com/stayintarkov/SIT.Launcher.Classic/releases) 페이지에서 최신 버전의 `SIT Launcher` 런처를 다운로드하세요.
2. 압축 파일을 풀고 내용물을 `SIT_DIR/launcher` 에 추출하세요.
3. `SIT.Launcher.exe` 를 실행하세요.
4. 런처를 처음 실행할 때 설치를 요청하는 메시지가 표시됩니다:

    *“No OFFLINE install found. Would you like to install now?”*

    “Yes” 를 클릭하세요.

5. 설치 디렉토리로 `SIT_DIR/game` 을 선택하세요.
6. 런처가 게임 파일을 복사하도록 하세요. 이 과정은 몇 분 정도 걸릴 수 있습니다.

## 2. [SPT-AKI 서버](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases)

1. [Releases](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases) 페이지에서 최신 버전의 `SPT-AKI Server` 서버를 다운로드하세요.
2. 압축 파일을 풀고 내용물을 `SIT_DIR/server` 에 추출하세요.

## 3. [SIT Server Mod](https://github.com/stayintarkov/SIT.Aki-Server-Mod)
1. 서버 모드의 zip 파일을 [GitHub](https://github.com/stayintarkov/SIT.Aki-Server-Mod) 에서 다운로드하세요 (Code > Download Zip 버튼 아래에서 찾을 수 있습니다).
2. 압축 파일을 풀고 내용물을 `SIT_DIR/server/user/mods` 에 추출하세요.

    *서버가 처음 실행될 때 `user/mods` 디렉토리가 자동으로 생성됩니다. `Aki.Server.exe` 를 실행하여 폴더를 생성하세요. 디렉토리가 생성된 후 서버를 중지하고 닫으세요. 설치 과정을 계속할 수 있도록 합니다.*

# 서버 설정

## 로컬호스트에서 호스팅(테스트용)

### 서버
1. `SIT_DIR/server/user/mods/SIT.Aki Server-Mod/config/coopConfig.json` 파일을 엽니다.

    *`coopConfig.json` 파일은 서버 모드가 처음 실행될 때 자동으로 생성됩니다. `Aki.Server.exe` 를 실행하여 파일을 생성하세요. 파일이 생성된 후 서버를 중지하고 닫으세요. 설치 과정을 계속할 수 있도록 합니다.*

    *참고: 파일을 편집할 때는 Notepad나 서식을 손상시키지 않는 텍스트 편집기를 사용하세요. Microsoft Word를 사용하지 마세요.*
2. `externalIP` 를 `127.0.0.1` 로 설정하세요.
3. `useExternalIPFinder` 를 `false` 로 설정하세요.
4. 필요에 따라 `SIT_DIR/server/Aki_Data/Server/configs/http.json` 에서 `logRequests` 를 `false` 로 설정하여 로그 스팸을 방지할 수 있습니다.

### 런처
서버로 `http://127.0.0.1:6969` 를 사용하여 연결하세요.

*로컬호스트를 사용하여 다른 사람을 초대하여 게임에 참여시킬 수는 없지만, 연결 문제를 디버깅하는 데 유용할 수 있습니다. 게임과 모드가 올바르게 설치되었는지 확인하기 위해 이를 사용하세요.*

## 포트 포워딩으로 호스팅하기

### 설정
포트 포워딩을 사용하면 로컬 컴퓨터를 공개적으로 접근 가능한 서버로 사용할 수 있습니다. 간단히 말해서, 라우터에는 변경되지 않는 정적 외부 IP 주소가 있습니다. 이 주소는 https://www.whatismyip.com 에 접속하여 확인할 수 있습니다. 이 IP 주소는 여러 기기가 네트워크에 연결되어 있더라도 세계에서 '당신'으로 인식되는 IP 주소입니다. 예를 들어, Wi-Fi에 연결된 휴대폰에서 whatismyip에 접속하면 컴퓨터에서 접속할 때와 동일한 IP를 볼 수 있습니다. 컴퓨터를 외부 트래픽에 사용하고 친구가 컴퓨터에서 실행 중인 서버에 연결할 수 있도록 하려면 몇 가지 작업을 수행해야 합니다:
1. **MAC 주소 찾기**: 단계 3에서 컴퓨터를 식별하기 위해 필요한 컴퓨터의 MAC 주소를 찾으세요. 명령 프롬프트를 열고 `ipconfig /all` 을 입력한 후 네트워크 어댑터(예: 이더넷 어댑터 이더넷) 아래에 있는 "물리적 주소"라고 표시된 줄을 찾으세요. 주소는 `00-00-00-00-00-00` 와 같은 형식일 것입니다.
2. **라우터 설정 페이지 열기**: 라우터 설정 웹 페이지에 접속하세요. 보통 http://192.168.1.1 을 통해 접속할 수 있지만, 모든 라우터에서 동일한 주소는 아닙니다. 라우터의 레이블을 확인하거나 특정 모델 번호에 대한 매뉴얼을 찾아보세요.
3. **컴퓨터에 고정 IP 주소 할당하기**: 컴퓨터(당신의 MAC 주소를 가진 장치)에 고정 로컬 IP 주소를 할당하세요. 선택할 수 있는 여러 가지 옵션이 있습니다. 일반적으로 192.168.0.0 - 192.168.255.255 범위 중 하나를 선택합니다(라우터 IP와 다른 IP여야 합니다).
4. **포트 포워딩 설정하기**: 라우터 설정에서 포트 포워딩을 찾고, `6969` 와 `6970` 포트를 (일반적으로 `6969,6970` 으로 작성할 수 있음) 방금 컴퓨터에 할당한 고정 로컬 주소로 포워딩하세요. 이 단계를 통해 해당 포트로 들어오는 모든 트래픽이 컴퓨터로 전달됩니다(기본적으로 라우터에서 해당 포트는 차단됩니다).
5. **방화벽 설정하기**: Windows 방화벽 설정(또는 사용하는 방화벽)에서 `6969` 와 `6970` 포트로 들어오는 TCP 트래픽을 열거나 허용하세요. 이 단계를 통해 컴퓨터가 해당 포트로부터의 트래픽을 수락할 수 있습니다.
6. **포트 보안 설정하기**: 이 단계는 필수는 아니지만 보안을 위해 권장됩니다. 라우터 설정에서 친구의 IP 주소(whatismyip을 통해 확인)를 화이트리스트에 등록하세요. 라우터에 따라 이 작업을 단계 4와 동일한 화면에서 수행해야 할 수도 있고 별도의 화면에서 수행해야 할 수도 있습니다. 예를 들어, ASUS 라우터에서는 포트 포워딩 화면에서 `source IP` 를 친구의 주소로 설정해야 합니다. 친구와 함께 플레이하려는 경우 이 작업을 각각의 친구에 대해 수행해야 합니다. 이 단계를 통해 인터넷에서 친구만 서버에 연결할 수 있으며, 누구나 연결할 수 있는 상태로 남기지 않습니다. 이 단계를 수행하지 않으면 인터넷의 누구나 서버에 연결할 수 있게 됩니다. 이는 권장되지 않습니다. **참고:** 컴퓨터 자신의 내부 IP 주소도 화이트리스트에 등록해야 합니다. 그렇지 않으면 자신의 서버에 연결할 수 없습니다!
7. **HTTP 구성 업데이트하기**: `SIT_DIR\server\Aki_Data\Server\configs` 로 이동하여 텍스트 편집기에서 `http.json` 파일을 엽니다. `ip` 값을 방금 컴퓨터에 할당한 고정 로컬 IP 주소로 변경하세요. 이 단계를 통해 자신의 컴퓨터에서 서버에 연결할 수 있습니다.
8. **Co-op 구성 업데이트하기**: `SIT_DIR\server\user\mods\SIT.Aki-Server-Mod-master\config` 로 이동하여 텍스트 편집기에서 `coopConfig.json` 파일을 엽니다. `externalIP` 값을 whatismyip에서 얻은 IP로 변경하세요. 이 단계를 통해 친구들이 서버에 연결할 수 있습니다.

이제 서버가 모두 설정되었습니다! 자신의 서버에 연결하려면 SIT 런처를 실행하고 로컬 고정 IP 주소인 `http://{ 로컬 IP }:6969`. 와 같이 입력하세요. 친구는 whatismyip에서 얻은 IP인 `http://{ 외부 IP }:6969` 와 같이 연결할 것입니다.
필요한 경우 `SIT_DIR/server/Aki_Data/Server/configs/http.json` 에서 `logRequests` 값을 `false` 로 설정하여 로그 스팸을 방지할 수 있습니다.

## Hamachi VPN으로 호스팅하기

### 서버
1. Hamachi를 실행하세요.
2. LogMeIn Hamachi 위젯에 표시된 IPv4 주소를 찾아 복사하세요. 이 가이드에서는 예시 IP로 `100.10.1.10` 을 사용하겠습니다.
3. `SIT_DIR/server/user/mods/SIT.Aki Server-Mod/config/coopConfig.json` 파일을 엽니다.

    *`coopConfig.json` 파일은 서버 모드가 처음 실행될 때 자동으로 생성됩니다. `Aki.Server.exe` 를 실행하여 파일을 생성하세요. 파일이 생성된 후 서버를 중지하고 닫으세요. 설치 과정을 계속할 수 있도록 합니다.*

    *참고: 파일을 편집할 때는 Notepad나 서식을 손상시키지 않는 텍스트 편집기를 사용하세요. Microsoft Word를 사용하지 마세요.*
4. `externalIP` 를 LogMeIn Hamachi에서 복사한 IP인 `100.10.1.10` 으로 설정하세요.
5. `useExternalIPFinder` 를 `false` 로 설정하세요.
6. `SIT_DIR/server/Aki_Data/Server/configs/http.json` 파일을 텍스트 편집기로 엽니다.

    *참고: 파일을 편집할 때는 Notepad나 서식을 손상시키지 않는 텍스트 편집기를 사용하세요. Microsoft Word를 사용하지 마세요.*
7. `ip` 를 `100.10.1.10` 으로 설정하세요.
8. 필요에 따라 `logRequests` 를 `false` 로 설정하여 로그 스팸을 방지할 수 있습니다.

### 런처
LogMeIn Hamachi 위젯에 표시된 IPv4 주소를 사용하여 연결하세요. 예시로 `http://100.10.1.10:6969` 를 서버로 사용합니다.

# 게임 시작하기

## 1. 서버 시작하기

`SIT_DIR/server` 에서 `Aki.Server.exe` 를 실행하세요.

## 2. 게임 시작하기

`SIT Launcher` 를 통해 게임을 실행하세요.

*새 자격 증명으로 연결하려고 시도하는 첫 번째 시도에서 계정을 만들라는 메시지가 표시됩니다. "Yes"를 클릭하세요(비밀번호는 평문으로 저장되므로 재사용하지 마세요). 게임이 시작된 후에 Alt+F4로 종료하라는 메시지가 표시될 수도 있으므로 게임을 닫고 다시 SIT 런처를 통해 실행하세요.*

## 3. 로비 생성하기

로비를 생성하는 방법은 해당 언어의 HOSTING.md를 참조하세요.
HOSTING 가이드는 다음 위치에서 확인할 수 있습니다: https://github.com/stayintarkov/StayInTarkov.Client/tree/master/wiki

## 추가 사항
1. 친구들은 서버를 설정할 필요가 없습니다. 그들은 런처를 사용하여 SIT를 설치하고 서버에 연결하기만 하면 됩니다.
2. 당신과 친구 모두가 동일한 SIT 버전을 사용하는 것이 좋습니다. 이를 위해 런처의 설정 메뉴에서 "Force install latest SIT" 옵션을 선택하세요.
3. 컴퓨터를 항상 실행하는 상태로 두면(슬립 모드를 해제해야 함), 친구들은 언제든지 연결할 수 있습니다. 그들은 로드아웃을 편집하고 플리마켓을 사용하며 은신처에 들어갈 수 있으며, 여러분이 없어도 솔로 레이드를 할 수도 있습니다. 두 개의 레이드가 동시에 진행 중인 경우, 두 레이드 중 하나가 종료되면 두 레이드 모두 종료됩니다.
4. `SIT_DIR\game\BepInEx\config\SIT.Core.cfg` 에서 추가적인 구성 옵션을 찾을 수 있습니다. 예를 들어, 레이드 중 우측 하단에 표시되는 스폰/킬 피드를 비활성화할 수 있습니다. 이는 클라이언트(서버가 아닌) 옵션으로, 모든 플레이어가 동일한 구성 옵션을 사용하는 것이 강력히 권장됩니다.
