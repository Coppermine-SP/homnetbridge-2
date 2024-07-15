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

**환경 변수 또는 실행 인자로 아래의 옵션을 구성하십시오:**
|환경 변수|실행 인자|설명|
|---|---|---|
|SERIAL_CLIENT|--serial|LnCP 통신에 사용할 클래스를 지정합니다.|
|ETHERNET_CAPTURE|--ethernet|이더넷 캡쳐에 사용할 클래스를 지정합니다.|

**시리얼 클라이언트 옵션**
|환경 변수 값|실행 인자 값|설명|
|---|---|---|
|LOCAL|LocalSerialClient|LnCP 통신에 로컬 시리얼 장치를 사용합니다.|
|REMOTE|RemoteSerialClient|LnCP 통신에 TCP 서버를 사용합니다. (EW11)|
|NULL|NullSerialClient|LnCP 통신을 사용하지 않습니다.|

**이더넷 캡쳐 옵션**
|환경 변수 값|실행 인자 값|설명|
|---|---|---|
|LOCAL|LocalEthernetCapture|이더넷 캡쳐에 로컬 네트워크 인터페이스를 사용합니다.|
|NULL|NulLEthernetCapture|이더넷 캡쳐를 사용하지 않습니다.|

### Run as Docker Container
docker-compose.yml 파일의 서비스에 아래와 같이 추가하십시오:
```yml
  homnetbridge:
    build: "https://github.com/Coppermine-SP/homnetbridge-2.git#master:src"
    volumes:
      - ./volumes/homnetbridge/appsettings.json:/app/appsettings.json
      - ./volumes/homnetbridge/apps:/app/apps
    environment:
      - "SERIAL_CLIENT=LOCAL"
      - "ETHERNET_CAPTURE=LOCAL"
    privileged: true
    network_mode: host
    restart: always
```
반드시 컨테이너는 호스트 장치에 접근 할 수 있도록 특권 모드(Privileged mode)로 실행되어야 합니다. 또한 호스트 네트워크 인터페이스에 접근 할 수 있도록 network_mode를 host로 구성하십시오.

환경 변수를 통해 옵션을 구성하고, /app/appsettings.json과 /app/apps 디렉터리를 마운트하십시오.

---

### Run Directly
Self-Contained 옵션으로 프로젝트를 배포하고 실행하십시오.

또는 Framework-Independent 옵션으로 배포한 경우, 호스트에 .NET 8.0 Runtime을 설치하십시오.

> [!WARNING]
> **반드시 root 권한으로 실행되어야 합니다.**
> 
> 시리얼 장치와 네트워크 인터페이스 접근에 root 권한을 요구합니다.
---

### Configure appsettings.json
appsettings.json을 아래와 같이 구성하십시오. 사용하지 않는 클래스는 구성하지 않아도 됩니다.
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "[Information 또는 Debug]",
      "Microsoft": "Warning"
    }
  },

  "RemoteSerialClient": {
    "Hostname": "[EW11 서버 주소]",
    "Port": [EW11 서버 포트]
  },

  "LocalSerialClient":{
    "InterfaceName": "[시리얼 장치 이름]"
  },

  "EthernetCapture": {
    "CaptureInterface": "[캡쳐 할 네트워크 인터페이스]",
    "CaptureFilter": "not broadcast and not multicast and not icmp and not arp",
    "ReadTimeout": 1500,
    "PacketVerbose": false
  },

  "ElevatorControl" : {
    "ReferenceFloor": [기준 층],
    "NotifyThreshold": [엘리베이터가 몆개의 층을 이동할 때 마다 알림을 발송할지 지정] 
  },

  "HomeAssistant": {
    "Host": "[HA URL]",
    "Port": [HA Port],
    "Ssl": false,
    "Token": "[HA Long-Lived Access Token]"
  },

   "NetDaemon": {
        "ApplicationConfigurationFolder": "./apps"
  }
}

```
