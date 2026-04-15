# Tiny Factory

`Tiny Factory`는 Eatventure류의 직관적인 운영 성장 문법을 전자/가전제품 생산, 조립, 판매, 수리, 자동화로 재해석하는 Steam PC용 게임 프로젝트다.

현재 단계는 Unity 구현 전 준비 단계다. 목표는 작은 전자제품 악세사리 판매점 안쪽의 조립 코너에서 시작해, 수리점, AS점, 소형 조립 공장, 자동화 생산 라인, 스마트 팩토리로 성장하는 게임의 첫 프로토타입을 만드는 것이다.

## 현재 기준

- 게임 가칭: `Tiny Factory`
- Unity 프로젝트명 추천: `TinyFactoryPrototype`
- Unity 프로젝트 위치 추천: `unity/TinyFactoryPrototype/`
- 목표 플랫폼: Steam PC
- 추천 엔진: Unity 6 LTS
- 추천 표현: 3D 탑다운 또는 아이소메트릭 2.5D
- 첫 프로토타입 배경: 전자제품 악세사리 판매점 안쪽의 작은 조립 코너
- 첫 제품 후보: 보조배터리 또는 임시 `Basic Gadget`

## 새 세션에서 먼저 읽을 문서

1. [세션 연속성 및 작업 지침](docs/99-session-continuity-guide.md)
2. [작업 히스토리](docs/work-history.md)
3. [첫 프로토타입 범위 정의서](docs/15-prototype-scope-definition.md)
4. [프로토타입 기술 설계 메모](docs/16-technical-design-brief.md)
5. [첫 프로토타입 작업 보드](docs/18-prototype-task-board.md)

## Unity 설치 전 읽을 문서

- [Unity 설치 및 프로젝트 생성 체크리스트](docs/17-unity-setup-checklist.md)
- [Unity 초보자 조작 가이드](docs/19-unity-beginner-operations-guide.md)

## 기획 핵심 문서

- [프로젝트 차터](docs/00-project-charter.md)
- [초기 게임 컨셉 가설](docs/01-initial-game-concept.md)
- [조사 계획](docs/02-research-plan.md)
- [레퍼런스 추출 프레임워크](docs/03-reference-extraction-framework.md)
- [테마 치환 가이드](docs/04-theme-translation-guide.md)
- [기획/설계 문서 로드맵](docs/05-design-document-roadmap.md)

## 제품/성장 구조 문서

- [제품군 후보 및 첫 제품 선택 가이드](docs/08-product-theme-options.md)
- [업장 성장 Progression 브레인스토밍](docs/09-business-progression-design.md)
- [제품 후보 카탈로그 브레인스토밍](docs/21-product-catalog-brainstorm.md)

## 프로토타입 관련 문서

- [첫 프로토타입 범위 정의서](docs/15-prototype-scope-definition.md)
- [프로토타입 기술 설계 메모](docs/16-technical-design-brief.md)
- [Unity 설치 및 프로젝트 생성 체크리스트](docs/17-unity-setup-checklist.md)
- [첫 프로토타입 작업 보드](docs/18-prototype-task-board.md)
- [Unity 초보자 조작 가이드](docs/19-unity-beginner-operations-guide.md)
- [플레이테스트 기록 템플릿](docs/20-playtest-log-template.md)

## 첫 프로토타입 목표

첫 프로토타입은 최종 게임의 축소판이 아니다. 다음 핵심 루프가 재미와 가능성을 갖는지 확인하는 검증용이다.

```text
부품 픽업
→ 조립대에 투입
→ 제품 완성
→ 판매/출고
→ 돈 획득
→ 업그레이드
→ 직원 1명 자동화
```

이번 프로토타입에서는 다음을 하지 않는다.

- Steamworks 연동
- 저장/불러오기
- 여러 제품군
- 연구/메타 진행
- 이벤트
- 최종 아트
- 정교한 경제 밸런스

## 다음 작업

1. Unity Hub와 Unity 6 LTS를 설치한다.
2. `unity/TinyFactoryPrototype/` 위치에 3D URP 프로젝트를 만든다.
3. Unity 버전, 프로젝트 경로, Console 에러 유무를 기록한다.
4. [첫 프로토타입 작업 보드](docs/18-prototype-task-board.md)의 마일스톤 1부터 진행한다.
5. 첫 제품을 보조배터리, 스마트폰, 임시 `Basic Gadget` 중 무엇으로 표현할지 결정한다.

## 작업 원칙

- 새 세션에서는 반드시 [세션 연속성 및 작업 지침](docs/99-session-continuity-guide.md)을 먼저 읽는다.
- 의미 있는 작업 후에는 [작업 히스토리](docs/work-history.md)를 업데이트한다.
- 사용자의 의도가 불명확하거나 프로젝트 방향에 큰 영향을 주는 결정은 AI가 혼자 확정하지 않고 사용자에게 되묻는다.
- Eatventure를 표면적으로 복제하지 않고, 구조적 원리만 추출해 전자/가전 테마로 재해석한다.
