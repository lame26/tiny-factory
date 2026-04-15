# Tiny Factory

`Tiny Factory`는 Eatventure류의 직관적인 운영 성장 문법을 전자/가전제품 생산, 조립, 판매, 자동화로 재해석하는 Steam PC용 게임 프로젝트다.

이 README는 새 세션의 단일 진입점이다. 최신 상태, 절대 헷갈리면 안 되는 방향, 다음 작업, 문서 역할만 빠르게 확인한다. 세부 설계는 각 전담 문서로 분리한다.

## 현재 상태

- 현재 날짜 기준 작업일: 2026-04-15
- 저장소 경로: `E:\tiny-factory-main`
- Unity 프로젝트 경로: `E:\tiny-factory-main\unity\TinyFactoryPrototype`
- Unity 버전: `6000.3.6f1`
- 렌더링: URP
- Unity MCP 서버: `unityMCP`
- Unity MCP URL: `http://localhost:8080/mcp`
- 현재 활성 기준 씬: `Assets/_Project/Scenes/Prototype_01_Workshop.unity`
- 마일스톤 0: 완료
- 마일스톤 1: 완료
- 마일스톤 2: 완료
- 마일스톤 3: 완료
- 마일스톤 4: 완료
- 마일스톤 5: 완료
- 마일스톤 6: 완료
- 마일스톤 7: 완료
- 마일스톤 7A: 완료
- 마일스톤 8: 완료
- 마일스톤 9: 완료
- 다음 작업: 마일스톤 10, 제품 레벨업과 단가 성장

## Unity MCP 재연결 절차

컴퓨터를 껐다 켜거나 새 Codex 세션을 시작했을 때 Unity MCP 연결이 안 되어 있으면 이 순서로 확인한다. 이 프로젝트는 이미 MCP 패키지와 자동 시작 스크립트가 들어가 있으므로, 대부분은 Unity 프로젝트를 열고 Codex 새 세션을 시작하면 된다.

### 정상 기준

- Unity 프로젝트: `E:\tiny-factory-main\unity\TinyFactoryPrototype`
- Unity Editor: `6000.3.6f1`
- Unity MCP 서버 이름: `unityMCP`
- Unity MCP URL: `http://localhost:8080/mcp`
- 실제 리스닝 주소: `127.0.0.1:8080`
- 자동 시작 스크립트: `unity/TinyFactoryPrototype/Assets/_Project/Editor/TinyFactoryMcpAutoStart.cs`
- Unity MCP 패키지: `com.coplaydev.unity-mcp`
- `uvx` 경로: `C:\Users\lame2\AppData\Local\Microsoft\WinGet\Packages\astral-sh.uv_Microsoft.Winget.Source_8wekyb3d8bbwe\uvx.exe`

### 빠른 재연결

