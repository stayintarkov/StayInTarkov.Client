
<div align=center style="text-align: center">
<h1 style="text-align: center"> SIT.Core </h1>
SPT-Aki 서버와 함께 사용할 수 있는 Escape From Tarkov BepInEx 모듈로, 최종 목표는 '오프라인' 협동 모드입니다. 
</div>

---

<div align=center>

![GitHub all releases](https://img.shields.io/github/downloads/paulov-t/SIT.Core/total) ![GitHub release (latest by date)](https://img.shields.io/github/downloads/paulov-t/SIT.Core/latest/total)

[English](README.md) **|** [简体中文](README_CN.md) **|** [Deutsch](README_DE.md) **|** [Português-Brasil](README_PO.md) **|** [日本語](README_JA.md) **|** [한국어-Korean](README_KO.md) 
</div>

---

## SIT 상태

** EFT 0.13.5.0.25800 와 SPT-Aki 3.7.0 기준. **
* SPT-Aki는 일부 작동 상태입니다. Discord에서 최신 정보를 확인하세요. 매일 업데이트를 게시합니다.
* SIT는 천천히 발전하고 있습니다.

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

## 지원

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/N4N2IQ7YJ)
* **나는 이 프로젝트에 대한 지원을 제공하지 않습니다. 나 또는 다른 기여자가 작성한 모든 자습서는 선의의 의도로 제공됩니다. 이 프로젝트를 작동시키지 못하면, 라이브 버전으로 돌아가는 것을 권장합니다!**
* Ko-Fi 링크는 커피 한 잔을 사주는 것뿐입니다! (나 또는 내 아내에게)
* Pull Requests는 환영입니다. 모든 기여자에게 감사드립니다!
* 해결책이나 도움을 기대하고 돈을 지불하지 마십시오. 
* 이 프로젝트는 취미로 만들어졌으며, 재미를 위한 것입니다. 진지하게 다루지 마십시오. 
* 이것은 반쯤 망가진 시도이지만, 최선을 다해 수정하겠습니다. 

