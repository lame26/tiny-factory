# 경제와 성장 설계

## 문서 목적

이 문서는 Tiny Factory의 돈, 제품 단가, 제품 레벨, 스테이션 해금, 직원 구매, 설비 확장의 관계를 정의한다.

핵심은 Eatventure식 성장 문법을 전자제품 생산 테마에 맞게 바꾸는 것이다. 플레이어는 단순히 생산량만 늘리는 것이 아니라, 제품 자체의 레벨을 올려 출고 단가를 키우고, 그 단가 상승으로 다음 자동화 구매 시간을 압축해야 한다.

## 핵심 결론

마일스톤 9 재테스트에서 두 번째 조립대는 정상 작동했지만 5분 처리량은 마일스톤 7A와 같은 `25회`였다. 이 결과는 조립대 수만 늘리는 것으로는 경제가 열리지 않는다는 신호다.

현재 빠진 축:

```text
제품 레벨업 -> 픽업/출고 1회당 수익 증가 -> 직원/설비 구매 시간 단축 -> 자동화 체감 증가
```

따라서 마일스톤 10은 주문 생성 튜닝보다 먼저 제품 레벨업과 단가 성장 시스템을 설계/구현한다.

## 성장 축 분리

Tiny Factory의 경제 성장은 네 축으로 나눈다.

| 축 | 담당 시스템 | 플레이어가 느끼는 변화 |
| --- | --- | --- |
| 제품 레벨 | 제품 단가 증가 | 같은 출고 1회가 더 큰 돈이 됨 |
| 조립 속도 | 조립 시간 감소 | 같은 조립대가 더 자주 제품을 만듦 |
| 작업자/동선 | 직원 수와 이동 속도 | 부품 투입과 완성품 회수가 빨라짐 |
| 설비 확장 | 조립대/자동 설비 해금 | 동시에 처리 가능한 작업 수가 늘어남 |

이 중 제품 레벨은 경제의 압축 축이다. 생산량이 늘어도 단가가 낮으면 다음 구매가 늦어지고, 자동화 확장이 체감되지 않는다.

## 제품 레벨업

첫 제품은 현재 플레이스홀더 기준 `Power Bank`로 본다. 코드상 임시 이름이 `Basic Gadget`으로 남아 있어도, 시스템 설계에서는 첫 제품을 `Power Bank`로 해석한다.

제품 레벨업 규칙:

- 제품마다 독립 레벨을 가진다.
- 제품 레벨이 오르면 픽업/출고 가치가 오른다.
- 제품 레벨업 비용은 레벨 기반 성장 커브를 따른다.
- 제품 레벨업은 항상 보이는 핵심 구매 버튼이어야 한다.
- 기존 `Sale Value` 업그레이드는 제품 레벨 시스템에 흡수하거나, 이름을 `Product Level`로 바꾼다.

초기 수치 초안:

| 제품 레벨 | 출고 가치 | 레벨업 비용 | 의도 |
| --- | --- | --- | --- |
| 1 | 5 | 10 | 첫 판매 몇 번 안에 구매 가능 |
| 2 | 7 | 16 | 첫 직원 전후로 단가 상승 체감 |
| 3 | 10 | 26 | 두 번째 조립대 목표를 앞당김 |
| 4 | 14 | 42 | 자동화 구매 전 경제 압축 |
| 5 | 20 | 68 | 5분 테스트에서 명확한 성장감 |

장기적으로는 다음 식을 기본 후보로 둔다.

```text
출고 가치 = baseValue * valueGrowth^(productLevel - 1)
레벨업 비용 = baseCost * costGrowth^(productLevel - 1)
```

첫 구현 후보:

```text
baseValue: 5
valueGrowth: 1.38
baseCost: 10
costGrowth: 1.6
```

값은 정수 반올림으로 표시한다. 이후 `K`, `M` 단위까지 자연스럽게 이어지도록 기존 `MoneyFormatter`를 사용한다.

