# 기획/설계 문서 로드맵

## 목적

이 문서는 초기 기획 이후 어떤 순서로 문서를 발전시켜야 하는지 정의한다. 목표는 바로 구현에 들어가는 것이 아니라, 조사, 비교, 치환, 설계를 거쳐 프로토타입 범위를 명확히 하는 것이다.

## 현재 문서 세트의 역할

- `00-project-charter.md`: 프로젝트 방향과 원칙
- `01-initial-game-concept.md`: 초기 컨셉 가설
- `02-research-plan.md`: 검증을 위한 조사 계획
- `03-reference-extraction-framework.md`: Eatventure 분석 기준
- `04-theme-translation-guide.md`: 전자/가전 테마 치환 기준
- `05-design-document-roadmap.md`: 이후 문서 작성 순서

현재 문서 세트는 "확정 기획"이 아니라 "조사와 설계를 시작하기 위한 기준선"이다.

## 이후 문서 목록 제안

### 06-eatventure-reference-analysis.md

목적:

- Eatventure의 표면층, 시스템층, 경제층, UX층, 메타층, 상품층을 실제 자료 기반으로 분석한다.
- 그대로 모사하면 위험한 요소와 구조적으로 참고할 수 있는 원리를 분리한다.

필요한 조사 결과:

- 첫 플레이 흐름
- 업그레이드와 자동화 구조
- 스테이지/이벤트/장비 구조
- 유저 리뷰와 공략에서 반복되는 핵심 포인트

우선순위: 매우 높음

### 07-steam-comparable-products.md

목적:

- Steam에서 유사한 운영, 자동화, 캐주얼 타이쿤 게임을 비교한다.
- PC 유저가 기대하는 조작감, 콘텐츠 볼륨, 가격, 깊이를 파악한다.

필요한 조사 결과:

- Steam 스토어 페이지
- 태그와 가격
- 긍정/부정 리뷰 패턴
- 플레이 영상
- 업데이트 빈도와 개발 방식

우선순위: 매우 높음

### 08-product-theme-options.md

목적:

- 전자/가전 제품군 후보를 비교하고, 어떤 제품을 초반/중반/후반에 배치할지 제안한다.
- 제품군별 공정, 시각적 구분, 업그레이드 확장성을 평가한다.

필요한 조사 결과:

- 친숙한 전자제품 목록
- 제품별 대표 부품
- 제품별 조립/검사/포장 표현 가능성
- 유저가 직관적으로 이해할 수 있는 단가 상승 순서

우선순위: 높음

### 09-core-loop-design.md

목적:

- 실제 1분, 5분, 30분 플레이 흐름을 설계한다.
- 플레이어 직접 조작, 직원 작업, 자동화 설비의 역할을 정리한다.

필요한 조사 결과:

- Eatventure의 코어 루프 분석
- Steam 유사 게임의 세션 구조
- 전자/가전 치환 기준

우선순위: 매우 높음

### 10-economy-and-progression-design.md

목적:

- 수익, 비용, 업그레이드, 제품 가격, 진행 속도의 기본 구조를 설계한다.
- 구체 수치 확정보다는 경제 카테고리와 밸런스 원칙을 먼저 정한다.

필요한 조사 결과:

- 레퍼런스의 보상 주기
- 업그레이드 카테고리
- PC용 반복 플레이 기대치
- 제품군별 가치 단계

우선순위: 높음

### 11-automation-and-production-system.md

목적:

- 조립대, 컨베이어, 로봇 팔, 검사대, 포장대, 출고 도크 등 생산 시스템을 설계한다.
- 자동화가 언제, 어떻게, 왜 재미있어지는지 정의한다.

필요한 조사 결과:

- 자동화 게임의 병목 구조
- Eatventure의 직원/자동화 역할
- 전자/가전 테마에서 자연스러운 설비 목록

우선순위: 높음

### 12-meta-progression-design.md

목적:

- 공장 이전, 브랜드 성장, 연구 개발, 제품 라인 확장, 이벤트 구조를 설계한다.
- 세션 간 지속 성장의 방향을 정의한다.

필요한 조사 결과:

- Eatventure의 메타 성장 구조
- Steam 유사 게임의 장기 목표 구조
- PC 유저 리뷰에서 반복되는 콘텐츠 볼륨 기대

우선순위: 중간

### 13-ux-hud-direction.md

목적:

- PC 화면에서 생산 라인, 주문, 업그레이드, 목표, 자원을 어떻게 보여줄지 정의한다.
- 직접 조작과 관리 조작이 충돌하지 않게 한다.

필요한 조사 결과:

