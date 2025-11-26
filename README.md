# YouTube 기반 리듬게임 🎵

YouTube 링크를 입력하면 자동으로 리듬 패턴을 생성해서 플레이할 수 있는 PC용 리듬게임

## 기능

- YouTube URL 입력 → 오디오 추출 → 자동 차트 생성
- 에너지 기반 BPM/비트 분석
- 난이도별 노트 패턴 생성 (Easy/Normal/Hard)
- 4키 리듬게임 (D, F, J, K)
- 판정 시스템 (Perfect/Great/Good/Miss)
- 콤보/정확도/랭크 시스템

## 기술 스택

- Unity 6000 (2022+ 호환)
- C#
- ffmpeg (오디오 변환)
- yt-dlp (YouTube 다운로드)

## 빠른 시작

```bash
# Unity Editor 열기
./build.sh editor

# Mac 빌드
./build.sh mac

# 게임 실행
./build.sh run
```

## 프로젝트 구조

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── Audio/        # YouTube 다운로드, 오디오 로딩
│   │   ├── Analysis/     # 비트 분석, 차트 생성
│   │   └── Gameplay/     # 게임 로직
│   ├── UI/               # UI 컴포넌트
│   ├── Data/             # 데이터 모델
│   └── Utils/            # 유틸리티
├── Editor/               # 빌드 스크립트
└── StreamingAssets/
    └── ffmpeg/           # ffmpeg, yt-dlp 바이너리
```

## 설정 방법

### 1. Unity에서 프로젝트 열기

```bash
./build.sh editor
```

### 2. Scene 생성

1. File > New Scene
2. 다음 오브젝트 추가:
   - GameManager (Empty GameObject)
   - AudioSource
   - NoteSpawner
   - UI Canvas

### 3. GameSettings 생성

1. Project 창에서 우클릭
2. Create > RhythmGame > GameSettings
3. 값 설정

## 조작법

| 키 | 레인 |
|----|------|
| D | 1번 레인 |
| F | 2번 레인 |
| J | 3번 레인 |
| K | 4번 레인 |
| ESC | 일시정지 |

## 판정 기준

| 판정 | 타이밍 오차 | 점수 |
|------|------------|------|
| PERFECT | ±50ms | 1000점 |
| GREAT | ±100ms | 800점 |
| GOOD | ±150ms | 500점 |
| MISS | >150ms | 0점 |

## 주의사항

- YouTube 다운로드는 개인 용도/연구 목적으로만 사용하세요
- 저작권이 있는 음악의 상업적 사용은 법적 문제가 될 수 있습니다

## 라이선스

MIT License
