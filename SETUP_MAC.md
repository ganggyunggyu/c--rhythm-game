# Mac 개발 환경 설정 가이드

YouTube 기반 리듬게임 개발을 위한 Mac 환경 설정 가이드 (Homebrew 기준)

## 1. 사전 준비

### Homebrew 설치 (없는 경우)

```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

설치 후 터미널 재시작 또는:

```bash
eval "$(/opt/homebrew/bin/brew shellenv)"
```

---

## 2. Unity Hub 설치

### 방법 1: Homebrew Cask (권장)

```bash
brew install --cask unity-hub
```

### 방법 2: 공식 사이트

https://unity.com/download 에서 직접 다운로드

### Unity Editor 설치

1. Unity Hub 실행
2. 좌측 "Installs" 클릭
3. "Install Editor" 클릭
4. **Unity 2022.3 LTS** 선택 (권장)
5. 모듈 선택:
   - ✅ Mac Build Support (IL2CPP)
   - ✅ Windows Build Support (Mono) - Windows 빌드 필요시
   - ✅ Documentation

```bash
# 또는 CLI로 설치 (Unity Hub 설치 후)
# Unity Hub에서 라이센스 활성화 필요
```

---

## 3. .NET SDK 설치

Unity는 자체 Mono 런타임을 사용하지만, 에디터 도구나 외부 스크립트용으로 .NET SDK가 필요할 수 있음.

```bash
# .NET 8 SDK 설치 (최신 LTS)
brew install dotnet@8

# PATH 설정 (zsh 기준)
echo 'export PATH="/opt/homebrew/opt/dotnet@8/bin:$PATH"' >> ~/.zshrc
source ~/.zshrc

# 설치 확인
dotnet --version
```

---

## 4. ffmpeg 설치 (오디오 변환용)

```bash
brew install ffmpeg
```

### 프로젝트에 ffmpeg 바이너리 복사

Unity 빌드에 ffmpeg를 포함시키려면 StreamingAssets에 바이너리 복사:

```bash
# ffmpeg 바이너리 위치 확인
which ffmpeg

# 프로젝트에 복사 (Mac용)
mkdir -p Assets/StreamingAssets/ffmpeg
cp $(which ffmpeg) Assets/StreamingAssets/ffmpeg/ffmpeg

# 실행 권한 확인
chmod +x Assets/StreamingAssets/ffmpeg/ffmpeg
```

---

## 5. yt-dlp 설치 (YouTube 다운로드용)

```bash
brew install yt-dlp
```

### 프로젝트에 복사

```bash
cp $(which yt-dlp) Assets/StreamingAssets/ffmpeg/yt-dlp
chmod +x Assets/StreamingAssets/ffmpeg/yt-dlp
```

---

## 6. IDE 설정

### Visual Studio Code (권장)

```bash
brew install --cask visual-studio-code
```

#### C# 확장 설치

```bash
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.csdevkit
```

### JetBrains Rider (유료, 강력 추천)

```bash
brew install --cask rider
```

### Visual Studio for Mac

```bash
brew install --cask visual-studio
```

---

## 7. Unity에서 프로젝트 열기

### 새 프로젝트 생성

1. Unity Hub 실행
2. "Projects" → "New Project"
3. **"2D (Built-in Render Pipeline)"** 선택
4. 프로젝트 이름: `c#-rhythm-game`
5. 위치: `/Users/ganggyunggyu/Programing/game/`
6. "Create project" 클릭

### 기존 스크립트 적용

이 레포지토리의 `Assets/Scripts/` 폴더가 Unity 프로젝트의 Assets 폴더에 있어야 함.

```bash
# 프로젝트 위치로 이동
cd /Users/ganggyunggyu/Programing/game/c#-rhythm-game

# Unity에서 이 폴더를 프로젝트로 열기
```

---

## 8. Unity 외부 도구(IDE) 연결

