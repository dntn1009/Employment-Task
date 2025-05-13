using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BoardMaskRenderer : MonoBehaviour
{
    [SerializeField] private Transform quadTr;

    private float yoffset = 0.625f;
    private float wallOffset = 0.23f;
    private List<GameObject> quads = new List<GameObject>();
    public async Task CreateMaskingTemp(BoardController boardController)
    {
        foreach (var quad in quads)
        {
            Destroy(quad);
        }
        quads.Clear();

        GameObject prefab = await boardController.LoadPrefabAsync("Quad.prefab");

        for (int i = -3; i <= boardController.boardWidth + 3; i++)
        {
            for (int j = -3; j <= boardController.boardHeight + 3; j++)
            {
                if (boardController.BBDic.ContainsKey((i, j))) continue;

                float xValue = i;
                float zValue = j;
                if (i == -1 && j <= boardController.boardHeight) xValue -= wallOffset;
                if (i == boardController.boardWidth + 1 && j <= boardController.boardHeight + 1) xValue += wallOffset;

                if (j == -1 && i <= boardController.boardWidth) zValue -= wallOffset;
                if (j == boardController.boardHeight + 1 && i <= boardController.boardWidth + 1) zValue += wallOffset;

                GameObject quad = GameObject.Instantiate(prefab, quadTr);
                quads.Add(quad);

                quad.transform.position = boardController.blockDistance * new Vector3(xValue, yoffset, zValue);
            }
        }
    }


    /* public void CreateMaskingTemp(BoardController boardController)
     {
         float blockDistance = boardController.blockDistance;

         if (quad != null)
             Destroy(quad);

         // 1개만 생성
         quad = GameObject.Instantiate(quadPrefab, quadTr);

         // 보드 정확한 크기 계산
         int width = boardController.boardWidth + 1;
         int height = boardController.boardHeight + 1;

         // 스케일 적용 (기본 Quad는 1x1 이므로 보드 크기만큼 곱셈)
         quad.transform.localScale = new Vector3(width * blockDistance + + wallOffset, height * blockDistance + + wallOffset, 1);

         // 중심 좌표 계산
         float centerX = (width - 1) / 2f;
         float centerZ = (height - 1) / 2f;
         Vector3 centerOffset = new Vector3(centerX, yoffset, centerZ);

         // 위치 설정
         quad.transform.position = blockDistance * centerOffset;

         // 평면이 위를 보게 회전
         quad.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
     }*/
}