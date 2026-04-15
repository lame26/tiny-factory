# 프로토타입 기술 설계 메모

## 문서 목적

이 문서는 첫 Unity 프로토타입을 만들기 전에 필요한 최소 기술 방향을 정리한다. 목표는 완성형 기술 설계가 아니라, Unity 초보 상태에서 프로젝트 구조, 씬 구성, 스크립트 책임이 흐트러지지 않도록 기준을 마련하는 것이다.

이 문서는 `docs/15-prototype-scope-definition.md`의 범위를 구현하기 위한 작업 메모다.

## 기술 스택

### 기본 선택

- 엔진: Unity
- 권장 버전: Unity 6 LTS 계열
- 언어: C#
- 렌더링: URP
- 표현 방식: 3D 탑다운 또는 아이소메트릭 2.5D
- 입력: 초기에는 기본 키보드 입력 또는 Unity Input System 중 단순한 쪽 선택
- UI: 초기에는 uGUI 권장
- 데이터: 초기에는 MonoBehaviour 필드와 ScriptableObject 혼합 가능
- 저장: 첫 프로토타입에서는 제외
- Steam 연동: 첫 프로토타입에서는 제외

### 선택 이유

- 프로토타입 속도가 중요하다.
- 화면에는 생산 공간, 오브젝트, 직원 이동이 보여야 한다.
- 초반에는 시스템 수가 적으므로 복잡한 아키텍처보다 읽기 쉬운 구조가 우선이다.
- 이후 확장을 고려해 스크립트 책임은 처음부터 과도하게 섞지 않는다.

## Unity 프로젝트 기본 정보

### 프로젝트명 후보

- `ConveyorDungeon`
- `TinyFactoryPrototype`
- `GadgetWorksPrototype`
- `FactoryShopPrototype`

현재 추천:

- Unity 프로젝트 폴더명: `TinyFactoryPrototype`
- 저장소명은 현재 `Conveyor-Dungeon`을 유지

Unity 프로젝트 폴더명은 작업용 이름이다. 현재 게임 가칭은 `Tiny Factory`지만, 출시명, 스팀 페이지명, 상표 검토 전까지는 최종 확정으로 보지 않는다.

## 첫 프로토타입 씬

### 씬 이름

- `Prototype_01_Workshop`

### 씬 목적

하나의 작은 전자제품 작업장에서 다음 루프를 검증한다.

1. 플레이어가 부품을 집는다.
2. 조립대에 부품을 넣는다.
3. 조립 시간이 지나 제품이 생성된다.
4. 제품을 판매대에 놓는다.
5. 돈이 증가한다.
6. 돈으로 업그레이드 또는 직원 구매를 한다.
7. 직원이 반복 작업 일부를 자동화한다.

## 추천 폴더 구조

Unity 프로젝트 생성 후 `Assets/` 아래에 다음 구조를 만든다.

```text
Assets/
  _Project/
    Art/
      Materials/
      Models/
      Placeholders/
    Audio/
    Data/
      Products/
      Upgrades/
    Prefabs/
      Characters/
      Stations/
      Items/
      UI/
    Scenes/
    Scripts/
      Core/
      Interaction/
      Items/
      Stations/
      Economy/
      Workers/
      UI/
    Settings/
```

원칙:

- 외부 에셋과 직접 만든 파일을 구분하기 위해 `_Project` 폴더를 쓴다.
- 첫 프로토타입에서는 폴더가 조금 비어 있어도 괜찮다.
- 스크립트는 기능별로 나누되, 과도한 계층화는 피한다.

## 주요 GameObject 구성

### Scene Root

권장 루트 오브젝트:

- `GameManager`
- `Player`
- `CameraRig`
- `Workshop`
- `Stations`
- `Items`
- `Workers`
- `UI`

### GameManager

역할:

- 게임 상태 관리
- 돈 관리 연결
- 업그레이드 구매 연결
- 전체 참조 연결

초기에는 하나의 `GameManager`가 많은 것을 중재해도 된다. 다만 실제 생산 로직이나 UI 표시 로직을 전부 넣지는 않는다.

### Player

역할:

- 이동
- 상호작용 입력
- 현재 들고 있는 아이템 관리
- 가까운 상호작용 대상 감지

필요 컴포넌트 후보:

- `PlayerController`
- `PlayerInteractor`
- `CarryHolder`
- `CharacterController` 또는 `Rigidbody`

초기 추천:

- 3D 탑다운에서는 `CharacterController` 기반 이동이 단순하다.

### Stations

포함 오브젝트:

- `PartBin`
- `AssemblyBench`
- `SellCounter`
- `UpgradeTerminal`
- `WorkerHireTerminal`

역할:

- `PartBin`: 부품 제공
- `AssemblyBench`: 부품을 받아 일정 시간 후 제품 생성
- `SellCounter`: 제품을 받아 돈 지급
- `UpgradeTerminal`: 업그레이드 구매
- `WorkerHireTerminal`: 직원 구매

### Items

아이템 후보:

- `PartItem`
- `ProductItem`

초기에는 실제 모델 대신 색이 다른 큐브나 단순 프리미티브를 사용한다.

### Workers

초기 직원:

- `Worker_Transporter`

역할:

- 부품 보관대에서 부품을 가져와 조립대에 넣는다.
- 조립대가 비어 있을 때만 움직인다.
- 제품 판매 자동화는 첫 버전에서는 제외한다.

## 주요 스크립트 설계

### Core

`GameManager`

