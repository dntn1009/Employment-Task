using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    [Header("StageDatas")] // �������� ������ (Scriptable Object)
    [SerializeField] private StageData[] stageDatas;

    /// <summary>
    /// ������ ������� ���� �������� ���� ������ �����Դϴ�.
    /// </summary>
    public int boardWidth;
    public int boardHeight;
    private int nowStageIndex = 0;

    /// <summary>
    /// Board Controller���� �������� �����Ϳ� �������� �ε����� �����ϴ� �����Դϴ�.
    /// </summary>
    public StageData[] GetStageDatas { get { return stageDatas; } }
    public int GetStageIndex { get { return nowStageIndex; } }

    public readonly float blockDistance = 0.79f;

    /// <summary>
    /// ���� �������� �ε����� �̿��Ͽ� �������� ��ȯ
    /// </summary>
    private async void LoadStage()
    {
        if (stageDatas == null)
        {
            Debug.LogError("StageData�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        blockFactory.boardBlockDic = new Dictionary<(int x, int y), BoardBlockObject>();
        blockFactory.CheckBlockGroupDic = new Dictionary<int, List<BoardBlockObject>>();

        blockFactory.boardParent = new GameObject("BoardParent");
        blockFactory.boardParent.transform.SetParent(transform);

        await wallFactory.CreateCustomWalls(this);

        await blockFactory.CreateBoardAsync(this);

        await playingBlockSpawner.CreatePlayingBlocksAsync(this);

        await boardMaskRenderer.CreateMaskingTemp(this);

        StartCoroutine(Wait());
    }

    /// <summary>
    /// �������� �����͸� �����ϴ� �������� �̸� ���� ���� �޼���
    /// </summary>
    public async void TestLoadStage(StageData stageData)
    {
        stageDatas[0] = stageData;
        nowStageIndex = 0;

        blockFactory.boardBlockDic = new Dictionary<(int x, int y), BoardBlockObject>();
        blockFactory.CheckBlockGroupDic = new Dictionary<int, List<BoardBlockObject>>();

        blockFactory.boardParent = new GameObject("BoardParent");
        blockFactory.boardParent.transform.SetParent(transform);

        await wallFactory.CreateCustomWalls(this);

        await blockFactory.CreateBoardAsync(this);

        await playingBlockSpawner.CreatePlayingBlocksAsync(this);

        await boardMaskRenderer.CreateMaskingTemp(this);

        StartCoroutine(Wait());
    }

    public void GoToPreviousLevel()
    {
        if (nowStageIndex == 0) return;

        Destroy(GetBoardTransform.gameObject);
        Destroy(GetPlayingBlockTransform.gameObject);
        --nowStageIndex;
        LoadStage();
    }

    public void GotoNextLevel()
    {
        if (nowStageIndex == stageDatas.Length - 1) return;

        Destroy(GetBoardTransform.gameObject);
        Destroy(GetPlayingBlockTransform.gameObject);
        ++nowStageIndex;
        LoadStage();
    }
    IEnumerator Wait()
    {
        yield return null;

        Vector3 camTr = Camera.main.transform.position;
        Camera.main.transform.position = new Vector3(1.5f + 0.5f * (boardWidth - 4), camTr.y, camTr.z);
    }
}