## 추천 목표 순서

마일스톤 10 기준 초반 목표 순서는 다음으로 바꾼다.

```text
첫 직원 구매
-> Power Bank Lv 2
-> Power Bank Lv 3
-> 두 번째 조립대 구매
-> 작업자 처리량 또는 세 번째 작업자
```

이 순서는 생산량 확장 전에 단가를 먼저 올려, 설비 구매가 너무 늦어지지 않게 하기 위한 것이다.

HUD 목표 문구 후보:

| 상황 | 목표 |
| --- | --- |
| 작업자 1명 | `Goal: Hire first worker` |
| 작업자 2명, 제품 Lv 1 | `Goal: Level Power Bank to Lv 2` |
| 제품 Lv 2 | `Goal: Level Power Bank to Lv 3` |
| 제품 Lv 3, 조립대 1개 | `Goal: Build second assembly bench` |
| 조립대 2개 | `Goal: Improve worker throughput` |

## 기존 업그레이드와의 관계

기존 업그레이드 정리:

| 기존 요소 | 마일스톤 10 기준 처리 |
| --- | --- |
| `Sale Value` | `Product Level`로 흡수하거나 이름 변경 |
| `Assembly Speed` | 유지. 제품 단가와 별도 축 |
| `Level Selected Station` | 유지. 선택 스테이션 보조 성장 |
| `Worker Throughput` | 유지. 동선 병목 해소 |
| `Build Assembly Bench` | 유지. 제품 Lv 3 이후 목표로 권장 |

우선순위:

1. 제품 레벨업을 HUD 핵심 버튼으로 만든다.
2. `PickupCounter`의 출고 가치는 제품 레벨에서 계산한다.
3. `Sale Value`라는 용어는 제거한다. 현재 구조가 픽업/출고이므로 `Product Value` 또는 `Product Level`이 맞다.

## 기술 구조 초안

첫 구현은 단일 제품만 다룬다.

필요 스크립트 후보:

```text
ProductProgressionManager
- productName
- productLevel
- basePickupValue
- valueGrowth
- levelUpBaseCost
- levelUpCostGrowth
- CurrentPickupValue
- LevelUpCost
- TryLevelUpProduct()
```

연결:

```text
ProductProgressionManager -> PickupCounter: CurrentPickupValue 제공
ProductProgressionManager -> HudController: 제품 레벨/비용/목표 표시
ProductProgressionManager -> UpgradeManager: 기존 Sale Value 업그레이드 대체 또는 위임
```

`PickupCounter`는 더 이상 내부 `saleValue`를 단독 진실로 갖지 않는다. 첫 구현에서는 호환을 위해 `saleValue` 필드를 남기되, `ProductProgressionManager`가 있으면 그 값을 우선 사용한다.

## 테스트 기준

마일스톤 10 재테스트에서 확인할 것:

| 항목 | 목표 |
| --- | --- |
| 첫 직원 구매 | 90~120초 유지 |
| Power Bank Lv 2 | 140초 안 |
| Power Bank Lv 3 | 210초 안 |
| 두 번째 조립대 구매 | 240초 안 |
| 5분 후 픽업/출고 수 | 25회 이상 |
| 5분 후 보유 금액 | 마일스톤 9보다 증가 |
| 두 번째 조립대 구매 후 60초 수익 | 구매 전 60초보다 증가 |

중요한 점은 픽업/출고 수만 보지 않는 것이다. 제품 레벨업을 넣으면 같은 출고 수에서도 돈 증가량이 달라져야 한다.

## 다음 구현 작업

1. `ProductProgressionManager`를 추가한다.
2. `PickupCounter`가 제품 레벨 기반 출고 가치를 사용하게 한다.
3. HUD의 `Sale Value` 버튼을 `Level Power Bank`로 바꾼다.
4. 추천 목표 순서를 제품 레벨 중심으로 바꾼다.
5. 마일스톤 9와 같은 조건으로 5분 재테스트를 진행한다.
