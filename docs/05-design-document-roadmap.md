# 기획/설계 문서 로드맵

## 목적

이 문서는 프로젝트 문서들의 역할을 명확히 나누고, 같은 내용을 여러 문서에 반복해서 쓰는 문제를 줄이기 위한 기준서다.

원칙은 단순하다.

- 결정 요약은 `docs/22-decision-log.md`에 둔다.
- 구현 상태와 다음 작업은 `README.md`와 `docs/18-prototype-task-board.md`에 둔다.
- 시간순 기록은 `docs/work-history.md`에 둔다.
- 세부 설계는 전담 문서 하나에 둔다.
- 다른 문서에는 필요한 경우 짧은 요약과 링크만 둔다.

## 문서 역할

| 문서 | 역할 | 중복 허용 범위 |
| --- | --- | --- |
| `README.md` | 새 세션 진입점. 현재 상태, 핵심 방향, 다음 작업만 빠르게 확인한다. | 최신 결정을 짧게 요약해도 된다. 세부 설계는 쓰지 않는다. |
| `docs/00-project-charter.md` | 프로젝트의 최초 목적, 원칙, 하지 않을 일을 기록한다. | 역사적 기준선이므로 자주 고치지 않는다. |
| `docs/01-initial-game-concept.md` | 게임 컨셉 가설과 장기 판타지를 설명한다. | 최신 결정과 다르면 최신 결정 문서가 우선한다. |
| `docs/02-research-plan.md` | 조사해야 할 질문과 자료 범위를 정한다. | 조사 계획 문서이므로 구현 상태를 넣지 않는다. |
| `docs/03-reference-extraction-framework.md` | 레퍼런스 분석 방법과 표면 복제 방지 기준을 정한다. | 개별 시스템 상세는 넣지 않는다. |
| `docs/04-theme-translation-guide.md` | 음식/레스토랑 구조를 전자/가전 테마로 바꾸는 치환 기준을 정한다. | 구체 수치와 구현 상태는 넣지 않는다. |
| `docs/05-design-document-roadmap.md` | 이 문서. 문서 역할과 중복 관리 기준을 정한다. | 없음. |
| `docs/06-eatventure-reference-analysis.md` | Eatventure에서 참고할 구조와 피해야 할 요소를 정리한다. | 최신 구현 상태는 넣지 않는다. |
| `docs/08-product-theme-options.md` | 제품군 후보와 공정 아이디어를 모은다. | 확정 제품 목록이 아니다. |
| `docs/09-business-progression-design.md` | 업장/스테이지 확장과 장기 성장 아이디어를 모은다. | 메타 장비 상세는 `docs/12`로 분리한다. |
| `docs/10-economy-and-progression-design.md` | 제품 레벨, 단가 성장, 비용 커브, 직원/설비 구매 시간 압축의 경제 설계 기준을 둔다. | README와 결정 로그에는 요약만 둔다. |
| `docs/12-meta-progression-design.md` | 보상 상자, 장비, 고유 작업자, 조합 기능의 장기 메타 설계 기준을 둔다. | README와 결정 로그에는 요약만 둔다. |
| `docs/15-prototype-scope-definition.md` | 첫 프로토타입에서 만들 것과 제외할 것을 정의한다. | 장기 시스템은 제외 항목 또는 설계 슬롯으로만 언급한다. |
| `docs/16-technical-design-brief.md` | Unity 구현 구조, 씬 구성, 스크립트 책임을 정한다. | 기획 설명은 필요한 만큼만 짧게 둔다. |
| `docs/17-unity-setup-checklist.md` | Unity 설치와 프로젝트 준비 체크리스트. | 설계 변경을 반영하지 않는다. |
| `docs/18-prototype-task-board.md` | 구현 마일스톤과 작업 상태를 관리한다. | 작업 완료 상태는 여기와 `work-history`에만 기록한다. |
| `docs/19-unity-beginner-operations-guide.md` | Unity 초보자용 조작 안내. | 게임 설계 상세를 넣지 않는다. |
| `docs/20-playtest-log-template.md` | 테스트 기록 양식. | 최신 설계 질문이 바뀌면 체크 항목만 갱신한다. |
| `docs/21-product-catalog-brainstorm.md` | 장기 제품 후보 목록. | 확정 제품군 문서가 아니다. |
| `docs/22-decision-log.md` | 현재 확정 결정과 보류 사항의 단일 요약표. | 가장 최신 결정을 짧게 유지한다. |
| `docs/99-session-continuity-guide.md` | 새 세션에서 AI가 일하는 절차와 협업 규칙. | 프로젝트 설계 상세는 링크로 보낸다. |
| `docs/work-history.md` | 날짜별 작업 기록과 변경 이유. | 중복이 아니라 히스토리 보존용으로 상세 기록을 허용한다. |

## 현재 단일 출처

| 주제 | 기준 문서 |
| --- | --- |
| 현재 결정 요약 | `docs/22-decision-log.md` |
| 다음 구현 작업 | `docs/18-prototype-task-board.md` |
| 첫 프로토타입 범위 | `docs/15-prototype-scope-definition.md` |
| Unity 스크립트/씬 구조 | `docs/16-technical-design-brief.md` |
| Eatventure 참고 원칙 | `docs/06-eatventure-reference-analysis.md` |
| 테마 치환 기준 | `docs/04-theme-translation-guide.md` |
| 제품 레벨/경제 성장 | `docs/10-economy-and-progression-design.md` |
| 보상 상자/장비/조합 메타 | `docs/12-meta-progression-design.md` |
| 작업 이력 | `docs/work-history.md` |

## 중복 작성 기준

중복을 허용하는 경우:

- README처럼 새 세션에서 바로 읽어야 하는 요약
- 결정 로그처럼 한눈에 보는 표
- 작업 히스토리처럼 시간순 맥락 보존이 필요한 기록
- 작업 보드처럼 구현 상태를 표시해야 하는 경우

중복을 피해야 하는 경우:

- 같은 시스템의 상세 규칙을 여러 문서에 반복 작성
- 이미 완료된 작업 목록을 여러 설계 문서에 복사
- 최신 결정과 과거 가설을 같은 톤으로 병렬 배치
- 구현 세부를 컨셉 문서에 길게 작성

## 새 결정이 생겼을 때 갱신 순서

1. 결정이 핵심 방향이면 `docs/22-decision-log.md`를 먼저 갱신한다.
2. 구현 범위에 영향이 있으면 `docs/15-prototype-scope-definition.md` 또는 `docs/18-prototype-task-board.md`를 갱신한다.
3. 기술 구조에 영향이 있으면 `docs/16-technical-design-brief.md`를 갱신한다.
4. 장기 메타 설계면 `docs/12-meta-progression-design.md`를 갱신한다.
5. 새 세션 진입에 필요하면 `README.md`에 한두 줄만 요약한다.
6. 작업을 마친 뒤 `docs/work-history.md`에 시간순 기록을 남긴다.

## 현재 반영된 최신 결정

- 시점은 수직 탑다운 고정 뷰를 기준으로 한다.
- 스테이지별 시작 첫 작업자는 플레이어 고유 작업자다.
- 스테이지 클리어 등 보상으로 상자를 열고 장비 아이템을 획득한다.
- 장비는 플레이어 고유 작업자에게 장착한다.
- 장비 부위와 성장 구조는 Eatventure를 참고하되, 직접 복제하지 않는다.
- 장비 조합 또는 합성 기능을 장기 메타 시스템으로 구현할 예정이다.
