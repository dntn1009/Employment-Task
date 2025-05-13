using UnityEditor;
using UnityEngine;
using Project.Scripts.Data_Script;
using System.IO;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

/// <summary>
/// 새 StageData ScriptableObject를 생성하고 필드 값을 입력하고 저장
/// </summary>
public class StageDataEditor : EditorWindow
{
    private string newFileName = "NewStageData";
    private int stageIndex = 0;

    private int boardBlockSize = 10; // 사용자가 입력하는 보드 크기 값
    private List<BoardBlockData> boardBlocks = new();
    private List<PlayingBlockData> playingBlocks = new();
    private List<WallData> walls = new();

    private Vector2 scrollPos;

    [MenuItem("Tools/Stage Data Editor")]
    public static void OpenWindow()
    {
        StageDataEditor window = GetWindow<StageDataEditor>("Stage Data Editor");
        window.minSize = new Vector2(450, 500);
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("StageData 에디터", EditorStyles.boldLabel);

        stageIndex = EditorGUILayout.IntField("Stage Index", stageIndex);
        newFileName = EditorGUILayout.TextField("파일 이름", newFileName);

        GUILayout.Space(10);
        DrawBoardBlockSection();
        GUILayout.Space(10);
        DrawPlayingBlockSection();
        GUILayout.Space(10);
        DrawWallSection();
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("StageData 생성 및 저장"))
        {
            CreateAndSaveStageData(newFileName);
        }

        if (GUILayout.Button("미리보기 (Editor Play)"))
        {
            PreviewStageDataInScene();
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    // BoardBlock 구성 영역
    private void DrawBoardBlockSection()
    {
        GUILayout.Label("Board Blocks", EditorStyles.boldLabel);

        boardBlockSize = EditorGUILayout.IntField("Board Size (NxN)", boardBlockSize);
        if (GUILayout.Button("NxN 자동 생성"))
        {
            boardBlocks.Clear();
            for (int y = 0; y < boardBlockSize; y++)
            {
                for (int x = 0; x < boardBlockSize; x++)
                {
                    boardBlocks.Add(new BoardBlockData { x = x, y = y, colorType = new(), dataType = new() });
                }
            }
        }

        if (GUILayout.Button("+ BoardBlock 추가"))
            boardBlocks.Add(new BoardBlockData() { x = 0, y = 0, colorType = new(), dataType = new() });

        for (int i = 0; i < boardBlocks.Count; i++)
        {
            var b = boardBlocks[i];
            GUILayout.BeginVertical("box");
            GUILayout.Label("Block " + i);
            b.x = EditorGUILayout.IntField("X", b.x);
            b.y = EditorGUILayout.IntField("Y", b.y);
            if (GUILayout.Button("삭제"))
            {
                boardBlocks.RemoveAt(i);
                break;
            }
            GUILayout.EndVertical();
        }
    }

    // PlayingBlock 구성 영역
    private void DrawPlayingBlockSection()
    {
        GUILayout.Label("Playing Blocks", EditorStyles.boldLabel);

        if (GUILayout.Button("+ PlayingBlock 추가"))
            playingBlocks.Add(new PlayingBlockData() { center = Vector2Int.zero, shapes = new(), gimmicks = new() });

        for (int i = 0; i < playingBlocks.Count; i++)
        {
            var p = playingBlocks[i];
            GUILayout.BeginVertical("box");
            GUILayout.Label("Block " + i);
            p.center = EditorGUILayout.Vector2IntField("Center", p.center);
            p.uniqueIndex = EditorGUILayout.IntField("UniqueIndex", p.uniqueIndex);
            p.colorType = (ColorType)EditorGUILayout.EnumPopup("Color", p.colorType);

            GUILayout.Label("Shapes");
            for (int s = 0; s < p.shapes.Count; s++)
            {
                p.shapes[s].offset = EditorGUILayout.Vector2IntField("Offset " + s, p.shapes[s].offset);
            }
            if (GUILayout.Button("+ Shape 추가")) p.shapes.Add(new ShapeData());
            if (p.shapes.Count > 0 && GUILayout.Button("- Shape 삭제")) p.shapes.RemoveAt(p.shapes.Count - 1);

            GUILayout.Label("Gimmicks");
            for (int g = 0; g < p.gimmicks.Count; g++)
            {
                p.gimmicks[g].gimmickType = EditorGUILayout.TextField("Gimmick " + g, p.gimmicks[g].gimmickType);
            }
            if (GUILayout.Button("+ Gimmick 추가")) p.gimmicks.Add(new GimmickData());
            if (p.gimmicks.Count > 0 && GUILayout.Button("- Gimmick 삭제")) p.gimmicks.RemoveAt(p.gimmicks.Count - 1);

            if (GUILayout.Button("삭제"))
            {
                playingBlocks.RemoveAt(i);
                break;
            }
            GUILayout.EndVertical();
        }
    }

    // Wall 구성 영역
    private void DrawWallSection()
    {
        GUILayout.Label("Walls", EditorStyles.boldLabel);

        if (GUILayout.Button("+ Wall 추가"))
            walls.Add(new WallData());

        for (int i = 0; i < walls.Count; i++)
        {
            var w = walls[i];
            GUILayout.BeginVertical("box");
            GUILayout.Label("Wall " + i);
            w.x = EditorGUILayout.IntField("X", w.x);
            w.y = EditorGUILayout.IntField("Y", w.y);
            w.WallDirection = (ObjectPropertiesEnum.WallDirection)EditorGUILayout.EnumPopup("Direction", w.WallDirection);
            w.length = EditorGUILayout.IntField("Length", w.length);
            w.wallColor = (ColorType)EditorGUILayout.EnumPopup("Color", w.wallColor);
            w.wallGimmickType = (WallGimmickType)EditorGUILayout.EnumPopup("Gimmick", w.wallGimmickType);

            if (GUILayout.Button("삭제"))
            {
                walls.RemoveAt(i);
                break;
            }
            GUILayout.EndVertical();
        }
    }

    // 최종 StageData ScriptableObject 저장
    private void CreateAndSaveStageData(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("파일 이름이 비어 있습니다.");
            return;
        }

        StageData newData = ScriptableObject.CreateInstance<StageData>();
        newData.stageIndex = stageIndex;
        newData.boardBlocks = boardBlocks;
        newData.playingBlocks = playingBlocks;
        newData.Walls = walls;

        string folderPath = "Assets/Resources/StageData";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string assetPath = Path.Combine(folderPath, fileName + ".asset");
        AssetDatabase.CreateAsset(newData, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("StageData 생성 완료: " + assetPath);
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newData;
    }

    // 미리보기용 만들고 있던 StageData를 리소스에 저장한 후, 테스트 씬으로 이동하여 확인하는 기능
    private void PreviewStageDataInScene()
    {
        CreateAndSaveStageData(newFileName);

        // 이제 씬 열고 플레이 모드 진입
        EditorSceneManager.OpenScene("Assets/Project/Scenes/StageDataTestScene.unity");
        EditorApplication.isPlaying = true;
    }
}