- Eatventure의 UX 구조
- Steam 유사 게임의 HUD 사례
- 마우스/키보드 조작 가설
- 정보 밀도 요구사항

우선순위: 높음

### 14-art-and-audio-direction.md

목적:

- 아트 스타일, 제품 실루엣, 공장 공간, 피드백 효과, 사운드 방향을 정리한다.
- Eatventure와 시각적으로 다른 정체성을 확보한다.

필요한 조사 결과:

- 레퍼런스 표면층 위험 요소
- 전자/가전 제품군 시각 자료
- Steam 스토어에서 눈에 띄는 캐주얼 생산 게임의 화면 특징

우선순위: 중간

### 15-prototype-scope-definition.md

목적:

- 첫 프로토타입에서 무엇을 만들고 무엇을 제외할지 정의한다.
- 검증 질문, 성공 기준, 플레이 시간, 포함 시스템을 명확히 한다.

필요한 조사 결과:

- 코어 루프 상세 기획
- 제품군 후보
- 자동화 시스템 초안
- UX 방향
- 최소 경제 구조

우선순위: 매우 높음, 단 앞선 핵심 문서 이후 작성

### 16-technical-design-brief.md

목적:

- 구현에 들어가기 전 필요한 기술적 방향을 정리한다.
- 엔진 후보, 데이터 구조, 저장, 시뮬레이션 단위, 입력 처리, 프로토타입 구현 범위를 검토한다.

필요한 조사 결과:

- 프로토타입 범위
- 코어 루프
- 자동화/생산 시스템 요구사항
- HUD와 입력 요구사항

우선순위: 프로토타입 직전

## 우선 작성 순서

1. `06-eatventure-reference-analysis.md`
   레퍼런스를 구조적으로 분석하지 않으면 무엇을 차용하고 무엇을 피해야 하는지 판단할 수 없다.

2. `07-steam-comparable-products.md`
   목표 플랫폼이 Steam이므로, 모바일 레퍼런스와 PC 시장 기대의 차이를 초기에 확인해야 한다.

3. `08-product-theme-options.md`
   전자/가전 테마가 단순 스킨 변경으로 끝나지 않으려면 제품군과 공정 후보를 먼저 정리해야 한다.

4. `09-core-loop-design.md`
   조사 결과와 테마 치환을 바탕으로 실제 플레이 루프를 구체화한다.

5. `11-automation-and-production-system.md`
   이 프로젝트의 차별화 핵심인 생산/자동화 쾌감을 구체화한다.

6. `10-economy-and-progression-design.md`
   루프와 자동화 구조가 잡힌 뒤 경제 카테고리와 성장 속도를 설계한다.

7. `13-ux-hud-direction.md`
   코어 루프와 경제 정보가 정리된 뒤 화면 정보 구조를 설계한다.

8. `15-prototype-scope-definition.md`
   구현할 최소 범위와 검증 질문을 확정한다.

9. `16-technical-design-brief.md`
   프로토타입 범위를 기준으로 기술 설계를 작성한다.

## 구현 단계로 넘어가기 전 필요한 문서 세트

최소 필요 문서:

- `00-project-charter.md`
- `01-initial-game-concept.md`
- `06-eatventure-reference-analysis.md`
- `07-steam-comparable-products.md`
- `08-product-theme-options.md`
- `09-core-loop-design.md`
- `10-economy-and-progression-design.md`
- `11-automation-and-production-system.md`
- `13-ux-hud-direction.md`
- `15-prototype-scope-definition.md`
- `16-technical-design-brief.md`

권장 추가 문서:

- `12-meta-progression-design.md`
- `14-art-and-audio-direction.md`
- 테스트 플레이 기록 템플릿
- 리스크 목록
- 일정/마일스톤 초안

## 문서 간 의존 관계

- 레퍼런스 분석은 코어 루프와 경제 설계의 근거가 된다.
- Steam 비교 분석은 상품 방향, UX 밀도, 콘텐츠 볼륨 판단의 근거가 된다.
- 테마 옵션 문서는 제품군, 공정, 아트 방향의 근거가 된다.
- 코어 루프 문서는 자동화, 경제, UX 문서의 중심 입력값이다.
- 프로토타입 범위 정의서는 기술 설계와 구현 착수의 직접 입력값이다.

## 다음 단계로 가장 먼저 해야 할 작업

1. Eatventure 분석 자료를 수집하고 `06-eatventure-reference-analysis.md`를 작성한다.
2. Steam 유사 제품 8~12개를 선정해 `07-steam-comparable-products.md` 비교 표를 만든다.
3. 전자/가전 제품군 후보를 10개 이상 정리하고 `08-product-theme-options.md`에서 초반/중반/후반 배치안을 만든다.
