# 코드 개선점 분석 보고서

> 분석일: 2025-11-26
> 분석 대상: Unity C# 리듬 게임 프로젝트 (Assets/Scripts 전체)

## 요약

- 🔴 Critical: 2건
- 🟠 High: 6건
- 🟡 Medium: 7건
- 🟢 Low: 5건

---

## 🔴 Critical Issues

### [CRIT-001] ProcessRunner 스트림 읽기 경합 조건

**위치**: [ProcessRunner.cs:55-66](Assets/Scripts/Core/Audio/Utilities/ProcessRunner.cs#L55-L66)

**문제**:
`BeginOutputReadLine()` 또는 `BeginErrorReadLine()` 호출 후에 `ReadToEndAsync()`를 호출하면 **InvalidOperationException**이 발생한다. 비동기 읽기가 이미 시작된 상태에서 동기 읽기를 시도하기 때문.

**현재 코드**:
```csharp
if (onOutput != null)
    process.BeginOutputReadLine();

if (onError != null)
    process.BeginErrorReadLine();

await process.WaitForExitAsync();

// 문제: 위에서 BeginOutputReadLine()을 호출했으면 ReadToEndAsync() 호출 불가
var output = onOutput == null ? await process.StandardOutput.ReadToEndAsync() : string.Empty;
var error = onError == null ? await process.StandardError.ReadToEndAsync() : string.Empty;
```

**영향**:
- 콜백 함수를 전달한 경우 output/error가 항상 빈 문자열
- 콜백 없이 호출 시 정상 동작하지만, 나중에 콜백 추가하면 버그 발생

**해결 방안**:
```csharp
var outputBuilder = new StringBuilder();
var errorBuilder = new StringBuilder();

process.OutputDataReceived += (_, e) =>
{
    if (!string.IsNullOrEmpty(e.Data))
    {
        outputBuilder.AppendLine(e.Data);
        onOutput?.Invoke(e.Data);
    }
};

process.ErrorDataReceived += (_, e) =>
{
    if (!string.IsNullOrEmpty(e.Data))
    {
        errorBuilder.AppendLine(e.Data);
        onError?.Invoke(e.Data);
    }
};

process.Start();
process.BeginOutputReadLine();
process.BeginErrorReadLine();

await process.WaitForExitAsync();

return new ProcessResult
{
    ExitCode = process.ExitCode,
    Output = outputBuilder.ToString().Trim(),
    Error = errorBuilder.ToString().Trim()
};
```

**검증 방법**:
- yt-dlp, ffmpeg 호출 시 output/error 값이 정상적으로 캡처되는지 확인

---

### [CRIT-002] JudgeController에서 Collection 순회 중 수정

**위치**: [JudgeController.cs:96-117](Assets/Scripts/Core/Gameplay/JudgeController.cs#L96-L117)

**문제**:
`_noteSpawner.ActiveNotes`를 `foreach`로 순회하면서 `ReturnToPool()`을 호출하여 리스트를 수정한다. 이는 **InvalidOperationException**을 발생시킬 수 있다.

**현재 코드**:
```csharp
private void CheckMissedNotes()
{
    var currentTime = _songController.SongTime;
    var notesToRemove = new List<Note>();  // 임시 리스트 사용

    foreach (var note in _noteSpawner.ActiveNotes)
    {
        // ...
        notesToRemove.Add(note);
    }

    foreach (var note in notesToRemove)  // 별도 루프에서 제거
    {
        _noteSpawner.ReturnToPool(note);
    }
}
```

**영향**:
- 현재는 임시 리스트로 우회했지만, `TryJudgeLane` 메서드에서는 직접 `ReturnToPool` 호출
- 다른 스레드/코루틴에서 ActiveNotes 접근 시 문제 발생 가능

**해결 방안**:
```csharp
// NoteSpawner.cs에서 스레드 안전한 방식 제공
public void ProcessNotes(Func<Note, bool> shouldRemove)
{
    for (int i = _activeNotes.Count - 1; i >= 0; i--)
    {
        if (shouldRemove(_activeNotes[i]))
        {
            var note = _activeNotes[i];
            note.Deactivate();
            _activeNotes.RemoveAt(i);
            _notePool.Enqueue(note);
        }
    }
}
```

**검증 방법**:
- 많은 노트가 동시에 미스되는 상황에서 예외 발생 여부 확인

---

## 🟠 High Priority Issues

### [HIGH-001] NoteData.type 문자열 하드코딩

**위치**: [NoteData.cs:10](Assets/Scripts/Data/NoteData.cs#L10)

**문제**:
노트 타입이 문자열("tap", "hold")로 정의되어 있어 타입 안전성이 없고, 오타 시 런타임 오류 발생.

**현재 코드**:
```csharp
public string type;  // "tap", "hold"
```

**영향**:
- `Note.cs:22`, `ChartGenerator.cs:92,155-157`에서 매직 문자열 비교
- 오타 시 컴파일 에러 없이 버그 발생

**해결 방안**:
```csharp
// 별도 파일 또는 NoteData.cs 상단에 정의
public enum NoteType
{
    Tap,
    Hold
}

public class NoteData
{
    public NoteType type;
    // ...
}
```

---

### [HIGH-002] ChartLoadingService의 async void 사용

**위치**: [ChartLoadingService.cs:58](Assets/Scripts/Core/Gameplay/ChartLoadingService.cs#L58)

**문제**:
`async void`는 예외 처리가 어렵고, 예외 발생 시 앱 크래시 가능성.

**현재 코드**:
```csharp
public async void LoadFromYoutube(string url, Difficulty difficulty = Difficulty.Normal)
```

**영향**:
- try-catch로 감싸도 await 이후 예외는 잡히지 않을 수 있음
- 테스트 시 완료 시점 파악 어려움

**해결 방안**:
```csharp
public async Task LoadFromYoutubeAsync(string url, Difficulty difficulty = Difficulty.Normal)
{
    // ...
}

// 호출부 (Unity 이벤트 등에서)
public void LoadFromYoutube(string url, Difficulty difficulty = Difficulty.Normal)
{
    _ = LoadFromYoutubeAsync(url, difficulty);
}
```

---

### [HIGH-003] CacheManager 예외 처리 누락

**위치**: [CacheManager.cs:45-56](Assets/Scripts/Utils/CacheManager.cs#L45-L56)

**문제**:
`Directory.Delete()`는 파일이 사용중이거나 권한 문제 시 예외를 던지지만 처리하지 않음.

**현재 코드**:
```csharp
public static void ClearCache(string videoId)
{
    var folder = GetVideoFolder(videoId);
    if (Directory.Exists(folder))
        Directory.Delete(folder, true);  // 예외 발생 가능
}
```

**해결 방안**:
```csharp
public static bool TryClearCache(string videoId, out string error)
{
    error = null;
    try
    {
        var folder = Path.Combine(CacheRoot, videoId);
        if (Directory.Exists(folder))
            Directory.Delete(folder, true);
        return true;
    }
    catch (Exception ex)
    {
        error = ex.Message;
        return false;
    }
}
```

---

### [HIGH-004] GameManager 이벤트 구독 해제 누락

**위치**: [GameManager.cs:69-72](Assets/Scripts/Core/Gameplay/GameManager.cs#L69-L72)

**문제**:
`SetupEventHandlers()`에서 `_loadingService.OnChartLoaded`에 구독하지만, `OnDestroy`에서 해제하지 않음.

**현재 코드**:
```csharp
private void SetupEventHandlers()
{
    _loadingService.OnChartLoaded += OnChartLoaded;  // 구독만 함
}
// OnDestroy 없음
```

**영향**:
- Singleton이라 큰 문제는 아니지만, Scene 재로드 시 메모리 누수 가능
- DontDestroyOnLoad와 일반 오브젝트 간 참조 문제

**해결 방안**:
```csharp
private void OnDestroy()
{
    if (_loadingService != null)
        _loadingService.OnChartLoaded -= OnChartLoaded;
}
```

---

### [HIGH-005] UI 컴포넌트 null 체크 누락

**위치**: [PauseMenuUI.cs:15-17](Assets/Scripts/UI/PauseMenuUI.cs#L15-L17)

**문제**:
SerializeField로 연결된 버튼들에 대해 null 체크 없이 바로 이벤트 등록.

**현재 코드**:
```csharp
private void Start()
{
    _resumeButton.onClick.AddListener(OnResumeClicked);  // null이면 크래시
    _restartButton.onClick.AddListener(OnRestartClicked);
    _quitButton.onClick.AddListener(OnQuitClicked);
}
```

**영향**:
- Inspector에서 연결 누락 시 NullReferenceException
- 빌드 후 발견 시 수정 어려움

**해결 방안**:
```csharp
private void Start()
{
    _resumeButton?.onClick.AddListener(OnResumeClicked);
    _restartButton?.onClick.AddListener(OnRestartClicked);
    _quitButton?.onClick.AddListener(OnQuitClicked);

    // 또는 Assert로 개발 중 발견
    Debug.Assert(_resumeButton != null, "Resume 버튼이 연결되지 않았습니다");
}
```

---

### [HIGH-006] LoadingUI의 메모리 누수

**위치**: [LoadingUI.cs:14-29](Assets/Scripts/UI/LoadingUI.cs#L14-L29)

**문제**:
`Start()`에서 GameManager.Instance에 이벤트 구독하고 `OnDestroy()`에서 해제하지만, `GameManager.Instance`가 null이 되는 시점에 따라 해제 실패 가능.

**현재 코드**:
```csharp
private void OnDestroy()
{
    if (GameManager.Instance != null)  // DontDestroyOnLoad라 null일 수 있음
    {
        GameManager.Instance.OnLoadProgressChanged -= UpdateProgress;
    }
}
```

**영향**:
- Scene 전환 시 이벤트 핸들러 누적 가능

**해결 방안**:
```csharp
private GameManager _cachedGameManager;

private void Start()
{
    _cachedGameManager = GameManager.Instance;
    if (_cachedGameManager != null)
    {
        _cachedGameManager.OnLoadProgressChanged += UpdateProgress;
    }
}

private void OnDestroy()
{
    if (_cachedGameManager != null)
    {
        _cachedGameManager.OnLoadProgressChanged -= UpdateProgress;
    }
}
```

---

## 🟡 Medium Priority Issues

### [MED-001] DifficultyConfig가 private nested class

**위치**: [ChartGenerator.cs:172-178](Assets/Scripts/Core/Analysis/ChartGenerator.cs#L172-L178)

**문제**:
난이도 설정이 ChartGenerator 내부에 하드코딩되어 외부에서 커스터마이징 불가.

**해결 방안**:
```csharp
// 별도 파일: DifficultyConfig.cs
[CreateAssetMenu(fileName = "DifficultyConfig", menuName = "RhythmGame/DifficultyConfig")]
public class DifficultyConfig : ScriptableObject
{
    public float noteDensity = 0.75f;
    public int maxSimultaneousNotes = 2;
    public float holdNoteChance = 0.1f;
    public int patternComplexity = 2;
}
```

---

### [MED-002] GameSettings 기본값 패턴 반복

**위치**: 여러 파일에서 반복

**문제**:
`_settings?.value ?? defaultValue` 패턴이 여러 곳에서 반복됨.

**현재 코드**:
```csharp
// JudgeController.cs
var perfectWindow = _settings?.perfectWindow ?? 50f;
var greatWindow = _settings?.greatWindow ?? 100f;

// ChartLoadingService.cs
new ChartGenerator(_settings?.laneCount ?? 4);
```

**해결 방안**:
GameSettings에 static default 인스턴스 제공:
```csharp
public class GameSettings : ScriptableObject
{
    private static GameSettings _default;
    public static GameSettings Default => _default ??= CreateDefaultSettings();

    private static GameSettings CreateDefaultSettings()
    {
        var settings = CreateInstance<GameSettings>();
        // 기본값 설정
        return settings;
    }
}
```

---

### [MED-003] UI 클래스 이벤트 패턴 중복

**위치**: LoadingUI, GameHUD, PauseMenuUI, ResultUI, ReadyUI 등

**문제**:
모든 UI 클래스가 동일한 이벤트 등록/해제 패턴을 반복.

**해결 방안**:
기본 클래스 추출:
```csharp
public abstract class GameStateUI : MonoBehaviour
{
    protected virtual GameState ActiveState => GameState.Idle;

    protected virtual void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnGameStateChanged;
    }

    protected virtual void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
    }

    protected virtual void OnGameStateChanged(GameState state)
    {
        gameObject.SetActive(state == ActiveState);
    }
}
```

---

### [MED-004] index.cs 파일들의 용도 불분명

**위치**: 각 폴더의 index.cs 파일들

**문제**:
Assets/Scripts 하위 폴더에 index.cs 파일들이 존재하지만 용도가 불분명.

**해결 방안**:
- 불필요하면 삭제
- 네임스페이스 문서화용이면 README.md로 대체
- asmdef 파일로 어셈블리 분리 필요 시 그쪽으로 이동

---

### [MED-005] SongController.LoadAudio null 체크

**위치**: [SongController.cs:34-39](Assets/Scripts/Core/Gameplay/SongController.cs#L34-L39)

**문제**:
`LoadAudio()`에서 null clip도 그대로 설정됨.

**현재 코드**:
```csharp
public void LoadAudio(AudioClip clip)
{
    _audioSource.clip = clip;  // null이어도 설정
    _pausedTime = 0f;
    _isPlaying = false;
}
```

**해결 방안**:
```csharp
public bool LoadAudio(AudioClip clip)
{
    if (clip == null)
    {
        Debug.LogWarning("AudioClip이 null입니다");
        return false;
    }

    _audioSource.clip = clip;
    _pausedTime = 0f;
    _isPlaying = false;
    return true;
}
```

---

### [MED-006] YoutubeAudioDownloader가 MonoBehaviour 아님

**위치**: [YoutubeAudioDownloader.cs](Assets/Scripts/Core/Audio/YoutubeAudioDownloader.cs)

**문제**:
일반 C# 클래스지만 Unity의 Debug.Log 사용. 일관성 문제.

**해결 방안**:
- MonoBehaviour로 변환하고 코루틴 사용, 또는
- ILogger 인터페이스 주입하여 테스트 가능하게 변경

---

### [MED-007] BeatAnalyzer 경계값 처리

**위치**: [BeatAnalyzer.cs:110-126](Assets/Scripts/Core/Analysis/BeatAnalyzer.cs#L110-L126)

**문제**:
`EstimateBpm`에서 빈 intervals 리스트나 0 간격 처리가 불완전.

**현재 코드**:
```csharp
private float EstimateBpm(List<float> peaks, float totalDuration)
{
    if (peaks.Count < 2)
        return 120f;  // 기본값

    // intervals가 비어있을 수 있음
    intervals.Sort();
    var medianInterval = intervals[intervals.Count / 2];  // 빈 리스트면 크래시
```

**해결 방안**:
```csharp
if (intervals.Count == 0)
    return 120f;
```

---

## 🟢 Low Priority Issues

### [LOW-001] ResultData 클래스 위치

**위치**: [ScoreManager.cs:133-144](Assets/Scripts/Core/Gameplay/ScoreManager.cs#L133-L144)

**문제**:
`ResultData` 클래스가 ScoreManager.cs 파일 안에 정의됨.

**해결 방안**:
별도 파일 `Assets/Scripts/Data/ResultData.cs`로 분리.

---

### [LOW-002] 매직 넘버 상수화

**위치**: 여러 파일

**문제**:
하드코딩된 숫자들이 의미 파악 어려움.

```csharp
// BeatAnalyzer.cs
private const int SampleRate = 44100;  // OK
// 하지만...
var threshold = 0.3f;  // 매직 넘버
var minPeakDistance = (int)(SampleRate * 0.1f / HopSize);  // 0.1f가 뭐지?

// NoteSpawner.cs
InitializePool(50);  // 왜 50?
```

**해결 방안**:
상수로 추출하고 이름에 의미 부여:
```csharp
private const float ENERGY_THRESHOLD = 0.3f;
private const float MIN_PEAK_DISTANCE_SECONDS = 0.1f;
private const int INITIAL_NOTE_POOL_SIZE = 50;
```

---

### [LOW-003] Debug.Log 조건부 컴파일

**위치**: 전체 프로젝트

**문제**:
릴리즈 빌드에서도 Debug.Log가 실행되어 성능 저하.

**해결 방안**:
```csharp
// Logger.cs
public static class Logger
{
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Log(string message) => Debug.Log(message);

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogWarning(string message) => Debug.LogWarning(message);
}
```

---

### [LOW-004] 네이밍 비일관성

**위치**: 전체 프로젝트

**문제**:
private 필드는 `_camelCase`, public 프로퍼티는 `PascalCase` 규칙이 대체로 지켜지지만 일부 불일치.

```csharp
// ChartGenerator.cs
private readonly int _laneCount;  // OK

// GameSettings.cs
public float perfectWindow = 50f;  // public 필드인데 camelCase
```

**해결 방안**:
Unity SerializeField는 camelCase 허용하되, public 프로퍼티는 PascalCase 래퍼 제공.

---

### [LOW-005] ExternalToolResolver 플랫폼 지원

**위치**: [ExternalToolResolver.cs:16-22](Assets/Scripts/Core/Audio/Utilities/ExternalToolResolver.cs#L16-L22)

**문제**:
Windows와 "기타"만 구분. macOS에서 실행 파일 권한 문제 가능.

**해결 방안**:
```csharp
private static string GetPlatformFileName(string toolName)
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    return $"{toolName}.exe";
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    return toolName;  // chmod +x 필요할 수 있음
#else
    return toolName;
#endif
}

public static void EnsureExecutable(string toolName)
{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    var path = GetToolPath(toolName);
    // chmod +x 실행
#endif
}
```

---

## 개선 로드맵

### Phase 1: 긴급 수정 (Critical + High) ✅ 완료
1. [x] CRIT-001: ProcessRunner 스트림 읽기 버그 수정
2. [x] CRIT-002: Collection 순회 중 수정 문제 해결
3. [x] HIGH-001: NoteType enum 도입
4. [x] HIGH-002: async void → async Task 변경
5. [x] HIGH-003: CacheManager 예외 처리 추가
6. [x] HIGH-004: GameManager 이벤트 해제 추가
7. [x] HIGH-005: UI null 체크 추가
8. [x] HIGH-006: UI 이벤트 메모리 누수 수정

### Phase 2: 품질 개선 (Medium) ✅ 완료
1. [x] MED-001: DifficultyConfig ScriptableObject화
2. [x] MED-002: GameSettings 기본값 패턴 통일
3. [x] MED-003: GameStateUI 기본 클래스 추출
4. [x] MED-004: index.cs 파일 정리
5. [x] MED-005: SongController null 체크
6. [x] MED-006: YoutubeAudioDownloader 구조 개선 (스킵 - 현재 구조 적합)
7. [x] MED-007: BeatAnalyzer 경계값 처리

### Phase 3: 리팩토링 (Low)
1. [ ] LOW-001: ResultData 파일 분리
2. [ ] LOW-002: 매직 넘버 상수화
3. [ ] LOW-003: 조건부 로깅 도입
4. [ ] LOW-004: 네이밍 컨벤션 통일
5. [ ] LOW-005: 크로스 플랫폼 도구 지원 개선

---

## 참고 사항

### 분석 방법론
- 정적 코드 분석 (파일별 수동 검토)
- Unity/C# 모범 사례 기준 적용
- 리듬 게임 특성 고려 (타이밍 정밀도, 프레임 일관성)

### 추가 권장 사항
1. **단위 테스트 도입**: 특히 BeatAnalyzer, ChartGenerator, YoutubeUrlParser
2. **어셈블리 분리**: Core, UI, Data를 별도 asmdef로 분리하여 컴파일 시간 단축
3. **프로파일링**: Note 스폰/업데이트 최적화를 위한 Unity Profiler 분석
4. **CI/CD 파이프라인**: 빌드 자동화 및 린트 검사 도입