1. Unity Hub 또는 직접 실행으로 프로젝트를 연다.

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe" -projectPath "E:\tiny-factory-main\unity\TinyFactoryPrototype"
```

2. Unity가 완전히 열린 뒤 8080 포트가 떠 있는지 확인한다.

```powershell
netstat -ano -p tcp | Select-String ':8080'
```

정상이면 대략 이런 줄이 보인다.

```text
TCP    127.0.0.1:8080    0.0.0.0:0    LISTENING    <pid>
```

3. Codex MCP 등록 상태를 확인한다.

```powershell
codex.cmd mcp list
codex.cmd mcp get unityMCP
```

정상 기준:

```text
unityMCP  http://localhost:8080/mcp  enabled
```

4. 새 Codex 세션을 `E:\tiny-factory-main`에서 시작한다. Codex는 세션 시작 시 MCP 설정을 읽으므로, MCP를 방금 등록했거나 수정했다면 기존 세션이 아니라 새 세션이 필요하다.

새 세션 첫 요청 예시:

```text
README.md 읽고 Unity MCP 연결 확인한 다음 마일스톤 10부터 진행해줘.
```

### 8080 포트가 안 뜰 때

1. Unity Editor가 프로젝트를 완전히 열었는지 확인한다. 첫 실행이나 패키지 갱신 직후에는 `Importing` 상태가 오래 갈 수 있다.
2. Unity 메뉴에서 `Window > MCP For Unity`를 열고 서버 상태를 확인한다.
3. 필요하면 `Start Server`를 누른다.
4. 그래도 안 뜨면 Unity Console에서 `[TinyFactory] MCP for Unity` 로그 또는 경고를 확인한다.

이 프로젝트에는 자동 시작 스크립트가 있어서 원칙적으로 직접 `Start Server`를 누르지 않아도 된다. 단, `uvx` 설치 경로가 바뀌었거나 WinGet 패키지 위치가 달라지면 자동 시작이 실패할 수 있다. 그 경우 `TinyFactoryMcpAutoStart.cs`의 `UvxPath` 값을 실제 `uvx.exe` 위치로 고친다.

### Codex에 unityMCP가 없을 때

다시 등록한다.

```powershell
codex.cmd mcp add unityMCP --url http://localhost:8080/mcp
```

등록 후에는 Codex 새 세션을 시작한다.

### uvx가 없을 때

확인:

```powershell
where.exe uvx
```

없으면 설치:

```powershell
winget install --id astral-sh.uv -e --accept-source-agreements --accept-package-agreements
```

설치 후 새 터미널에서 확인한다.

```powershell
uvx --version
```

### 예전에 오래 걸렸던 원인

- Unity URP 템플릿의 `com.unity.inputsystem` `1.12.0`이 Unity `6000.3.6f1`에서 `BuildTarget.ReservedCFE` 컴파일 에러를 냈다.
- 해결: `Packages/manifest.json`에서 `com.unity.inputsystem`을 `1.17.0`으로 올렸다.
- Unity MCP 패키지가 `Texture2D.EncodeToPNG()`를 쓰는데 `Image Conversion` 내장 모듈 참조가 없어 컴파일 에러가 났다.
- 해결: `Packages/manifest.json`에 `com.unity.modules.imageconversion: 1.0.0`을 추가했다.
- Codex MCP는 세션 시작 시 설정을 읽기 때문에, 서버를 등록한 직후 같은 세션에서는 Unity 도구가 안 보일 수 있다.
- 해결: `codex.cmd mcp list`로 등록 확인 후 Codex 새 세션을 시작한다.

## 현재 확정 방향

- 플레이어는 WASD로 캐릭터를 직접 움직이지 않는다.
- 플레이어는 공장 운영자이며, 입력은 스테이션 클릭과 UI 버튼 중심이다.
- 작업자와 스테이션이 자동으로 부품 운반, 조립, 판매를 수행한다.
- 시점은 위에서 수직으로 내려다보는 고정 탑다운 뷰를 기준으로 한다.
- 스테이지별 시작 첫 작업자는 플레이어 고유 작업자다.
- 스테이지 클리어 등 보상으로 상자를 열고, 나온 장비 아이템을 플레이어 고유 작업자에게 장착하는 메타 성장을 구현할 예정이다.
- 장비 부위와 장비 성장 구조는 Eatventure를 참고하되, 명칭, 외형, 수치, 과금 구조는 그대로 복제하지 않는다.
- 장비 조합 또는 합성 기능도 장기 메타 시스템으로 계획한다.

## 현재 프로토타입 목표

첫 프로토타입은 최종 게임의 축소판이 아니라 자동 생산과 운영 업그레이드가 재미있는지 확인하는 검증용이다.

현재 핵심 루프:

```text
작업자가 부품대에서 부품을 가져감
→ 작업자가 조립대에 부품을 투입
→ 조립대가 제품을 생산
→ 작업자가 제품을 판매대/출고대로 운반
→ 돈 증가
→ 플레이어가 스테이션 레벨업 또는 업그레이드 구매
→ 추가 직원 또는 자동화 설비 구매
→ 처리량 증가
```

첫 제품은 아직 최종 확정하지 않았다. 현재 후보는 보조배터리, 스마트폰, 임시 `Basic Gadget`이다.

## 완료된 작업

- Unity Editor `6000.3.6f1` 설치 확인
- `unity/TinyFactoryPrototype/`에 Unity 6 URP 프로젝트 생성
- Unity MCP 연결과 자동 시작 확인
- `Assets/_Project/` 폴더 구조 생성
- `Prototype_01_Workshop` 씬 생성
- 기본 루트 오브젝트, 카메라, 조명, 작업장 바닥, 경계벽, 중앙 작업 구역 배치
- `PartBin`, `AssemblyBench`, `SellCounter` 스테이션 플레이스홀더 배치
- `Worker_01` 작업자 캡슐 생성
- `WorkerController` 작성 및 `PartBin -> AssemblyBench -> SellCounter` 순회 연결
- Play 모드에서 작업자 순회 이동 확인
- Console 에러 0건 확인
- 검증 스크린샷:
  - `unity/TinyFactoryPrototype/Assets/Screenshots/Prototype_01_Workshop_M1.png`
  - `unity/TinyFactoryPrototype/Assets/Screenshots/Prototype_01_Workshop_M2.png`
- 마일스톤 3 아이템 운반과 스테이션 처리 골격을 구현했다.
- `ISelectable`, `StationSelectionController`, `StationSelectable`, `Item`, `CarryHolder`를 추가했다.
- `Operator` 루트에 스테이션 클릭 선택 컨트롤러를 붙였다.
- `PartBin`, `AssemblyBench`, `SellCounter`에 선택 컴포넌트와 클릭용 콜라이더를 붙였다.
- `Worker_01`을 `PlayerWorker_01`로 정리하고 `CarryHolder`와 `CarryHoldPoint`를 추가했다.
- 부품/제품 플레이스홀더 `PartItem_Placeholder`, `ProductItem_Placeholder`를 씬에 배치했다.
- 검증 스크린샷:
  - `unity/TinyFactoryPrototype/Assets/Screenshots/Prototype_01_Workshop_M3.png`
- 마일스톤 4 생산 루프를 구현했다.
- `PartBin`, `AssemblyBench`, `SellCounter`, `MoneyManager`를 추가했다.
- `WorkerController`를 부품 픽업, 조립대 투입, 제품 대기/회수, 판매대 전달 상태 머신으로 바꿨다.
- Play 모드에서 첫 판매 후 돈이 `5`로 증가하고 판매 수량이 `1`로 증가하는 것을 확인했다.
- 검증 스크린샷:
  - `unity/TinyFactoryPrototype/Assets/Screenshots/Prototype_01_Workshop_M4.png`
- 마일스톤 5 UI와 업그레이드를 구현했다.
- `HudController`를 추가해 돈, 선택 스테이션, 선택 상태, 조립대 상태, 업그레이드 버튼, 비용 부족 메시지를 표시한다.
- `UpgradeManager`를 추가해 선택 스테이션 레벨업, 조립 속도 업그레이드, 판매가 업그레이드를 처리한다.
- 기존 `MoneyManager`와 `StationSelectionController`의 디버그 패널은 새 HUD와 중복되지 않도록 껐다.
- Play 모드에서 비용 부족 메시지, 스테이션 레벨업, 조립 속도 감소, 판매가 증가, 업그레이드 후 판매 금액 증가를 확인했다.
- 검증 스크린샷:
  - `unity/TinyFactoryPrototype/Assets/Screenshots/Prototype_01_Workshop_M5.png`
- Eatventure식 비용 스케일링 보강을 적용했다.
- `MoneyFormatter`와 `UpgradeCostCalculator`를 추가해 돈/비용을 `K`, `M`, `B`, `T` 단위로 표시하고, 업그레이드 비용을 레벨 기반 성장 커브로 계산한다.
- 업그레이드 비용은 낮은 비용에서 시작하되 레벨이 쌓이면 `K`, `M` 단위까지 증가한다.
- Play 모드에서 `1.2K`, `2.5M` 표시, 레벨업 비용 `2.6K`, `504K`, `299M`, 비용 부족 메시지 `Not enough money. Need 299M.`을 확인했다.
- 검증 스크린샷:
  - `unity/TinyFactoryPrototype/Assets/Screenshots/Prototype_01_Workshop_CostCurve.png`
- 마일스톤 6 확장 자동화와 보조 시스템 슬롯을 구현했다.
- `WorkerManager`를 추가해 돈을 내고 추가 작업자를 구매할 수 있게 했다.
- HUD에 `Hire Worker`, `Worker Throughput` 버튼과 작업자 수/속도 표시를 추가했다.
- `WorkerController`를 여러 작업자가 같은 조립대를 공유해도 제품 회수와 부품 투입이 막히지 않도록 보강했다.
- `SupportBonusSlots`를 추가해 팁/보너스 수익, 임시 헬퍼, 장비 이동 속도/판매가 보너스 확장 지점을 만들었다.
- Play 모드에서 작업자 3명, 작업자 속도 `1.90`, 판매 3회 진행, Console 에러 0건을 확인했다.
- 검증 스크린샷:
  - `unity/TinyFactoryPrototype/Assets/Screenshots/Prototype_01_Workshop_M6.png`
- 마일스톤 7 1차 플레이테스트를 진행했다.
- 5분 상당 자동 테스트 2회를 실행했다.
- 첫 판매는 `8.8초`, 첫 업그레이드는 `22.8초`, 직원 구매 우선 플레이의 첫 직원 구매는 `158.8초`로 측정됐다.
- 싼 업그레이드를 먼저 누르는 플레이에서는 5분 안에 직원 구매까지 도달하지 못했다.
- 결론: 기술 루프는 안정적이지만, 초반 목표 설계와 판매 보상 피드백이 약하다.
- 플레이테스트 로그:
  - `docs/23-playtest-2026-04-15-m7.md`
- 검증 스크린샷:
  - `unity/TinyFactoryPrototype/Assets/Screenshots/Prototype_01_Workshop_M7_Playtest.png`
- 사용자의 지적에 따라 Eatventure의 테이블/서버 구조를 그대로 쓰지 않고, 전자제품에 맞는 주문/픽업 구조로 전환했다.
- `CustomerOrderCounter`는 고객 주문을 생성하고, 작업자는 주문이 있을 때만 부품을 가져가 제작한다.
- 기존 `SellCounter`는 `PickupCounter`로 재해석했으며, 완성품 픽업/출고가 완료될 때 돈이 증가한다.
- Play 모드에서 첫 주문 처리 완료가 `7.6초`에 발생하고, `Pickup Counter` 상태가 `Done: 1`, 돈이 `5`로 증가하는 것을 확인했다.
- 검증 스크린샷:
  - `unity/TinyFactoryPrototype/Assets/Screenshots/Prototype_01_Workshop_OrderPickup.png`
- 마일스톤 7A 1차 튜닝 패스를 완료했다.
- 첫 직원 구매 기본 비용을 `60`에서 `40`으로 낮추고, 재테스트에서 첫 직원 구매가 `114.6초`에 발생하는 것을 확인했다.
- HUD에 초반 추천 목표와 진행 바를 추가하고, 첫 직원 구매 전 `Hire Worker - Goal` 라벨을 표시한다.
- 픽업/출고 완료 시 `+$` 플로팅 피드백을 표시한다.
- 부품/제품 런타임 아이템과 씬 플레이스홀더를 회로기판/보조배터리 형태로 개선했다.
- 플레이테스트 로그:
  - `docs/24-playtest-2026-04-15-m7a.md`
- 검증 스크린샷:
  - `unity/TinyFactoryPrototype/Assets/Screenshots/Prototype_01_Workshop_M7A_Tuning.png`
- 마일스톤 8 장기 메타 시스템 후보를 구체화했다.
- 첫 장비 슬롯은 `Head`, `Body`, `Tool` 3부위로 시작한다.
- 보상 상자는 스테이지 클리어와 생산 목표 달성 보상으로 지급하는 방향으로 정리했다.
- 첫 스테이지 클리어 조건 초안은 `픽업/출고 완료 30회 + 직원 2명 보유`, 보상은 `Basic Box 1개`다.
- 장비 장착 UI는 고유 작업자 중심의 `Worker Gear` 화면으로 시작한다.
- 장비 조합은 같은 장비 3개를 같은 장비 레벨 +1로 합치는 단순 규칙부터 시작한다.
- 마일스톤 9 2차 자동화 확장 후보를 구현했다.
- 자동 설비 대신 두 번째 조립대를 먼저 선택했다.
- `AssemblyBench_02`를 잠긴 설비로 씬에 배치하고, `FacilityManager`와 HUD `Build Assembly Bench` 버튼으로 구매할 수 있게 했다.
- `WorkerController`와 `WorkerManager`를 보강해 여러 조립대 중 완성품이 있거나 비어 있는 곳을 선택하게 했다.
- 재테스트에서 첫 직원 구매 `115.2초`, 두 번째 조립대 구매 `216.0초`, 두 번째 조립대 구매 후 첫 픽업/출고 완료 `229.2초`를 확인했다.
- 5분 후 픽업/출고 완료 수는 `25회`로 마일스톤 7A와 같아, 다음 병목은 주문 생성/작업자 동선/픽업 우선순위로 판단했다.
- 사용자 지적에 따라 마일스톤 10의 핵심 병목을 제품 레벨업/단가 성장 부재로 재정의했다.
- `docs/10-economy-and-progression-design.md`를 추가해 제품 레벨업, 출고 가치 성장, 추천 목표 순서, 구현 구조를 먼저 정리했다.
- 플레이테스트 로그:
  - `docs/25-playtest-2026-04-15-m9.md`
- 검증 스크린샷:
  - `unity/TinyFactoryPrototype/Assets/Screenshots/Prototype_01_Workshop_M9_SecondBench_Tuned.png`

## 다음 작업

`docs/18-prototype-task-board.md` 기준 마일스톤 10부터 진행한다.

1. `ProductProgressionManager` 구현
2. `PickupCounter`를 제품 레벨 기반 출고 가치에 연결
3. HUD에 `Level Power Bank` 버튼 추가
4. 추천 목표를 첫 직원 → 제품 Lv 2~3 → 두 번째 조립대 순서로 변경
5. 제품 레벨업 적용 후 5분 재테스트

마일스톤 10에서는 생산량 확장 전에 제품 단가가 같이 성장해야 자동화 구매 시간이 압축되는지 확인한다.

## 문서 역할

- `README.md`: 새 세션 진입점. 최신 상태와 다음 작업만 짧게 유지한다.
- `docs/05-design-document-roadmap.md`: 전체 문서 역할과 중복 관리 규칙.
- `docs/22-decision-log.md`: 현재 확정 결정과 보류 사항의 단일 요약표.
- `docs/work-history.md`: 시간순 작업 기록. 왜 바뀌었는지 추적할 때 사용한다.
- `docs/18-prototype-task-board.md`: 구현 마일스톤과 작업 상태.
- `docs/15-prototype-scope-definition.md`: 첫 프로토타입의 포함/제외 범위.
- `docs/16-technical-design-brief.md`: Unity 구현 구조와 스크립트 책임.
- `docs/06-eatventure-reference-analysis.md`: Eatventure에서 참고할 구조와 피해야 할 표면 복제.
- `docs/10-economy-and-progression-design.md`: 제품 레벨, 단가 성장, 비용 커브, 자동화 구매 시간 압축의 경제 설계.
- `docs/12-meta-progression-design.md`: 보상 상자, 장비, 고유 작업자, 조합 같은 장기 메타 설계.
- `docs/99-session-continuity-guide.md`: 새 세션에서 AI가 일하는 절차와 협업 규칙.

## 작업 후 갱신 규칙

의미 있는 구현 작업 후에는 기본적으로 다음을 갱신한다.

- `docs/18-prototype-task-board.md`
- `docs/work-history.md`

방향 결정이 바뀌었으면 추가로 `docs/22-decision-log.md`를 갱신한다. 세부 설계가 생겼으면 해당 전담 문서에만 상세를 쓰고, README에는 요약만 둔다.
