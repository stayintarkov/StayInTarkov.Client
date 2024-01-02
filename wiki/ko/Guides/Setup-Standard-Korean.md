# 설치 가이드 

* 이 가이드는 사용자가 ISP 라우터의 포트 포워딩 및 방화벽 설정에 액세스할 수 있으며 이를 변경하는 방법을 알고 있다고 가정합니다.
* 이 가이드는 이미 BattleState Games의 공식 Escape from Tarkov 버전이 설치되어 있다고 가정합니다.

## 호스트

1. [SPT Aki](https://www.sp-tarkov.com/) 를 다운로드하고 Aki.Server와 Aki_Data를 원하는 폴더에 압축 해제하세요.
2. [Install SIT Coop Mod](https://github.com/paulov-t/SIT.Aki-Server-Mod) 를 따라서 서버에 설치하고 올바르게 구성하세요.
3. 현재 보유하고 있는 오프라인 EFT의 모든 복사본을 삭제하세요.
4. [SIT-Launcher](https://github.com/stayintarkov/SIT.Launcher.Classic) [최신 릴리즈](https://github.com/stayintarkov/SIT.Launcher.Classic/releases) .zip 파일을 다운로드하고 원하는 위치에 압축을 해제하세요.
5. SIT.Launcher.exe를 실행하세요.
6. SIT-Launcher의 지침에 따라 라이브 EFT의 사본을 생성하고 최신 버전의 SIT를 자동으로 설치하세요.
7. SIT-Launcher가 SIT/어셈블리를 설치하도록 설정 탭을 확인하세요.
8. [Install SIT Coop Mod](https://github.com/stayintarkov/SIT.Aki-Server-Mod) 를 따라 작업하고 Aki 서버의 http.json을 네트워크 카드의 내부 IP로 설정하고 Coop Mod의 coopConfig.json을 [외부 IP](https://www.whatismyip.com/) 로 설정하세요.
9. 서버를 실행하고 친구들과 외부 IP 주소를 공유하세요.
10. 라우터의 포트 6969, 6970을 서버의 로컬 네트워크 카드의 IP 주소 (예: 192.1.2.3)로 포트 포워딩하세요.
11. 서버와 라우터의 방화벽을 6969, 6970 포트로 열어주세요.
12. 런처에 외부 IP와 포트를 입력하고 Launch 버튼을 클릭하여 연결을 테스트하세요.
13. 이제 잘 작동합니다!

## CLIENT

1. 현재 보유하고 있는 오프라인 EFT의 모든 복사본을 삭제하세요.
2. [SIT-Launcher](https://github.com/stayintarkov/SIT.Launcher.Classic) [최신 릴리즈](https://github.com/stayintarkov/SIT.Launcher.Classic/releases) .zip 파일을 다운로드하고 원하는 위치에 압축을 해제하세요.
3. SIT.Launcher.exe를 실행하세요.
4. SIT-Launcher의 지침에 따라 라이브 EFT의 사본을 생성하고 최신 버전의 SIT를 자동으로 설치하세요.
5. SIT-Launcher가 SIT/어셈블리를 설치하도록 설정 탭을 확인하세요.
6. 호스트가 제공한 IP와 포트에 연결하세요 (예: http://111.222.255.255:6969)
7. 다른 소스에서 가져온 사용자 이름과 비밀번호를 사용하지 마세요. 모든 비밀번호는 평문으로 저장됩니다!