## SPT-AKI 요구사항
* Tarkov에서 Stay in Tarkov 모드를 사용하려면 [최신 버전의 AKI 서버](https://dev.sp-tarkov.com/SPT-AKI/Server)가 필요합니다. SPT-Aki에 대해 자세히 알아보려면 [여기](https://www.sp-tarkov.com/)를 방문하십시오.
* SPT-Aki 클라이언트에 이것을 설치하지 마십시오! 서버에만 설치하십시오!

## [위키](https://github.com/paulov-t/SIT.Core/wiki)
**위키는 여러 기여자에 의해 작성되었습니다. 모든 지침은 위키 디렉토리의 소스 내에서도 확인할 수 있습니다.**
  - ### [설치 매뉴얼](https://github.com/paulov-t/SIT.Core/wiki/Guides-English)
  - ### [FAQs](https://github.com/paulov-t/SIT.Core/wiki/FAQs-English)

## Coop

### Coop 요약
**주의**
* Coop는 개발 초기 단계입니다. 
* 대부분의 기능은 (대략) 작동하며, 버그가 있을 수 있습니다. "Playable"과 "perfect"는 매우 다른 것입니다. 지연(비동기화), 문제 및 버그가 예상됩니다.
* 제 테스트에는 모든 맵이 포함되어 있습니다. 가장 잘 작동하는 맵은 Factory와 Labs입니다. 성능은 서버 및 클라이언트의 CPU/인터넷 및 서버의 AI 수에 매우 의존합니다.
* HOSTING 및 COOP에 대한 자세한 정보는 [HOSTING.md 문서](https://github.com/paulov-t/SIT.Core/wiki/en/Guides/HOSTING-English.md)를 참고하세요.
* 호스트 및 서버는 안정적인 연결과 최소 5-10mbps의 업로드 속도가 필요합니다. AI는 실행에 많은 CPU 및 네트워크 대역폭을 사용합니다.
* 많은 사람들이 그렇게 말하지만, 세계 각국의 사람들과 함께 플레이할 수 있습니다(로컬 네트워크만 가능한 것은 아닙니다). 나는 200ms 이상의 핑을 가진 사람들과 함께 플레이했습니다. 그들은 라이브와 유사한 지연이 발생하지만 다른 방식으로 표시됩니다.

### 사전 요구사항
이 모듈을 사용하려면 서버에 [SPT-Aki 모드](https://github.com/paulov-t/SIT.Aki-Server-Mod)가 설치되어 있어야 합니다. Coop 모듈을 사용하지 않으려면 BepInEx 구성 파일에서 비활성화해야 합니다.

### Coop은 BSG의 Coop 코드를 사용할 수 있습니까?
아니요. BSG 서버 코드는 명백한 이유로 클라이언트에서 숨겨져 있습니다. 따라서 BSG의 Coop 구현은 PvPvE와 동일한 온라인 서버를 사용합니다. 우리는 이것을 보지 못하기 때문에 이를 사용할 수 없습니다.

### 코딩 설명
- 이 프로젝트는 BepInEx Harmony 패치의 여러 가지 방법과 Unity 구성 요소를 결합하여 목표를 달성합니다.
- 클라이언트->서버->클라이언트 간의 지속적인 폴링이 필요한 기능/방법(이동, 회전, 보기 등)은 데이터를 보내기 위해 구성 요소를 사용합니다(AI 코드는 업데이트/레이트 업데이트 명령 및 함수를 실행하고 모든 틱마다 실행되므로 네트워크 플러드를 유발합니다).
- "복제"할 수 있는 기능/방법은 ModuleReplicationPatch 추상 클래스를 사용하여 호출을 쉽게 왕복합니다.
- 모든 서버 통신은 JSON TCP Http 및 Web Socket 호출을 통해 이루어지며, [SPT-Aki에서 개발한 "Web Server"](https://dev.sp-tarkov.com/SPT-AKI/Server)를 사용하여 [typescript mod](https://github.com/paulov-t/SIT.Aki-Server-Mod)를 사용하여 "백엔드" 작업을 처리합니다.
- CoopGameComponent는 Coop 준비 게임이 시작될 때 GameWorld 개체에 연결됩니다(숨은 장소가 아닌 모든 게임). CoopGameComponent는 정보를 위해 서버를 폴링하고 데이터를 PlayerReplicatedComponent에 전달합니다.

## SPT-Aki

### Aki 모듈은 지원됩니까?
The following Aki Modules are supported.
- aki-core
- Aki.Common
- Aki.Reflection
- SPT-AKI 클라이언트 모드는 작성된 패치가 얼마나 잘 되어 있는지에 따라 작동합니다. 만약 그것들이 GCLASSXXX 또는 PUBLIC/PRIVATE를 직접 대상으로 한다면, 그것들은 실패할 가능성이 높습니다.

### 왜 Aki 모듈 DLL을 사용하지 않나요?
SPT-Aki DLL은 자체 Deobfuscation 기술을 위해 작성되었으며, 현재 내 기술은 Aki 모듈과 잘 작동하지 않습니다. 그래서 SPT-Aki의 많은 기능을 이 모듈로 이식했습니다. 최종 목표는 SPT-Aki에 의존하고 이 모듈은 SIT 전용 기능에만 집중하는 것입니다.

## 컴파일하는 방법
[컴파일 문서](COMPILE.md)

# BepInEx 설치 방법
[https://docs.bepinex.dev/articles/user_guide/installation/index.html](https://docs.bepinex.dev/articles/user_guide/installation/index.html)

## Tarkov에 설치하는 방법
먼저 BepInEx 5를 설치하고 구성해야 합니다 (BepInEx 설치 방법 참조). 빌드된 .dll 파일을 BepInEx 플러그인 폴더에 넣으십시오.

## Tarkov에서 테스트하는 방법
- Tarkov 폴더 내에서 BepInEx가 설치된 위치로 이동합니다.
- config 폴더를 엽니다.
- BepInEx.cfg 파일을 엽니다.
- 다음 설정 [Logging.Console]를 True로 변경합니다
- 구성 파일을 저장합니다.
- 런처나 다음과 같은 bat 파일을 통해 Tarkov를 실행합니다 (ID를 교체하십시오).
```
start ./Clients/EmuTarkov/EscapeFromTarkov.exe -token=pmc062158106353313252 -config={"BackendUrl":"http://127.0.0.1:6969","Version":"live"}
```
- BepInEx가 작동하면 콘솔이 열리고 "plugin" 모듈이 시작된 것으로 표시됩니다.


## 감사드리는 팀
- SPT-Aki team
- MTGA team
- SPT-Aki Modding Community
- DrakiaXYZ ([BigBrain](https://github.com/DrakiaXYZ/SPT-BigBrain))
- Dvize ([NoBushESP](https://github.com/dvize/NoBushESP))

## 라이선스 - [English](README.md)

- DrakiaXYZ 프로젝트는 MIT 라이선스를 포함합니다.
- SPT-Aki 팀이 원래 코어 및 싱글 플레이어 기능의 95%를 완료했습니다. 이들에 대한 라이선스가 이 소스 내에 있을 수 있습니다.
- 내 작업 중 어떤 것도 라이선스가 없습니다. 이것은 단순히 즐거움을 위한 프로젝트입니다. 당신이 이것으로 무엇을 하든 상관하지 않습니다.
