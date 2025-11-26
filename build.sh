#!/bin/bash

# Unity Editor 경로
UNITY_PATH="/Applications/Unity/Hub/Editor/6000.2.13f1-arm64/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="/Users/ganggyunggyu/Programing/game/c#-rhythm-game"

# 색상
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${YELLOW}========================================${NC}"
echo -e "${YELLOW}  YouTube 리듬게임 빌드 스크립트${NC}"
echo -e "${YELLOW}========================================${NC}"

# Unity 존재 확인
if [ ! -f "$UNITY_PATH" ]; then
    echo -e "${RED}Unity를 찾을 수 없습니다: $UNITY_PATH${NC}"
    echo "Unity Hub에서 Unity 6000.2.13f1을 설치해주세요."
    exit 1
fi

case "$1" in
    mac)
        echo -e "${GREEN}Mac 빌드 시작...${NC}"
        "$UNITY_PATH" \
            -batchmode \
            -nographics \
            -projectPath "$PROJECT_PATH" \
            -executeMethod BuildScript.BuildMacCLI \
            -logFile - \
            -quit

        if [ -d "$PROJECT_PATH/Builds/Mac/RhythmGame.app" ]; then
            echo -e "${GREEN}빌드 성공!${NC}"
            echo "실행: open $PROJECT_PATH/Builds/Mac/RhythmGame.app"
        else
            echo -e "${RED}빌드 실패${NC}"
            exit 1
        fi
        ;;

    windows)
        echo -e "${GREEN}Windows 빌드 시작...${NC}"
        "$UNITY_PATH" \
            -batchmode \
            -nographics \
            -projectPath "$PROJECT_PATH" \
            -executeMethod BuildScript.BuildWindowsCLI \
            -logFile - \
            -quit

        if [ -f "$PROJECT_PATH/Builds/Windows/RhythmGame.exe" ]; then
            echo -e "${GREEN}빌드 성공!${NC}"
        else
            echo -e "${RED}빌드 실패${NC}"
            exit 1
        fi
        ;;

    run)
        echo -e "${GREEN}게임 실행 중...${NC}"
        if [ -d "$PROJECT_PATH/Builds/Mac/RhythmGame.app" ]; then
            open "$PROJECT_PATH/Builds/Mac/RhythmGame.app"
        else
            echo -e "${RED}빌드된 게임이 없습니다. 먼저 빌드하세요:${NC}"
            echo "./build.sh mac"
            exit 1
        fi
        ;;

    editor)
        echo -e "${GREEN}Unity Editor 실행 중...${NC}"
        "$UNITY_PATH" -projectPath "$PROJECT_PATH" &
        ;;

    clean)
        echo -e "${YELLOW}빌드 폴더 정리 중...${NC}"
        rm -rf "$PROJECT_PATH/Builds"
        echo -e "${GREEN}완료!${NC}"
        ;;

    *)
        echo "사용법: ./build.sh [명령]"
        echo ""
        echo "명령어:"
        echo "  mac      - Mac용 빌드"
        echo "  windows  - Windows용 빌드"
        echo "  run      - 빌드된 게임 실행"
        echo "  editor   - Unity Editor 열기"
        echo "  clean    - 빌드 폴더 삭제"
        echo ""
        echo "예시:"
        echo "  ./build.sh mac     # Mac 빌드"
        echo "  ./build.sh run     # 게임 실행"
        ;;
esac