- 씬의 핵심 시스템을 연결한다.
- 초기화 순서를 관리한다.
- 너무 많은 세부 로직을 직접 갖지 않도록 주의한다.

`MoneyManager`

- 현재 돈을 저장한다.
- 돈 증가, 지출 가능 여부, 지출 처리를 담당한다.
- UI에 변경 이벤트를 전달한다.

### Interaction

`IInteractable`

- 상호작용 가능한 오브젝트의 공통 인터페이스.
- 예: `Interact(PlayerInteractor interactor)`

`PlayerInteractor`

- 플레이어 주변의 상호작용 대상을 찾는다.
- 입력이 들어오면 가장 가까운 대상과 상호작용한다.

### Items

`Item`

- 부품과 제품의 공통 기반.
- 아이템 타입 정보를 가진다.

`CarryHolder`

- 플레이어 또는 직원이 현재 들고 있는 아이템을 관리한다.
- 한 번에 하나만 들 수 있다.

### Stations

`PartBin`

- 상호작용 시 부품을 제공한다.
- 초기에는 무한 제공한다.

`AssemblyBench`

- 부품을 받는다.
- 조립 시간을 진행한다.
- 완료 후 제품을 생성한다.
- 이미 작업 중이면 추가 투입을 받지 않는다.

`SellCounter`

- 제품을 받는다.
- 판매가만큼 돈을 지급한다.

### Economy

`UpgradeManager`

- 업그레이드 구매 가능 여부를 확인한다.
- 조립 속도 또는 판매가 업그레이드를 적용한다.

초기 업그레이드:

- 조립 속도 업그레이드
- 판매가 업그레이드

### Workers

`WorkerController`

- 직원 상태를 관리한다.
- 목표 지점으로 이동한다.
- 부품 픽업과 조립대 투입을 반복한다.

초기 상태 후보:

- `Idle`
- `MoveToPartBin`
- `PickPart`
- `MoveToAssemblyBench`
- `DropPart`

### UI

`HudController`

- 현재 돈
- 들고 있는 아이템
- 조립대 상태
- 업그레이드 비용
- 직원 구매 가능 여부

## 데이터 처리 방식

첫 프로토타입에서는 데이터 구조를 과하게 만들지 않는다.

초기 방식:

- 제품 판매가, 조립 시간, 업그레이드 비용은 Inspector에서 조정 가능한 필드로 둔다.
- 제품/부품이 1종뿐이므로 ScriptableObject는 필수로 만들지 않는다.

두 번째 프로토타입부터 검토:

- `ProductDefinition`
- `StationDefinition`
- `UpgradeDefinition`
- JSON 또는 CSV 기반 밸런스 테이블

## 입력 방식

초기 입력:

- 이동: WASD
- 상호작용: E
- 카메라: 고정
- 마우스: UI 클릭에만 사용

선택 이유:

- Unity 초보가 테스트하기 쉽다.
- 클릭 이동보다 구현과 디버깅이 단순하다.
- 조작 피로도는 첫 테스트 후 판단한다.

## 카메라 방향

초기 추천:

- 3D 탑다운에 가까운 고정 카메라
- 약간 기울어진 아이소메트릭 느낌

예시:

- 카메라 위치: 작업장 중심 위쪽과 뒤쪽
- 카메라 회전: X축 45~60도
- 플레이어와 스테이션이 모두 보이는 거리

## 아트와 플레이스홀더

첫 프로토타입에서는 다음처럼 단순화한다.

- 플레이어: 캡슐
- 직원: 다른 색 캡슐
- 부품: 작은 파란 큐브
- 제품: 작은 초록 큐브
- 부품 보관대: 파란 박스
- 조립대: 회색 테이블
- 판매대: 초록 카운터
- 업그레이드 지점: 노란 패널

목표는 예쁘게 보이는 것이 아니라, 역할이 즉시 구분되는 것이다.

## 저장과 Steam 연동

첫 프로토타입에서는 제외한다.

제외 이유:

- 5~10분 루프 검증에는 필요하지 않다.
- 저장과 Steam 연동은 데이터 구조가 안정된 뒤 넣는 것이 낫다.
- Unity 초보 단계에서 초기 복잡도를 낮춘다.

## 구현 우선순위

1. Unity 프로젝트 생성
2. 폴더 구조 생성
3. 씬 생성
4. 플레이스홀더 오브젝트 배치
5. 플레이어 이동
6. 상호작용 감지
7. 부품 집기
8. 조립대 투입
9. 제품 생성
10. 판매대 판매
11. 돈 UI
12. 업그레이드
13. 직원 구매
14. 직원 자동 운반
15. 테스트 플레이와 수치 조정

## 구현 중 유지할 원칙

- 첫 버전은 돌아가는 루프가 우선이다.
- 완벽한 아키텍처보다 수정 가능한 단순 구조가 낫다.
- 하나의 스크립트가 너무 많은 역할을 갖기 시작하면 분리한다.
- 수치는 Inspector에서 쉽게 바꿀 수 있게 둔다.
- 사용자에게 Unity 에디터 조작이 필요한 경우 단계별로 안내한다.

## 구현 전 확인할 사항

구현에 들어가기 전 사용자에게 확인할 수 있는 항목:

- Unity 설치 가능 여부
- Unity 버전
- 프로젝트 폴더명
- 첫 제품 표현: 스마트폰, 이어폰, 보조배터리, 임시 큐브 중 선택

다만 첫 기술 프로토타입에서는 `Basic Gadget` 임시 큐브로 시작해도 충분하다.
