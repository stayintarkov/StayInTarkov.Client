# 설치 가이드

## [SIT.Core](https://github.com/stayintarkov/StayInTarkov.Client) 를 [Hamachi](https://www.vpn.net/) (또는 유사한 프로그램)와 [SIT.Launcher](https://github.com/stayintarkov/SIT.Launcher.Classic) 를 사용하여 친구와 함께 사용하는 방법에 대한 간단한 가이드입니다.

### 시작하기 전에 읽어주세요!
이 작업을 수행하려면 [Escape From Tarkov](https://www.escapefromtarkov.com/) 를 구입하고 설치해야 합니다.

[Hamachi](https://www.vpn.net/) 또는 유사한 프로그램을 설치해야 합니다. 이 가이드에서는 Hamachi를 사용합니다.

이 가이드는 SIT.Core.Release-64와 SIT-Launcher.Release-71에서 테스트되었습니다.

이 가이드는 제가 이 모드를 작동시키는 방법이며, 일부 단계는 불필요할 수 있으며 모드가 업데이트됨에 따라 이 가이드도 빠르게 오래되어질 수 있습니다!

이 가이드는 호스트가 포트를 올바르게 포워딩한 경우에도 적용할 수 있으며, 이 경우 Hamachi 부분을 건너뛰고 호스트의 공용 IPv4를 사용하면 됩니다.

# 서버 설치 가이드
### 친구가 서버를 호스팅하는 경우에는 건너뛰세요.
1. 클라이언트 설치 가이드에서 설명한 것과 동일한 종류의 폴더 구조를 만드세요. 이번에는 "Server"라는 이름의 폴더를 생성하세요. SITCOOP/Server
2. 최신 [SPT-AKI Stable Release](https://dev.sp-tarkov.com/SPT-AKI/Stable-releases/releases) 를 다운로드하여 Server 폴더에 압축을 해제하세요.
3. Aki.Server.exe를 실행하고 "Happy playing"이라고 나온 후에 종료하세요.
4. [SIT.Aki server mod](https://github.com/stayintarkov/SIT.Aki-Server-Mod) 를 다운로드하여 AKI 서버에 다른 모드를 설치하는 것과 동일한 방법으로 설치하세요.
모드의 폴더 경로는 다음과 같아야 합니다:
C:\SITCOOP\Server\user\mods\SIT.Aki-Server-Mod-master\
SIT.Aki-Server-Mod-master 폴더 안에는 package.json 파일이 있어야 합니다.
5. Configure http.json and coopconfig.json as described in [호스팅 문서](./HOSTING-Korean.md)
에서 설명한 대로 http.json 및 coopconfig.json을 구성하세요. Hamachi를 사용하는 경우 http.json 및 coopconfig.json에서 Hamachi IPv4를 사용하세요.
*여전히 127.0.0.1 또는 Localhost를 사용하지 마세요!*
6. 서버를 시작하세요 (관리자 권한으로) 그리고 클라이언트 설치 단계로 이동하세요.

# 클라이언트로 SIT.Launcher 및 SIT.Core 설치하기.

1. 설치를 원하는 폴더로 이동하세요. 저는 "C:\SITCOOP" 폴더에 설치하기로 결정했습니다.
2. "SITCOOP" 폴더 내에 "Game" 및 "Launcher" 폴더를 생성하세요.
3. SIT.Launcher를 다운로드하여 "Launcher" 폴더에 압축을 해제하세요.
4. SIT.Launcher.exe를 실행하고 게임을 이전에 만든 "Game" 폴더로 설정하세요.
5. 설치가 완료되면 런처를 닫아야 합니다.
6. "Launcher" 폴더 내에 "AkiSupport"라는 폴더를 생성하세요.
7. "AkiSupport" 내에 다음과 같은 경로로 폴더를 생성하세요:\
C:\SPCOOP\Launcher\AkiSupport\Bepinex\Patchers\
C:\SPCOOP\Launcher\AkiSupport\Bepinex\Plugins\
C:\SPCOOP\Launcher\AkiSupport\Managed\
6. 친구가 서버를 호스팅하는 경우 Hamachi 클라이언트에서 해당 친구의 Hamachi IPv4를 복사하세요. 호스트인 경우 자신의 Hamachi IPv4를 복사하세요. **127.0.0.1은 작동하지 않습니다!**
7. 런처를 시작하고 서버 필드를 친구의 IP로 변경하세요.
예시: http://100.100.100.100:6969
8. 원하는 사용자 이름과 비밀번호를 입력하세요.

**사용자 이름과 비밀번호는 평문으로 호스트의 컴퓨터에 저장되므로 다른 곳에서 사용하는 사용자 이름/비밀번호를 사용하지 마세요. 또는 호스트가 볼 수 없는 사용자 이름/비밀번호를 사용하지 마세요!**

9. 런처 설정으로 이동하여 "SIT 자동 설치", "최신 SIT 강제 설치...", "AKi Support 자동 설치"가 활성화되어 있는지 확인하세요.

**축하합니다. 이제 SIT의 최신 설치가 완료되었습니다.**

**즐겨주세요**

ppyLEK에 의해 작성됨 *(SlejmUr에 의해 일부 수정됨)*