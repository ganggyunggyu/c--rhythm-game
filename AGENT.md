# YouTube 리듬게임 - Unity 개발 가이드

## 프로젝트 개요

- **타입**: Unity 3D 리듬게임
- **엔진**: Unity 6000.2 (2022+ 호환)
- **언어**: C# (.NET Standard 2.1)
- **플랫폼**: Mac / Windows

YouTube URL → 오디오 추출 → BPM 분석 → 자동 차트 생성 → 4키 리듬게임

## 기술 스택

### 핵심
- Unity 6000 (LTS)
- C#
- yt-dlp (YouTube 다운로드)
- ffmpeg (오디오 변환)

### 오디오 분석
- 에너지 기반 온셋 검출
- 자체 구현 BPM 추정 알고리즘

## 디렉토리 구조

```
Assets/Scripts/
├── Core/
│   ├── Audio/           # AudioLoader
│   ├── Analysis/        # BeatAnalyzer, ChartGenerator
│   └── Gameplay/        # Note, SongController, ScoreManager
├── UI/                  # MainMenuUI, GameHUD, LoadingUI, PauseMenuUI, ReadyUI, ResultUI
├── Data/                # NoteData, ChartData, GameSettings
└── Utils/               # CacheManager, YoutubeUrlParser
```

## 네임스페이스 구조

```csharp
RhythmGame.Data           // 데이터 모델
RhythmGame.Core.Audio     // 오디오 처리
RhythmGame.Core.Analysis  // 비트 분석
RhythmGame.Core.Gameplay  // 게임플레이
RhythmGame.UI             // UI 컴포넌트
RhythmGame.Utils          // 유틸리티
```

## 핵심 클래스

### Data
| 클래스 | 용도 |
|--------|------|
| `NoteData` | 노트 정보 (time, lane, type, duration) |
| `ChartData` | 차트 정보 (오디오 경로, BPM, 노트 리스트) |
| `GameSettings` | 게임 설정 (ScriptableObject) |

### Core
| 클래스 | 용도 |
|--------|------|
| `BeatAnalyzer` | 에너지 기반 비트 검출, BPM 추정 |
| `ChartGenerator` | 비트 → 노트 패턴 변환 |
| `Note` | 노트 MonoBehaviour |
| `SongController` | 음악 재생, 타이밍 싱크 |
| `ScoreManager` | 점수/콤보/정확도 계산 |

### UI
| 클래스 | 용도 |
|--------|------|
| `MainMenuUI` | 메인 메뉴, URL 입력 |
| `LoadingUI` | 로딩 화면 |
| `GameHUD` | 게임 중 UI (점수, 콤보) |
| `PauseMenuUI` | 일시정지 |
| `ResultUI` | 결과 화면 |

## 게임플레이 상수

### 판정 시스템
| 판정 | 타이밍 오차 | 점수 |
|------|------------|------|
| PERFECT | ±50ms | 1000점 |
| GREAT | ±100ms | 800점 |
| GOOD | ±150ms | 500점 |
| MISS | >150ms | 0점 |

### 오디오 분석 파라미터
```csharp
SampleRate = 44100
WindowSize = 1024
HopSize = 512
PeakThreshold = 0.3f
MinPeakDistance = 100ms
BPM 범위 = 60~200
```

### 입력 매핑
| 키 | 레인 |
|----|------|
| D | 0 |
| F | 1 |
| J | 2 |
| K | 3 |

## 개발 규칙

### 코드 스타일
- 네임스페이스 필수 사용
- 필드는 `_camelCase` (언더스코어 prefix)
- 프로퍼티는 `PascalCase`
- Unity 직렬화 필드: `[SerializeField] private`

### 오디오 싱크
- `AudioSettings.dspTime` 기반 정밀 타이밍
- 노트 스폰: lead time 2초 전

### 캐시 구조
```
cache/{videoId}/
├── audio.wav
└── chart.json
```

## 빌드 명령어

```bash
# Unity Editor 열기
./build.sh editor

# Mac 빌드
./build.sh mac

# Windows 빌드
./build.sh windows

# 게임 실행
./build.sh run

# 빌드 정리
./build.sh clean
```

## 주의사항

- StreamingAssets에 ffmpeg, yt-dlp 바이너리 필요
- YouTube 다운로드는 개인/연구 목적만
