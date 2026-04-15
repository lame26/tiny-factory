# Unity 초보자 조작 가이드

## 문서 목적

이 문서는 Unity를 처음 사용하는 사용자가 첫 프로토타입 작업 중 최소한으로 알아야 할 에디터 조작을 정리한다. 목표는 Unity를 완전히 배우는 것이 아니라, AI와 협업하며 프로젝트를 열고, 실행하고, 에러를 확인하고, 간단한 오브젝트 상태를 설명할 수 있는 수준까지 빠르게 도달하는 것이다.

## 가장 먼저 익힐 창

### Hierarchy

씬 안의 모든 오브젝트 목록이다.

예:

- `Worker`
- `Main Camera`
- `PartBin`
- `AssemblyBench`
- `SellCounter`

오브젝트가 씬에 있는지 확인할 때 본다.

### Scene

작업용 화면이다. 오브젝트를 배치하고 위치를 확인한다.

주로 할 일:

- 오브젝트 선택
- 위치 확인
- 카메라와 작업장 배치 확인

### Game

실제 게임 화면이다. Play 버튼을 누르면 플레이어가 보게 될 운영 화면을 확인한다.

주로 할 일:

- 게임이 실행되는지 확인
- UI가 보이는지 확인
- 카메라 구도가 괜찮은지 확인

### Inspector

선택한 오브젝트의 설정을 보는 창이다.

주로 할 일:

- 위치, 회전, 크기 확인
- 붙어 있는 스크립트 확인
- 스크립트의 공개 값 수정
- Collider, Renderer, Rigidbody 같은 컴포넌트 확인

### Project

파일과 폴더를 보는 창이다.

주로 할 일:

- `Assets/_Project` 폴더 확인
- 씬 파일 열기
- 스크립트 위치 확인
- Prefab 위치 확인

### Console

에러와 경고를 보는 창이다.

중요:

- 빨간색은 Error다. 실행이 막힐 가능성이 높다.
- 노란색은 Warning이다. 당장 막히지 않을 수 있지만 확인이 필요하다.
- 에러가 있으면 메시지를 요약하지 말고 가능한 전체를 전달한다.

## 기본 조작

### 프로젝트 열기

1. Unity Hub 실행
2. Projects 탭으로 이동
3. `TinyFactoryPrototype` 선택
4. Unity Editor가 열릴 때까지 기다림
5. Console에 빨간 에러가 있는지 확인

### 씬 열기

1. Project 창에서 `Assets/_Project/Scenes` 이동
2. `Prototype_01_Workshop` 더블클릭
3. Hierarchy에 해당 씬 오브젝트가 보이는지 확인

### 실행하기

1. 상단 Play 버튼 클릭
2. Game 창에서 동작 확인
3. 작업자가 자동으로 움직이는지 확인
4. 스테이션 클릭 또는 UI 버튼이 동작하는지 확인
5. 다시 Play 버튼을 눌러 종료

주의:

- Play Mode 중 Inspector에서 바꾼 값은 종료하면 사라질 수 있다.
- 영구 수정은 Play Mode가 꺼진 상태에서 한다.

### Console 에러 복사하기

1. Console 창 열기
2. 빨간 에러 클릭
3. 메시지 전체 선택
4. 복사해서 AI에게 전달

가능하면 다음도 함께 전달한다.

- 언제 발생했는지
- Play 버튼을 누르자마자 발생했는지
- 특정 행동 후 발생했는지
- 마지막으로 수정한 파일 또는 오브젝트

## 오브젝트 선택과 확인

### 오브젝트가 씬에 있는지 확인

1. Hierarchy에서 이름을 찾는다.
2. 오브젝트를 클릭한다.
3. Inspector에서 Transform 값을 확인한다.
4. Scene 창에서 선택 표시가 보이는지 확인한다.

### 오브젝트 위치가 이상할 때

AI에게 다음 정보를 전달한다.

- 오브젝트 이름
- Inspector의 Position 값
- Scene 또는 Game 화면 캡처
- 예상 위치
- 실제 위치

## Inspector에서 주로 볼 값

### Transform

모든 GameObject에 있는 기본 컴포넌트다.

- Position: 위치
- Rotation: 회전
- Scale: 크기

### Collider

충돌, 클릭 판정, 작업자 도착 판정에 쓰인다.

첫 프로토타입에서 자주 볼 수 있는 것:

- Box Collider
- Sphere Collider
- Capsule Collider

### Rigidbody

물리 이동이나 충돌 처리에 쓰인다. 첫 프로토타입에서는 필요한 오브젝트에만 붙인다.

### Script Component

C# 스크립트를 붙인 컴포넌트다.

예:

- `WorkerController`
- `StationSelectionController`
- `PartBin`
- `AssemblyBench`
- `SellCounter`

스크립트에 공개된 값은 Inspector에서 조정할 수 있다.

## 첫 프로토타입에서 자주 생길 문제

### Play 버튼을 눌렀는데 아무것도 안 움직임

확인할 것:

- Console에 에러가 있는가
- Worker 오브젝트가 있는가
- Worker에 `WorkerController`가 붙어 있는가
- PartBin, AssemblyBench, SellCounter 참조가 연결되어 있는가
- Game 창이 활성화되어 있는가

### 스테이션 선택 또는 업그레이드가 안 됨

확인할 것:

- 선택 컨트롤러가 씬에 있는가
- 대상 오브젝트에 선택 또는 스테이션 스크립트가 붙어 있는가
- 마우스 클릭을 받을 Collider가 있는가
- Collider가 있는가
- UI 버튼이 올바른 메서드에 연결되어 있는가

### 아이템이 안 보임

확인할 것:

- 아이템 오브젝트가 생성되었는가
- 위치가 너무 멀리 있지 않은가
- Scale이 0이 아닌가
- Renderer가 있는가
- 카메라에 잡히는 위치인가

### UI가 안 보임

확인할 것:

- Canvas가 있는가
- Text 또는 Button이 Canvas 아래에 있는가
- Game 창 해상도에서 화면 밖으로 나가지 않았는가
- Console에 UI 관련 에러가 있는가

## AI에게 질문할 때 좋은 형식

문제가 생겼을 때는 다음 형식이 가장 좋다.

```text
상황:
예상 동작:
실제 동작:
에러 메시지:
내가 마지막으로 한 작업:
관련 화면:
```

예:

```text
상황: Play 버튼을 눌렀는데 Worker가 PartBin으로 이동하지 않음
예상 동작: Worker가 부품을 들고 AssemblyBench로 이동해야 함
실제 동작: Worker가 제자리에 멈춰 있음
에러 메시지: Console 빨간 에러 없음
내가 마지막으로 한 작업: PartBin 위치를 옮김
관련 화면: Game 창 캡처 첨부 예정
```

## 처음에는 몰라도 되는 것

첫 프로토타입 단계에서는 다음을 몰라도 된다.

- 애니메이터 세부 설정
- 복잡한 셰이더
- 라이트 베이킹
- Addressables
- DOTS/ECS
- Timeline
- Cinemachine 고급 설정
- Steamworks
- 빌드 자동화

지금 필요한 것은 프로젝트 열기, 실행, 에러 확인, 오브젝트 상태 설명이다.
