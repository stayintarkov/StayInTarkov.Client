
<div align=center style="text-align: center">
<h1 style="text-align: center"> StayInTarkov.Client </h1>
SPT-Aki 서버와 함께 사용할 수 있는 Escape From Tarkov BepInEx 모듈로, 최종 목표는 '오프라인' 협동 모드입니다. 
</div>

---

<div align=center>

![GitHub all releases](https://img.shields.io/github/downloads/stayintarkov/StayInTarkov.Client/total) ![GitHub release (latest by date)](https://img.shields.io/github/downloads/stayintarkov/StayInTarkov.Client/latest/total)

[English](README.md) **|** [简体中文](README_CN.md) **|** [Deutsch](README_DE.md) **|** [Português-Brasil](README_PO.md) **|** [日本語](README_JA.md) **|** [한국어-Korean](README_KO.md) 
</div>

---

## Stay In Tarkov 상태

* Stay In Tarkov는 SIT 팀에 의해 활발히 개발 중입니다.
* Pull Request와 기여는 항상 받아들여집니다 (작동한다면!)

--- 

## 소개

Stay in Tarkov 프로젝트는 Battlestate Games(BSG)가 Escape from Tarkov의 순수 PvE 버전을 만들기를 꺼리면서 탄생했습니다.
이 프로젝트의 목표는 간단합니다. 진행 상황을 유지하면서 협동 PvE 경험을 만드는 것입니다. 
만약 BSG가 공식 게임에서 이와 유사한 기능을 만들기로 결정하거나, 이 프로젝트가 DMCA 요청을 받는다면 즉시 프로젝트가 종료됩니다.

## 면책 조항 ( 부인 설명 )

* 이 프로젝트를 사용하려면 게임을 구매해야 합니다. 게임은 [https://www.escapefromtarkov.com](https://www.escapefromtarkov.com)에서 구매할 수 있습니다. 
* 이 프로젝트는 결코 치트를 위해 만들어진 것이 아닙니다. (이 프로젝트는 치트로 인해 라이브 경험이 파괴된 것을 해결하기 위해 만들어졌습니다)
* 이 프로젝트는 결코 불법적으로 게임을 다운로드하기 위해 만들어진 것이 아닙니다. (불법 다운로드를 시도하는 사용자를 차단하는 기능이 있습니다!)
* 이 프로젝트는 순수하게 교육적인 목적으로 만들어졌습니다. Unity 및 TCP/UDP/Web Socket Networking을 배우기 위해 사용하였으며, BattleState Games에서 많은 것을 배웠습니다 \o/.
* 나는 BSG나 다른 프로젝트를 진행 중인 Reddit 또는 Discord 사용자와 관련이 없습니다. 이 프로젝트에 대해 SPTarkov subreddit 또는 Discord에 문의하지 마십시오.
* 이 프로젝트는 SPTarkov와 공식적으로 관련이 없지만, SPTarkov의 우수한 서버를 사용합니다.
* 나는 이 프로젝트에 대한 지원을 제공하지 않습니다. 이 프로젝트는 "있는 그대로" 제공됩니다. 사용자가 직접 이 프로젝트가 작동하는지 여부를 판단해야 합니다.

## 지원, 도움

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/N4N2IQ7YJ)
* Ko-Fi 링크는 Paulov에게 커피를 사는 것입니다.
* Pull Request는 장려됩니다. 모든 기여자에게 감사드립니다!
* 도움이나 해결책을 기대하며 돈을 건네지 마세요.
* 이것은 취미로, 재미로 하는 프로젝트입니다. 진지하게 대하지 마세요.
* Paulov: 이것은 반쯤 고장난 시도이지만 최선을 다해 고칠 것입니다. SIT 기여자: "우리는 할 수 있어요!"
* [SIT Discord](https://discord.gg/f4CN4n3nP2) 를 통해 커뮤니티에 참여하고 서로 도움을 주고 커뮤니티 서버를 만들 수 있습니다.


## SPT-AKI 요구 사항
* tay in Tarkov는 협동 연결을 위해 [최신 AKI 서버](https://dev.sp-tarkov.com/SPT-AKI/Server) 가 필요합니다. SPT-Aki에 대해 자세히 알아보려면 [여기](https://www.sp-tarkov.com/)를 참조하세요.
* __**참고:**__ SIT 클라이언트 모드는 SPT-Aki 클라이언트에 설치되어서는 **안됩니다**.
대신 별도의 Tarkov 복사본에 설치되어야 합니다. 자세한 내용은 설치>클라이언트 섹션을 참조하십시오.

## [위키](https://github.com/stayintarkov/StayInTarkov.Client/blob/master/wiki/Home.md)
**위키는 다양한 기여자들에 의해 구성되었습니다. 모든 안내는 또한 위키 디렉토리 내의 소스에 유지되고 있습니다.**
  - ### [설치 메뉴얼](./wiki/ko/Home-Korean.md)
  - ### [FAQs - 질문 답변](./wiki/ko/FAQs-Korean.md)

## 설치

### 개요
SIT는 크게 2개의 주요 요소와 런처로 구성되어 있습니다.
- [SIT SPT-Aki Server Mod](https://github.com/stayintarkov/SIT.Aki-Server-Mod) ( 서버 모드 )
- SIT 클라이언트 모듈 (이 저장소!), Tarkov의 인스턴스에 설치됩니다.
- [SIT Manager](https://github.com/stayintarkov/SIT.Manager.Avalonia) (<s>[SIT Manager](https://github.com/stayintarkov/SIT.Manager) 또는 [SIT Launcher Classic](https://github.com/stayintarkov/SIT.Launcher.Classic)</s> 모두 보관됨)
  - <s>SIT Manager를 사용하는 것이 좋습니다. 클래식 런처는 기존의 클래식 런처 사용자를 위해 명시적으로 언급되었습니다.</s>

SIT 설치를 위해 다음과 같은 디렉토리 구조를 생성하는 것이 권장됩니다. 이 구조는 다음 섹션에서 참조될 것입니다.
```
SIT/
├── server/      # SPT-Aki Server Mod
├── game/        # EFT Client
└── launcher/    # SIT Manager or Classic Launcher
```

### 서버 설치
- [SIT SPT-Aki Server Mod 레포지토리](https://github.com/stayintarkov/SIT.Aki-Server-Mod) 의 지침에 따라 서버를 설치하고 구성하세요. 설치는 `SIT/server` 폴더에 이루어집니다.
- 협동을 위해 정확히 한 명의 사용자만 서버를 실행해야 합니다. 이 사용자는 포트 포워딩을 해야 하거나, 그룹이 Hamachi나 기타 VPN 솔루션을 통해 연결해야 합니다. 이러한 작업을 수행하는 방법을 모르는 경우 SIT 디스코드에서 도움을 받을 수 있는지 확인해 보세요.
  - 기본 SPT에서는 로컬 서버를 실행한 다음 클라이언트를 시작하여 해당 서버에 연결하는 것에 익숙할 것입니다. 그러나 SIT에서는 한 명이 모딩된 서버를 실행하고 다른 모든 사람들이 인터넷을 통해 해당 서버에 연결합니다.

### 런처 설치
- SIT Manager 저장소의 지침을 따라 설치하세요. `SIT/launcher` 폴더에 설치합니다.

### 클라이언트 설치
- **모든 사용자**는 SIT 클라이언트 모드를 설치해야 합니다. 원하는 경우 SIT Manager를 사용하거나 수동으로 설치할 수 있습니다.
- **SPT를 이미 사용 중인 경우**: 기존 SPT 설치에 SIT 클라이언트 모드를 **설치하지 마세요.** 현재 SIT 클라이언트 모드는 SPT-Aki 클라이언트와 호환되지 않으므로 Tarkov의 별도 복사본에 설치해야 합니다.

#### SIT Manager 방법
- 실제 Escape from Tarkov(EFT) 설치 폴더의 내용을 현재 비어 있는 `SIT/game` 폴더로 복사하세요.
  - 기본 위치에 Tarkov를 설치한 경우, `C:\Battlestate Games\EFT` 폴더 안에 있을 것입니다.
- `SIT/launcher/SIT.Manager.exe` 를 실행하세요.
- 매니저를 대상 게임 디렉토리로 설정하세요.
  - 왼쪽 하단에 있는 `Settings` 를 엽니다.
  - `Install Path` 를 `X:\<Full_Path_To>\SIT\game`로 설정하거나, Change 버튼을 사용하여 `SIT/game` 폴더를 선택하세요. 
    - `X` 와 `<Full_Path_To>` 를 해당 폴더의 경로로 대체하세요.
  - Settings를 닫습니다.
- 왼쪽의 `Tools` 메뉴를 열고 `+ Install SIT` 를 선택하세요.
- 원하는 SIT 버전을 선택하세요 (잘 모르는 경우 최신 버전을 선택하세요).
- `Install` 을 클릭하세요.

<details>
 <summary>수동 설치 방법</summary>
SIT Manager가 수행하는 것과 동일한 단계입니다. 특별한 이유가 없다면 SIT Manager를 사용하는 것이 훨씬 빠르고 쉽습니다. (정말로, 여기서 무언가를 숨기는 것은 없습니다. 이 단계들은 [the manager code](https://github.com/stayintarkov/SIT.Manager/blob/master/SIT.Manager/Classes/Utils.cs#L613)의 일반적인 설명입니다.)

- 실제 Escape from Tarkov(EFT) 설치 폴더의 내용을 현재 비어 있는 `SIT/game` 폴더로 복사하세요.
  - 기본 위치에 Tarkov를 설치한 경우, `C:\Battlestate Games\EFT` 폴더 안에 있을 것입니다.
- `SIT/game` 폴더에 다음과 같은 디렉토리를 생성하세요:
  - `SITLauncher/`
  - `SITLauncher/CoreFiles/`
  - `SITLauncher/Backup/Corefiles/`
- 이 저장소의 [릴리스 페이지](https://github.com/stayintarkov/StayInTarkov.Client/releases) 에서 원하는 클라이언트 모드 버전을 다운로드하여 `SIT/game/SITLauncher/CoreFiles` 폴더에 저장하세요 (잘 모르는 경우 최신 버전을 선택하세요).
  - `SIT/game/SITLauncher/CoreFiles/StayInTarkov-Release/` 폴더를 생성하세요.
  - 릴리스 아카이브의 내용을 해당 폴더에 압축 해제하세요.
- `SIT/game` 디렉토리를 정리하세요.
  - 다음 파일과 디렉토리를 삭제하세요:
    - `BattleEye/` \*
    - `EscapeFromTarkov_BE.exe` \*
    - `cache/`
    - `ConsistencyInfo`
    - `Uninstall.exe`
    - `Logs/`
  - \* 우려되는 경우, 이 방법은 실제 Tarkov에서 사용하여 치팅하는 데 사용할 수 없습니다. SPT(그리고 SIT)는 SPT-Aki 서버에서 BattleEye를 실행하지 않기 때문에 BattleEye 실행 파일/파일을 사용하지 않습니다. 주의하시고, 실제 디렉토리에서 이러한 파일을 삭제하지 마십시오. 최선의 경우 설치가 망가져 실제 서버에 연결할 수 없게 될 수 있습니다. 최악의 경우 BattleEye 감지가 트리거되어 계정/IP/HWID가 표시될 수 있습니다.
- 필요한 경우 복사한 Tarkov를 다운그레이드하세요
  - 실제 Tarkov의 버전이 단계 3에서 선택한 SIT 버전과 다른 경우 다운그레이드해야 합니다.
    - 실제 Tarkov의 버전은 BSG 런처 오른쪽 하단에 있는 5자리 숫자입니다.
  - SIT는 Tarkov를 다운그레이드하기 위한 도구를 유지하지 않습니다. Tarkov 다운그레이드에 대한 지침은 [여기](https://hub.sp-tarkov.com/doc/entry/49-a-comprehensive-step-by-step-guide-to-installing-spt-aki-properly/)에서 찾을 수 있습니다.
    - 7, 8, 9단계를 따르세요. "DowngradePatchers" 폴더에는 원하는 폴더를 사용하고, "SPTARKOV" 폴더에는 `SIT/game` 폴더를 사용하세요.
	- 여기서 문제가 발생하면, SIT는 DowngradePatcher를 유지하지 않습니다. SPT 개발자에게 문의할 수 있지만, 다른 SIT 주제에 대한 지원은 제공하지 않을 것이라는 점을 이해하세요. **절대로** 다른 SIT 주제에 대한 도움을 요청하지 마세요. 그들은 도움을 제공하지 않을 것입니다.
      - 그렇지만, 가지고 있는 문제가 단순한 오류가 아닌 합당한 문제라면, SIT 팀이 이미 인지하고 보고한 가능성이 매우 높습니다. SIT 매니저도 Downgrade Patcher를 사용합니다.
- BepInEx v5.4.22 설치
  - [아카이브](https://github.com/BepInEx/BepInEx/releases/download/v5.4.22/BepInEx_x64_5.4.22.0.zip)를 다운로드하세요.
  - 를 다운로드하세요. `SIT/game` 폴더에 압축 해제하세요.
    - `SIT/game` 폴더에 `BepInEx` 폴더, `doorstop_config.ini` 파일, `changelog.txt` 파일 및 `winhttp.dll` 파일이 포함되어 있어야 합니다.
  - `SIT/game/BepInEx/plugins` 폴더를 생성하세요.
- SIT 클라이언트 DLL 설치
  - Assembly-CSharp.dll
    - 원래의 `SIT/game/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll` 을 `SIT/game/SITLauncher/Backup/CoreFiles/` 로 백업하세요.
    - 원래의 dll을 `SIT/game/SITLauncher/CoreFiles/StayInTarkov-Release/Assembly-CSharp.dll` 로 교체하세요.
  - `SIT/game/SITLauncher/CoreFiles/StayInTarkov-Release/StayInTarkov.dll` 을 `SIT/game/BepInEx/plugins/` 로 복사하세요.
- Aki 클라이언트 DLL 설치
  - [SPT 릴리즈 페이지](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases) 에서 최신 SPT-AKI 릴리스를 다운로드하세요.
  - 릴리스에서 `EscapeFromTarkov_Data/Managed/Aki.Common.dll` 과 `EscapeFromTarkov_Data/Managed/Aki.Reflection.dll` 두 개의 파일을 `SIT/game/EscapeFromTarkov_Data/Managed/` 로 압축 해제하세요. 그리고 행운을 빕니다. 설치가 완료되었을 것입니다.
</details>

### Playing
- **서버 호스트만**: 서버 호스트가 수정된 서버를 시작하세요 (포트 포워딩 / VPN 또는 Hamachi를 통해 그룹의 나머지에게 연결됨)
  - 서버모드 저장소의 지침에 따라 IP 주소를 구성했는지 확인하세요!
- **모두**: SIT 매니저를 시작하고 호스트의 IP 및 포트를 입력한 다음 재생 버튼을 클릭하세요!
  - 누구나 레이드 로비를 시작할 수 있으며, 위치/시간/보험을 선택한 후 "호스트 레이드"를 클릭하여 플레이어의 수와 원하는 설정을 구성한 다음 시작하세요. 다른 모든 플레이어들은 시작 후 로비가 표시되며 그때 참여합니다. (게임은 모든 플레이어가 로드될 때까지 시작되지 않으며, 일반적인 타르코프와 같습니다.)

## FAQ

### BSG의 Coop 코드를 Coop에서 사용할 수 있나요?
아닙니다. BSG 서버 코드는 자명한 이유로 클라이언트로부터 숨겨져 있습니다. 따라서 BSG의 Coop 구현은 PvPvE와 동일한 온라인 서버를 사용합니다. 하지만 우리는 이를 볼 수 없으므로 사용할 수 없습니다.

### Aki BepInEx (클라이언트 모드) 모듈은 지원됩니까?
다음 Aki 모듈은 지원됩니다.
- aki-core
- Aki.Common
- Aki.Reflection

### SPT-AKI 클라이언트 모드는 작동합니까?
이는 패치가 얼마나 잘 작성되었는지에 따라 달라집니다. 만약 패치가 직접적으로 GCLASSXXX 또는 PUBLIC/PRIVATE를 대상으로 한다면, 작동하지 않을 가능성이 높습니다.

### 왜 Aki 모듈 DLL을 사용하지 않나요?
SPT-Aki DLL은 SPT-Aki 자체의 해독 기술을 위해 특별히 작성되었으며, 현재 Paulov의 기술은 Aki 모듈과 잘 작동하지 않습니다.
따라서 SPT-Aki의 많은 기능을 이 모듈로 이식했습니다. 최종 목표는 SPT-Aki에 의존하고 이 모듈은 SIT만을 위한 기능에만 집중하는 것입니다.

## 컴파일하는 방법은?
[컴파일 문서](COMPILE.md)를 참조하세요.

## 감사의 글
- PT-Aki 팀 (사용된 각 코드 파일에 대한 크레딧이 제공되고, 지원을 위해 그들의 개발팀에 많은 사랑을 보냅니다)
- SPT-Aki 모딩 커뮤니티
- DrakiaXYZ (이 프로젝트에 [BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain) & [Waypoints](https://github.com/DrakiaXYZ/SPT-Waypoints) 가 통합되어 있습니다)
- SIT 팀과 원래 기여자들

## 라이선스

- DrakiaXYZ 프로젝트는 MIT 라이선스를 포함합니다.
- SPT-Aki 팀이 작성한 원래의 코어 및 싱글 플레이어 기능은 99% 완료되었습니다. 이 소스에는 그들과 관련된 라이선스가 있습니다.
- 제가 작성한 작업에는 라이선스가 없습니다. 이것은 단순히 재미를 위한 프로젝트입니다. 사용에 대해 신경쓰지 않습니다.
