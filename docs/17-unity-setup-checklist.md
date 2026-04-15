# Unity 설치 및 프로젝트 생성 체크리스트

## 문서 목적

이 문서는 Unity를 처음 사용하는 사용자가 집에서 Unity 설치와 프로젝트 생성을 진행할 때 따라갈 수 있는 체크리스트다. 목표는 설치 과정에서 막히는 지점을 줄이고, 프로젝트 생성 직후 AI가 이어서 작업할 수 있는 상태를 만드는 것이다.

## 설치 전 준비

확인할 것:

- Unity 계정 생성 또는 로그인 가능 여부
- 충분한 디스크 여유 공간
- Git 사용 가능 여부
- 현재 저장소 위치 확인
- 인터넷 연결

권장:

- Unity Hub 설치
- Unity 6 LTS 계열 설치
- Windows Build Support 설치

Steam PC 출시를 목표로 하므로 Windows 빌드 지원은 우선 필요하다.

## Unity Hub 설치

1. Unity Hub를 설치한다.
2. Unity 계정으로 로그인한다.
3. 라이선스 관련 안내가 나오면 Personal 또는 사용 가능한 라이선스를 선택한다.
4. Installs 탭으로 이동한다.
5. Unity 6 LTS 계열 버전을 설치한다.

설치 모듈 권장:

- Windows Build Support
- Documentation
- Visual Studio 또는 C# IDE 연동 도구

Visual Studio Code를 사용할 수도 있지만, Unity 초보라면 Unity 설치 과정에서 제공하는 기본 C# IDE 연동을 우선 사용해도 된다.

## 프로젝트 생성

Unity Hub에서 새 프로젝트를 만든다.

권장 설정:

- Template: 3D URP
- Project name: `TinyFactoryPrototype`
- Location: 현재 저장소 안 또는 별도 위치 중 선택 필요

중요:

- 현재 작업 저장소는 `/workspaces/Conveyor-Dungeon`이다.
- 로컬 PC에서 작업할 때도 가능하면 같은 저장소 루트 안에 Unity 프로젝트를 만든다.
- 저장소 루트 바로 아래에 Unity 프로젝트 파일들이 생기게 할지, `unity/` 하위 폴더를 만들지는 결정이 필요하다.

현재 추천:

```text
Conveyor-Dungeon/
  docs/
  unity/
    TinyFactoryPrototype/
```

이 구조를 추천하는 이유:

- 문서와 Unity 프로젝트가 같은 저장소에 있으면서도 구분된다.
- Unity가 생성하는 파일이 저장소 루트를 너무 어지럽히지 않는다.
- 나중에 README, 빌드, 도구 문서를 루트에서 관리하기 쉽다.

## 프로젝트 생성 후 확인

Unity 프로젝트가 열리면 다음을 확인한다.

- Scene 뷰가 보이는가
- Game 뷰가 보이는가
- Hierarchy 창이 보이는가
- Inspector 창이 보이는가
- Project 창이 보이는가
- Console 창이 보이는가
- Play 버튼을 눌렀을 때 에러 없이 실행되는가

처음 실행 시 화면에 아무것도 없어도 괜찮다. Console에 빨간 에러가 없는지가 더 중요하다.

## Unity에서 꼭 알아야 할 최소 개념

### Scene

게임 화면 하나다. 첫 프로토타입에서는 `Prototype_01_Workshop` 씬 하나로 시작한다.

### GameObject

씬 안에 존재하는 모든 오브젝트다. 플레이어, 카메라, 조립대, 판매대가 모두 GameObject다.

### Component

GameObject에 붙는 기능이다. 예를 들어 이동 스크립트, Collider, Renderer가 Component다.

### Prefab

재사용 가능한 GameObject 템플릿이다. 부품, 제품, 조립대, 직원은 나중에 Prefab으로 만드는 것이 좋다.

### Inspector

선택한 오브젝트의 설정을 보는 창이다. 스크립트의 공개 필드 값도 여기서 바꿀 수 있다.

### Console

에러와 경고를 보는 창이다. 문제가 생기면 이 창의 내용을 AI에게 전달하면 된다.

## 첫 세팅 후 AI에게 알려줄 것

Unity 프로젝트를 생성한 뒤 다음 정보를 알려준다.

- 설치한 Unity 버전
- 프로젝트 위치
- 프로젝트 템플릿
- Play 버튼 실행 시 Console 에러 유무
- 에러가 있다면 에러 메시지 전체
- 프로젝트 폴더 구조 스크린샷 또는 설명

## 문제가 생겼을 때 전달할 정보

Unity 설치나 실행 중 문제가 생기면 다음을 그대로 전달한다.

- 어느 단계에서 막혔는지
- 화면에 나온 메시지
- Console 에러 전문
- 운영체제
- Unity Hub 버전
- Unity Editor 버전

에러 메시지는 일부만 요약하지 말고 가능한 전체를 전달하는 것이 좋다.

## 설치 후 다음 작업

설치가 끝나면 다음 순서로 진행한다.

1. Unity 프로젝트 위치를 확인한다.
2. Git 상태를 확인한다.
3. `docs/16-technical-design-brief.md` 기준으로 폴더 구조를 만든다.
4. `Prototype_01_Workshop` 씬을 만든다.
5. 플레이스홀더 오브젝트를 배치한다.
6. 첫 C# 스크립트 작업을 시작한다.

## 주의할 점

- Unity가 자동 생성한 폴더를 임의로 삭제하지 않는다.
- Console의 빨간 에러는 무시하지 않는다.
- 처음부터 에셋 스토어 패키지를 많이 설치하지 않는다.
- Steamworks 관련 패키지는 아직 설치하지 않는다.
- 저장, 업적, 클라우드 기능은 첫 프로토타입 이후로 미룬다.
