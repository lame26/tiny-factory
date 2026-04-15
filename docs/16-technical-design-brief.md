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
- 표현 방식: 위에서 수직으로 내려다보는 3D 탑다운 고정 뷰
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

1. 작업자가 부품을 집는다.
2. 작업자가 조립대에 부품을 넣는다.
3. 조립 시간이 지나 제품이 생성된다.
4. 작업자가 제품을 판매대 또는 출고대에 놓는다.
5. 돈이 증가한다.
6. 돈으로 업그레이드 또는 직원 구매를 한다.
7. 추가 직원 또는 설비가 반복 작업 처리량을 높인다.

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
- `Operator`
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

### Operator

역할:

- 마우스 클릭 또는 UI 입력
- 스테이션 선택
- 업그레이드 구매
- 작업자/설비 구매
- 현재 운영 상태 확인

필요 컴포넌트 후보:

- `OperatorController`
- `StationSelectionController`

초기 추천:

- 직접 이동 캐릭터는 만들지 않는다.
- 수직 탑다운 카메라에서 스테이션 클릭과 UI 버튼으로 운영한다.

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

초기 작업자:

- `PlayerWorker_01` 또는 현재 프로토타입의 `Worker_01`

역할:

- 부품 보관대에서 부품을 가져와 조립대에 넣는다.
- 조립 완료 제품을 판매대 또는 출고대로 옮긴다.
- 조립대가 비어 있거나 완성품이 있을 때 우선순위에 따라 움직인다.
- 첫 작업자는 스테이지 시작 고유 작업자로 간주한다.

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

`ISelectable`

- 클릭 또는 UI에서 선택 가능한 오브젝트의 공통 인터페이스.
- 예: `Select(OperatorController operatorController)`

`StationSelectionController`

- 마우스 클릭 또는 UI 입력으로 스테이션을 선택한다.
- 선택된 스테이션의 상태와 구매 가능한 업그레이드를 HUD에 전달한다.

### Items

`Item`

- 부품과 제품의 공통 기반.
- 아이템 타입 정보를 가진다.

`CarryHolder`

- 작업자 또는 자동화 설비가 현재 들고 있는 아이템을 관리한다.
- 한 번에 하나만 들 수 있다.

`EquipmentBonusSource`

- 장기 메타 시스템에서 장비 효과를 전달할 수 있는 확장 지점.
- 첫 프로토타입에서는 구현하지 않는다.
- 작업자 속도와 운반량은 나중에 장비 보너스를 더하기 쉬운 필드로 둔다.

### Stations

`PartBin`

- 작업자가 도착하면 부품을 제공한다.
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

`ProductProgressionManager`

- 현재 제품 이름과 제품 레벨을 관리한다.
- 제품 레벨에 따른 픽업/출고 가치를 계산한다.
- 제품 레벨업 비용을 계산하고 구매를 처리한다.
- `PickupCounter`와 HUD에 현재 제품 가치와 다음 비용을 제공한다.

`UpgradeManager`

- 업그레이드 구매 가능 여부를 확인한다.
- 조립 속도, 스테이션 레벨 같은 보조 업그레이드를 적용한다.

초기 업그레이드:

- 제품 레벨업
- 조립 속도 업그레이드
- 작업자 처리량 업그레이드

### Workers

`WorkerController`

- 직원 상태를 관리한다.
- 목표 지점으로 이동한다.
- 부품 픽업, 조립대 투입, 완성품 회수, 판매대 전달을 반복한다.

초기 상태 후보:

- `Idle`
- `MoveToPartBin`
- `PickPart`
- `MoveToAssemblyBench`
- `DropPart`
- `WaitForProduct`
- `PickProduct`
- `MoveToSellCounter`
- `SellProduct`

### UI

`HudController`

- 현재 돈
- 선택한 스테이션
- 작업자 상태
- 조립대 상태
- 업그레이드 비용
- 직원 구매 가능 여부

## 데이터 처리 방식

첫 프로토타입에서는 데이터 구조를 과하게 만들지 않는다.

초기 방식:

- 제품 레벨, 제품 판매가, 조립 시간, 업그레이드 비용은 Inspector에서 조정 가능한 필드로 둔다.
- 제품/부품이 1종뿐이므로 ScriptableObject는 필수로 만들지 않는다.

두 번째 프로토타입부터 검토:

- `ProductDefinition`
- `StationDefinition`
- `UpgradeDefinition`
- JSON 또는 CSV 기반 밸런스 테이블

## 입력 방식

초기 입력:

- 이동: 없음
- 상호작용: 스테이션 클릭 또는 UI 버튼
- 카메라: 고정
- 마우스: 스테이션 선택, 업그레이드 구매, 직원 구매

선택 이유:

- Eatventure식 핵심은 직접 이동이 아니라 자동 작업과 투자 판단이다.
- 초기 구현은 작업자 상태 머신과 스테이션 경제를 먼저 검증해야 한다.
- 조작 피로도보다 생산 흐름 가독성과 업그레이드 체감이 우선이다.

## 카메라 방향

초기 추천:

- 위에서 수직으로 내려다보는 3D 탑다운 고정 카메라
- 작업자와 스테이션의 위치 관계를 가장 중요하게 본다.
- 캐릭터 앞뒤 구분보다 생산 흐름과 병목 가독성을 우선한다.

예시:

- 카메라 위치: 작업장 중심 위쪽
- 카메라 회전: X축 90도에 가까운 수직 하향
- 필요하면 Orthographic 카메라를 우선 검토
- 작업자와 스테이션이 모두 보이는 거리

## 아트와 플레이스홀더

첫 프로토타입에서는 다음처럼 단순화한다.

- 운영자 아바타: 없음 또는 UI 초상화
- 플레이어 고유 작업자: 캡슐 또는 명확히 구분되는 플레이스홀더
- 추가 작업자: 고유 작업자와 색이나 형태가 다른 캡슐
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
5. 스테이션 배치
6. 작업자 자동 이동
7. 작업자 부품 픽업
8. 조립대 투입
9. 제품 생성
10. 작업자 제품 회수와 판매대 전달
11. 돈 UI
12. 스테이션 선택과 레벨업
13. 업그레이드
14. 직원 구매 또는 자동화 설비 구매
15. 테스트 플레이와 수치 조정

장비, 보상 상자, 조합은 첫 프로토타입 이후 `docs/12-meta-progression-design.md`를 기준으로 별도 구현한다.

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
