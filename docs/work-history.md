# 작업 히스토리

이 문서는 프로젝트의 업무 연속성을 유지하기 위한 기록 파일이다. 새 세션에서는 `docs/99-session-continuity-guide.md`를 먼저 읽고, 이어서 이 문서를 읽어 최근 결정과 다음 작업을 확인한다.

## 2026-04-15

### 작업 요약

- 프로젝트 초기 기획 문서 세트를 생성했다.
- Eatventure를 구조적 레퍼런스로 삼되, 음식/레스토랑 운영을 전자/가전제품 생산, 조립, 판매, 자동화 게임으로 재해석하는 방향을 문서화했다.
- Steam PC용 프로토타입으로 이동할 수 있을 만큼 초기 방향은 충분하다고 판단했다.
- 기술 스택은 Unity 기반을 추천했다.
- 사용자가 Unity 초보이며 AI 도움을 받아 바이브 코딩 방식으로 진행하고 싶다는 점을 확인했다.
- 새 세션 연속성을 위해 본 지침 파일과 히스토리 파일을 생성했다.
- 사용자의 승인에 따라 첫 Unity 프로토타입 범위 정의 문서를 생성했다.
- Unity를 집에서 세팅해야 하므로, 세팅 전 2시간 동안 진행 가능한 준비 문서를 추가로 생성했다.
- Unity 없이도 진행 가능한 추가 준비 작업으로 초보자 조작 가이드, 플레이테스트 기록 템플릿, 제품군 후보 문서를 생성했다.
- 사용자의 제안에 따라 업장 성장 progression과 장기 제품 후보 100개 이상을 미리 브레인스토밍했다.
- 현재 게임 가칭을 `Tiny Factory`로 정했다.
- 루트 진입점으로 `README.md`를 복구하고, 핵심 결정/미결정 사항을 빠르게 확인하는 결정 로그를 생성했다.

### 생성한 파일

- `docs/00-project-charter.md`
- `docs/01-initial-game-concept.md`
- `docs/02-research-plan.md`
- `docs/03-reference-extraction-framework.md`
- `docs/04-theme-translation-guide.md`
- `docs/05-design-document-roadmap.md`
- `docs/99-session-continuity-guide.md`
- `docs/work-history.md`
- `docs/15-prototype-scope-definition.md`
- `docs/16-technical-design-brief.md`
- `docs/17-unity-setup-checklist.md`
- `docs/18-prototype-task-board.md`
- `docs/19-unity-beginner-operations-guide.md`
- `docs/20-playtest-log-template.md`
- `docs/08-product-theme-options.md`
- `docs/09-business-progression-design.md`
- `docs/21-product-catalog-brainstorm.md`
- `README.md`
- `docs/22-decision-log.md`

### 결정된 사항

- 목표 플랫폼은 Steam PC로 둔다.
- 기본 레퍼런스는 Eatventure이나, 표면 모사가 아니라 구조적 원리 추출만 허용한다.
- 핵심 테마는 전자/가전제품 생산, 조립, 판매, 자동화다.
- 현재 단계는 문서 중심 초기 기획에서 프로토타입 범위 정의 단계로 넘어갈 수 있다.
- 추천 기술 스택은 Unity, C#, URP, 3D 탑다운 또는 아이소메트릭 2.5D다.
- 초기 프로토타입에는 Steamworks 연동을 넣지 않는다.
- Steam 연동은 출시 준비 단계에서 Steamworks.NET을 우선 검토한다.
- 사용자는 새 세션에서 `docs/99-session-continuity-guide.md`를 읽고 이어서 작업하는 방식을 선호한다.
- 의미 있는 작업 후에는 `docs/work-history.md`를 업데이트한다.
- 중대한 방향 결정이나 사용자 의도가 불명확한 경우 AI가 혼자 추측하지 않고 반드시 사용자에게 되묻는다.
- 첫 프로토타입은 최종 게임의 축소판이 아니라 핵심 루프 검증용으로 만든다.
- 첫 프로토타입의 최소 루프는 부품 픽업, 조립, 판매, 돈 획득, 업그레이드, 직원 1명 자동화다.
- 첫 프로토타입에서는 Steamworks, 저장, 여러 제품군, 메타 진행, 최종 아트, 정교한 밸런스를 제외한다.
- 첫 프로토타입 기술 구조는 Unity 6 LTS, C#, URP, 3D 탑다운/아이소메트릭 2.5D, uGUI 우선, 저장/Steam 제외를 기준으로 한다.
- 게임 가칭은 `Tiny Factory`다. 의미는 "내 컴퓨터 속 작은 공장"이며, 최종 출시명 확정 전까지는 상표/Steam 중복 검토가 필요하다.
- Unity 프로젝트 폴더명은 저장소 안 `unity/TinyFactoryPrototype/` 구조를 추천한다.
- 첫 씬 이름은 `Prototype_01_Workshop`을 추천한다.
- 첫 제품 후보는 보조배터리를 1순위로 추천한다. 구현 직전 사용자가 스마트폰 또는 임시 `Basic Gadget`을 선호하는지 확인한다.
- 장기 업장 progression은 전자제품 악세사리 판매점에서 시작해 수리점, 리퍼비시 샵, 브랜드 AS점, 온라인 주문 처리 센터, 소형 조립 공장, 자동화 생산 라인, 스마트 가전 공장, 글로벌 납품 허브, 연구 개발 캠퍼스로 확장하는 방향을 추천한다.
- 첫 프로토타입 배경은 "전자제품 악세사리 판매점 안쪽의 작은 조립 코너"로 보는 것이 현재 가장 자연스럽다.
- 장기 제품 후보는 120개를 1차 브레인스토밍했으며, 이후 업장 단계별 해금표와 제품별 부품/공정 매핑이 필요하다.

### 아직 미정인 사항

- Unity 정확한 버전
- Unity 프로젝트명 최종 확정
- 2D, 2.5D, 3D 중 최종 표현 방식
- uGUI와 UI Toolkit 중 초기 UI 선택
- 첫 제품군
- 첫 공장/스테이지 콘셉트
- 아트 스타일
- Steam 상품 전략

### 다음 작업 후보

1. Unity Hub와 Unity 6 LTS 설치
2. `unity/TinyFactoryPrototype/` 위치에 3D URP 프로젝트 생성
3. 생성한 Unity 버전, 프로젝트 경로, Console 에러 유무를 기록
4. `docs/18-prototype-task-board.md`의 마일스톤 1부터 진행
5. 첫 제품을 보조배터리, 스마트폰, 임시 `Basic Gadget` 중 무엇으로 표현할지 결정
6. 이후 문서 작업을 더 한다면 업장 단계별 제품 해금표를 작성

### 사용자 선호 및 지시

- 출력은 한국어를 선호한다.
- 구현보다 문서와 방향 정리를 먼저 중시한다.
- 다만 현재는 프로토타입 제작으로 이동하는 것이 우선이라고 판단하고 있다.
- AI가 중대한 사항을 혼자 판단하지 말고, 의도가 애매하면 반드시 되묻기를 원한다.
- 세션이 바뀌어도 업무 연속성을 유지할 수 있는 지침 파일을 먼저 읽고 이어서 작업하기를 선호한다.
