# HomNetBridge-2
<img src="https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=Docker&logoColor=white"> <img src="https://img.shields.io/badge/ASP.NET-512BD4?style=for-the-badge&logo=blazor&logoColor=white"> <img src="https://img.shields.io/badge/Home Assistant-18BCF2?style=for-the-badge&logo=homeassistant&logoColor=white">

> [!NOTE]
> 
> [cloudinteractive-homnetbridge](https://github.com/Coppermine-SP/cloudinteractive-homnetbridge), [cloudinteractive-homnetbridge-serial](https://github.com/Coppermine-SP/cloudinteractive-homnetbridge-serial) 프로젝트가 통합되었습니다.
<img src="/img/title.png">

**LG HomNet 스마트 홈 시스템을 Home Assistant에 통합하는 프로젝트입니다.**
- LnCP 송수신을 통한 연동 가전 제어 (TCP를 통한 EW11 연결 / 로컬 시리얼 장치)
- 월패드 이더넷 캡쳐를 통한 정보 수신 (tcpdump)

**Features**
- 차량 입차 알림
- 공동 현관문 출입 요청 알림
- 엘리베이터 호출 및 위치 알림
- 전등 제어
- 현관문 열림, 닫힘 감지
- HBM 로그 수신

### Table of Content
- [Overview](#overview)
- [Dependencies](#dependencies)
- [Configurations](#configurations)

## Overview
<img src="/img/diagram.png">

LG HomNet 시스템을 통합하기 위하여 단지 서버에서 HomNet 서버로 들어가는 이더넷 패킷을 캡쳐한 데이터와, HomNet 서버와 단말 장치 사이의 LnCP (RS-485) 통신을 사용합니다.

이 프로젝트는 Raspberry PI 환경에서 Docker Container로 구동 가능하게 설계되었습니다. 

또한 Home Assistant 서버와 통신하기 위하여 NetDaemon 4를 사용합니다.

>[!WARNING]
>**이더넷 네트워크에 대한 직접적인 접근은 수행하지 않습니다.**
>
>제 3자 장치를 통해 단지 내 서버에 직접적으로 접근하는 경우, 예기치 못한 오류 및 법적인 문제가 발생 할 수 있습니다.

아래 기능을 구현하기 위해 이더넷 캡쳐를 사용합니다:
- 입차 알림
- 공동 현관문 출입 요청 알림
- 엘리베이터 위치 알림
- 현관문 열림, 닫힘 감지
- HBM 로그 수신

아래 기능을 구현하기 위해 LnCP 통신을 사용합니다:
- 전등 제어
- 엘리베이터 호출

>[!NOTE]
>**LnCP 통신 데이터와 HBM 로그를 보려면, appsettings.json의 Default LogLevel을 Debug로 설정하십시오.**
## Dependencies
- **Microsoft.Extensions.Hosting** - 8.0.0
- **Microsoft.VisualStudio.Azure.Containers.Tools.Targets** - 1.20.1
- **NetDaemon.AppModel** - 24.27.0
- **NetDaemon.Client** - 24.27.0
- **NetDaemon.Extensions.Logging** - 24.27.0
- **NetDaemon.Extensions.Scheduling** - 24.27.0
- **NetDaemon.Extensions.Tls** - 24.27.0
- **NetDaemon.HassModel** - 24.27.0
- **NetDaemon.Runtime** - 24.27.0
- **Serilog.AspNetCore** - 8.0.1
- **SharpPcap** - 6.3.0
- **System.CommandLine.DragonFruit** - 0.4.0-alpha-22272.1
- **System.IO.Ports** - 9.0.0-preview.5.24306.7
- **System.Text.Encoding.CodePages** - 8.0.0

## Configurations
**Docker Container를 통해 구동하거나, .NET 8.0 Runtime을 설치하여 직접 구동 할 수 있습니다.**

> [!WARNING]
> **LocalEthernetCapture을 사용하는 경우, tcpdump가 필요합니다.**
> 
> 프로젝트의 Dockerfile를 사용하는 경우, 자동으로 해당 의존성을 설치합니다.
