using UnityEngine;

/// <summary>
/// 스테이지 데이터를 생성하는 과정에서 미리보기 위해 리소스를 통하여 스테이지를 소환하는 스크립트입니다.
/// </summary>
public class StagePreviewBridge : MonoBehaviour
{

    private void Start()
    {
        StageData previewData = Resources.Load<StageData>("StageData/NewStageData");
        BoardController.Instance.TestLoadStage(previewData);
    }
}