# gmDefense
디펜스게임

# Merge Hero Defense (S2RD 스타일) - 영웅 합성 디펜스 게임

## 🎯 프로젝트 개요
Star2 Random Defense (S2RD) 스타일의 영웅 합성 기반 디펜스 게임.

- **엔진**: Unity 2022 (2D)
- **언어**: C#
- **플랫폼**: Android 우선 개발

---

## 🧪 테스트 실행 방법

**터미널(명령 프롬프트)** 에서 아래 명령어를 실행한다.

```bash
# 저장소 루트(gmDefense 폴더)에서
cd Tests
dotnet test
```

> 자세한 설명은 **[TESTING.md](TESTING.md)** 를 참고한다.  
> (터미널 여는 법, 경로 이동 방법, Unity 플레이 테스트 방법 포함)

---

## 📁 디렉터리 구조

```
Assets/
└── Scripts/
    ├── Core/                   # 핵심 게임 관리
    │   ├── GameManager.cs      # 게임 상태 및 흐름 전체 관리
    │   └── GoldSystem.cs       # 골드 획득/소비, 소환 비용 증가
    ├── Hero/                   # 영웅 관련 컴포넌트
    │   ├── HeroData.cs         # ScriptableObject 기반 영웅 데이터 정의
    │   ├── Hero.cs             # 영웅 자동 전투 로직
    │   └── GridSystem.cs       # 영웅 배치 그리드 슬롯 시스템
    ├── Enemy/                  # 적 관련 컴포넌트
    │   ├── Enemy.cs            # 적 기본/보스 클래스 (경로 이동, 데미지 처리)
    │   └── EnemySpawner.cs     # 웨이브 적 생성 관리
    ├── Systems/                # 핵심 게임 시스템
    │   ├── MergeManager.cs     # 영웅 합성 처리
    │   ├── DragDropHandler.cs  # 모바일 터치/드래그 앤 드롭
    │   ├── WaveManager.cs      # 웨이브 진행, 보스 스폰 관리
    │   └── TraitSystem.cs      # 영웅 특성 및 시너지 계산
    └── UI/
        └── UIManager.cs        # 골드/HP/웨이브/시너지 UI 갱신
```

---

## 🎮 핵심 게임 시스템

### 1. 영웅 소환 시스템 (`GameManager` + `GoldSystem`)
- 골드를 사용하여 랜덤 영웅 생성
- 소환 비용은 소환 횟수에 따라 증가
- 영웅은 빈 슬롯에 자동 배치

### 2. 영웅 합성 시스템 (`MergeManager` + `DragDropHandler`)
- 같은 종류 + 같은 등급 영웅 드래그 시 합성
- 합성 시 상위 등급 영웅 생성
- 7단계 성장 구조 (`HeroGrade` enum)

### 3. 자동 전투 시스템 (`Hero`)
- 영웅은 사거리 내 적 자동 공격
- 가장 가까운 적을 우선 타겟
- 투사체 또는 직접 데미지 방식 지원

### 4. 웨이브 시스템 (`WaveManager` + `EnemySpawner`)
- 일정 시간마다 적 생성
- 웨이브가 증가할수록 난이도 상승
- 5웨이브마다 보스 등장 (`BossEnemy`)

### 5. 특성 시스템 (`TraitSystem`)
- 영웅은 `HeroData.특성목록`에 정의된 고유 특성 보유
- 배치된 영웅들의 특성 수에 따라 시너지 효과 발생
- 공격력/공격속도 배율 보너스 적용

---

## 🏗 개발 단계

- [x] Step 1: 영웅 배치용 그리드 시스템 (`GridSystem.cs`)
- [x] Step 2: 영웅 랜덤 소환 시스템 (`GameManager.cs` + `GoldSystem.cs`)
- [x] Step 3: 드래그 앤 드롭 합성 시스템 (`DragDropHandler.cs` + `MergeManager.cs`)
- [x] Step 4: 적 웨이브 생성 로직 (`EnemySpawner.cs` + `WaveManager.cs`)
- [x] Step 5: 자동 공격 시스템 (`Hero.cs`)
- [x] Step 6: 골드 및 UI 시스템 (`GoldSystem.cs` + `UIManager.cs`)

---

## 📦 ScriptableObject 사용법

`HeroData` ScriptableObject를 생성하려면:

1. Unity 에디터에서 `Assets > Create > GMDefense > 영웅 데이터` 선택
2. 영웅 이름, 종류, 등급, 스탯, 특성 설정
3. `GameManager`의 `소환가능영웅풀`에 추가

---

## ⚙️ 설정 방법

1. `GameManager` 오브젝트에 `GameManager.cs` 부착 후 소환 가능 영웅 풀 설정
2. `GridSystem` 오브젝트에 `GridSystem.cs` 부착 후 열/행 수 설정
3. `EnemySpawner` 오브젝트에 `EnemySpawner.cs` 부착 후 경로 포인트 설정
4. `WaveManager` 오브젝트에 `WaveManager.cs` 부착
5. `TraitSystem` 오브젝트에 `TraitSystem.cs` 부착 후 시너지 설정
6. UI 캔버스에 `UIManager.cs` 부착 후 각 UI 요소 연결

---

## 📝 코드 규칙

- 모든 코드 주석, 변수명, 함수명, 클래스명 설명은 **한국어** 사용
- Clean Architecture 원칙에 따라 각 시스템을 독립적으로 구성
- 싱글톤 패턴으로 각 매니저 클래스에 전역 접근 가능
- 이벤트 기반 통신으로 시스템 간 결합도 최소화

