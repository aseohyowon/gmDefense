# 🧪 테스트 방법 안내

이 문서는 **Merge Hero Defense Game** 프로젝트의 테스트 방법을 설명한다.

---

## 📋 테스트 구조

```
Tests/
├── GMDefense.Tests.csproj          ← .NET 테스트 프로젝트 설정
├── GameLogic/                       ← Unity 없이 실행 가능한 순수 게임 로직
│   ├── 골드시스템로직.cs             ← GoldSystem.cs 로직 추출
│   ├── 영웅데이터로직.cs             ← HeroData.cs 로직 추출
│   ├── 웨이브로직.cs                 ← WaveManager.cs 로직 추출
│   └── 시너지로직.cs                 ← TraitSystem.cs 로직 추출
└── Tests/                           ← 단위 테스트 파일
    ├── 골드시스템테스트.cs            ← 골드 추가/소비/소환비용 테스트 (15개)
    ├── 영웅데이터테스트.cs            ← 합성 조건/공격력 계산 테스트 (16개)
    ├── 웨이브로직테스트.cs            ← 보스 주기/난이도 스케일링 테스트 (16개)
    └── 시너지테스트.cs               ← 시너지 카운팅/배율 테스트 (13개)
```

---

## ▶️ 방법 1: `dotnet test` 로 단위 테스트 실행 (Unity 불필요)

### 사전 요구사항
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) 설치

### 실행 방법

```bash
# 프로젝트 루트에서
cd Tests
dotnet test
```

### 예상 출력

```
Passed!  - Failed: 0, Passed: 60, Skipped: 0, Total: 60, Duration: ~50ms
```

### 상세 출력 (각 테스트 이름 표시)

```bash
cd Tests
dotnet test --verbosity normal
```

### 특정 테스트 클래스만 실행

```bash
# 골드 시스템 테스트만 실행
cd Tests
dotnet test --filter "FullyQualifiedName~골드시스템테스트"

# 영웅 데이터 테스트만 실행
dotnet test --filter "FullyQualifiedName~영웅데이터테스트"

# 웨이브 로직 테스트만 실행
dotnet test --filter "FullyQualifiedName~웨이브로직테스트"

# 시너지 테스트만 실행
dotnet test --filter "FullyQualifiedName~시너지테스트"
```

---

## ▶️ 방법 2: Unity Editor에서 플레이 테스트

Unity Editor에서 직접 씬을 실행하여 게임을 테스트한다.

### 사전 요구사항
- [Unity 2022 LTS](https://unity.com/releases/lts) 설치
- Android Build Support 모듈 설치 (모바일 테스트 시)

### Unity 프로젝트 열기

1. Unity Hub를 실행한다
2. **Open** → 이 저장소의 루트 폴더 선택
3. Unity 2022.x 버전으로 프로젝트를 연다

### 기본 씬 구성

Unity Editor에서 새 씬을 만들고 아래 게임오브젝트들을 추가한다:

| 게임오브젝트 이름 | 부착 컴포넌트 | 설명 |
|-----------------|-------------|------|
| `GameManager` | `GameManager.cs` | 게임 전체 관리 |
| `GoldSystem` | `GoldSystem.cs` | 골드 시스템 |
| `GridSystem` | `GridSystem.cs` | 영웅 배치 그리드 |
| `MergeManager` | `MergeManager.cs` | 합성 시스템 |
| `WaveManager` | `WaveManager.cs` | 웨이브 관리 |
| `EnemySpawner` | `EnemySpawner.cs` | 적 생성 |
| `TraitSystem` | `TraitSystem.cs` | 특성 시너지 |
| `UI Canvas` | `UIManager.cs` | UI 관리 |

### ScriptableObject 영웅 데이터 생성

1. Unity Editor 메뉴: **Assets > Create > GMDefense > 영웅 데이터**
2. 각 영웅 종류(전사/궁수/마법사 등)별로 7등급 데이터를 생성한다

| 설정 항목 | 예시 값 |
|----------|--------|
| 영웅이름 | 전사_1등급 |
| 영웅종류 | 전사 |
| 영웅등급 | 등급1 |
| 공격력 | 10 |
| 공격속도 | 1.0 |
| 사거리 | 3.0 |
| 특성목록 | 빛의수호자 |

### 인스펙터 설정

**GameManager**:
- `소환가능영웅풀`: 생성한 HeroData ScriptableObject들을 드래그하여 배정
- `영웅프리팹`: Hero 컴포넌트가 부착된 프리팹

**GridSystem**:
- `열수`: 4 (기본값)
- `행수`: 3 (기본값)
- `슬롯간격`: 1.5 (기본값)

**WaveManager**:
- `웨이브대기시간`: 10 (초)
- `보스웨이브주기`: 5 (5웨이브마다 보스)
- `일반적프리팹`: NormalEnemy 컴포넌트가 부착된 프리팹
- `보스적프리팹`: BossEnemy 컴포넌트가 부착된 프리팹

**EnemySpawner**:
- `경로포인트`: 적이 이동할 웨이포인트 Transform들 배정

### 플레이 테스트 방법

1. **Play 버튼(▶)** 을 눌러 게임을 시작한다
2. UI의 **게임 시작** 버튼을 클릭한다
3. **소환 버튼** 을 눌러 영웅을 소환한다
4. 같은 종류·같은 등급 영웅 두 개를 **드래그하여 겹치면** 합성된다
5. 웨이브가 시작되면 적들이 경로를 따라 이동한다

---

## ▶️ 방법 3: Unity Test Framework (Edit Mode 테스트)

Unity 내장 테스트 러너를 사용하여 에디터에서 테스트한다.

1. Unity Editor 메뉴: **Window > General > Test Runner**
2. **Edit Mode** 탭 선택
3. **Run All** 클릭

> 💡 현재 `Tests/` 폴더의 단위 테스트는 Unity Test Framework와 별개로,
> `.NET SDK`만 있으면 실행 가능한 독립 테스트다.

---

## 📊 테스트 커버리지 요약

| 테스트 파일 | 검증 항목 | 테스트 수 |
|-----------|---------|---------|
| 골드시스템테스트 | 골드 추가, 소비, 소환 비용 증가, 이벤트 발행 | 15개 |
| 영웅데이터테스트 | 합성 가능 조건, 최종 공격력 계산, 최대 등급 | 16개 |
| 웨이브로직테스트 | 보스 주기, 적 생성 수, 생성 간격, 보상 골드 | 16개 |
| 시너지테스트 | 특성 카운팅, 공격력 배율, 공격속도 배율 | 13개 |
| **합계** | | **60개** |

---

## 🔧 문제 해결

### `dotnet test` 실패 시

```bash
# .NET 버전 확인
dotnet --version  # 8.0 이상이어야 함

# 패키지 복원
cd Tests
dotnet restore

# 재실행
dotnet test
```

### Unity 씬에서 NullReferenceException 발생 시

- 모든 Manager 오브젝트가 씬에 배치되었는지 확인한다
- `GameManager`의 소환가능영웅풀에 HeroData가 1개 이상 배정되었는지 확인한다
- `EnemySpawner`의 경로포인트가 설정되었는지 확인한다