1. Unity Editor 실행
2. **Edit → Preferences** (macOS: Unity → Settings)
3. **External Tools** 탭
4. **External Script Editor** 선택:
   - Visual Studio Code
   - 또는 Rider

---

## 9. 필수 Unity 패키지

Unity Package Manager에서 설치 (Window → Package Manager):

| 패키지 | 용도 |
|--------|------|
| TextMeshPro | UI 텍스트 (필수) |
| Input System | 새로운 입력 시스템 (선택) |

### Package Manager CLI (선택)

`Packages/manifest.json`에 직접 추가:

```json
{
  "dependencies": {
    "com.unity.textmeshpro": "3.0.6",
    "com.unity.inputsystem": "1.7.0"
  }
}
```

---

## 10. NuGet 패키지 (YoutubeExplode)

Unity에서 NuGet 패키지를 사용하려면 `NuGetForUnity` 에셋 사용 권장.

### 설치 방법

1. Unity Asset Store에서 **NuGetForUnity** 검색 후 설치
2. 또는 GitHub에서 unitypackage 다운로드:
   https://github.com/GlitchEnzo/NuGetForUnity

### YoutubeExplode 설치

1. NuGet → Manage NuGet Packages
2. "YoutubeExplode" 검색
3. 설치

또는 수동 설치:
```bash
# .NET 프로젝트에서 DLL 추출 후 Unity Plugins 폴더에 복사
dotnet new console -n TempProject
cd TempProject
dotnet add package YoutubeExplode
dotnet publish -c Release

# 생성된 DLL 파일들을 Unity의 Assets/Plugins/ 폴더로 복사
```

---

## 11. 빌드 및 실행

### Mac 빌드

1. **File → Build Settings**
2. **Platform**: Mac
3. **Build** 클릭

### Windows 빌드 (Mac에서)

1. Unity Hub에서 Windows Build Support 모듈 설치 필요
2. **File → Build Settings**
3. **Platform**: PC, Mac & Linux Standalone
4. **Target Platform**: Windows
5. **Build** 클릭

---

## 12. 트러블슈팅

### ffmpeg 실행 권한 문제

```bash
# Mac Gatekeeper 우회
xattr -d com.apple.quarantine Assets/StreamingAssets/ffmpeg/ffmpeg
xattr -d com.apple.quarantine Assets/StreamingAssets/ffmpeg/yt-dlp
```

### Unity에서 스크립트 인식 안됨

1. Unity 재시작
2. 또는: **Assets → Reimport All**

### .csproj 파일 생성 안됨

1. **Edit → Preferences → External Tools**
2. **Regenerate project files** 클릭

### YoutubeExplode DLL 관련 에러

- .NET Standard 2.1 타겟인지 확인
- Player Settings → Api Compatibility Level → .NET Standard 2.1

---

## 전체 명령어 요약

```bash
# 1. Homebrew 설치
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# 2. 필수 도구 설치
brew install --cask unity-hub
brew install dotnet@8
brew install ffmpeg
brew install yt-dlp

# 3. IDE 설치 (택1)
brew install --cask visual-studio-code
# 또는
brew install --cask rider

# 4. VS Code 확장 (VS Code 선택시)
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.csdevkit

# 5. ffmpeg/yt-dlp 프로젝트에 복사
mkdir -p Assets/StreamingAssets/ffmpeg
cp $(which ffmpeg) Assets/StreamingAssets/ffmpeg/ffmpeg
cp $(which yt-dlp) Assets/StreamingAssets/ffmpeg/yt-dlp
chmod +x Assets/StreamingAssets/ffmpeg/*
xattr -d com.apple.quarantine Assets/StreamingAssets/ffmpeg/*

# 6. Unity Hub에서 프로젝트 열기
```

---

## 다음 단계

1. Unity Hub에서 프로젝트 열기
2. Prefabs 폴더에 Note 프리팹 생성
3. Scene에 GameManager, NoteSpawner 등 배치
4. UI Canvas 구성
5. 테스트 실행!
