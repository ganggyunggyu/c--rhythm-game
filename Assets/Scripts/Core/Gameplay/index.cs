// Barrel export for Gameplay module
// C# doesn't have index.ts style exports, but this file documents the module structure
// All classes are accessible via: using RhythmGame.Core.Gameplay;
//
// Classes:
// - GameManager: 전체 게임 흐름 관리 (싱글톤)
// - SongController: 음악 재생/타임라인 싱크
// - NoteSpawner: 노트 생성/풀링
// - Note: 개별 노트 오브젝트
// - JudgeController: 입력 판정
// - ScoreManager: 점수/콤보/정확도 계산
// - GameState (enum): Idle, Loading, Ready, Playing, Paused, Result
// - JudgeResult (enum): None, Perfect, Great, Good, Miss
// - ResultData: 결과 데이터 클래스
