using UnityEngine;

/// <summary>
/// �������� �����͸� �����ϴ� �������� �̸����� ���� ���ҽ��� ���Ͽ� ���������� ��ȯ�ϴ� ��ũ��Ʈ�Դϴ�.
/// </summary>
public class StagePreviewBridge : MonoBehaviour
{

    private void Start()
    {
        StageData previewData = Resources.Load<StageData>("StageData/NewStageData");
        BoardController.Instance.TestLoadStage(previewData);
    }
}