using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PlayingBlockSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnerTr;
    [SerializeField] private GameObject blockGroupPrefab;
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private Material[] testBlockMaterials;

    public GameObject playingBlockParent;
  

    public async Task CreatePlayingBlocksAsync(BoardController boardController)
    {
        float blockDistance = boardController.blockDistance;
        var stageIdx = boardController.GetStageIndex;
        var stageDatas = boardController.GetStageDatas;

        playingBlockParent = new GameObject("PlayingBlockParent");

        for (int i = 0; i < stageDatas[stageIdx].playingBlocks.Count; i++)
        {
            var pbData = stageDatas[stageIdx].playingBlocks[i];

            GameObject blockGroupObject = Instantiate(blockGroupPrefab, playingBlockParent.transform);
            blockGroupObject.transform.position = new Vector3(
                pbData.center.x * blockDistance,
                0.33f,
                pbData.center.y * blockDistance
            );

            BlockDragHandler dragHandler = blockGroupObject.GetComponent<BlockDragHandler>();
            if (dragHandler != null) dragHandler.blocks = new List<BlockObject>();

            dragHandler.uniqueIndex = pbData.uniqueIndex;
            foreach (var gimmick in pbData.gimmicks)
            {
                if (Enum.TryParse(gimmick.gimmickType, out ObjectPropertiesEnum.BlockGimmickType gimmickType))
                {
                    dragHandler.gimmickType.Add(gimmickType);
                }
            }

            int maxX = 0;
            int minX = boardController.boardWidth;
            int maxY = 0;
            int minY = boardController.boardHeight;
            foreach (var shape in pbData.shapes)
            {
                GameObject singleBlock = Instantiate(blockPrefab, blockGroupObject.transform);

                singleBlock.transform.localPosition = new Vector3(
                    shape.offset.x * blockDistance,
                    0f,
                    shape.offset.y * blockDistance
                );
                dragHandler.blockOffsets.Add(new Vector2(shape.offset.x, shape.offset.y));

                /*if (shape.colliderDirectionX > 0 && shape.colliderDirectionY > 0)
                {
                    BoxCollider collider = dragHandler.AddComponent<BoxCollider>();
                    dragHandler.col = collider;

                    Vector3 localColCenter = singleBlock.transform.localPosition;
                    int x = shape.colliderDirectionX;
                    int y = shape.colliderDirectionY;

                    collider.center = new Vector3
                        (x > 1 ? localColCenter.x + blockDistance * (x - 1)/ 2 : 0
                         ,0.2f, 
                         y > 1 ? localColCenter.z + blockDistance * (y - 1)/ 2 : 0);
                    collider.size = new Vector3(x * (blockDistance - 0.04f), 0.4f, y * (blockDistance - 0.04f));
                }*/
                var renderer = singleBlock.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null && pbData.colorType >= 0)
                {
                    renderer.material = testBlockMaterials[(int)pbData.colorType];
                }

                if (singleBlock.TryGetComponent(out BlockObject blockObj))
                {
                    blockObj.colorType = pbData.colorType;
                    blockObj.x = pbData.center.x + shape.offset.x;
                    blockObj.y = pbData.center.y + shape.offset.y;
                    blockObj.offsetToCenter = new Vector2(shape.offset.x, shape.offset.y);

                    if (dragHandler != null)
                        dragHandler.blocks.Add(blockObj);
                    boardController.BBDic[((int)blockObj.x, (int)blockObj.y)].playingBlock = blockObj;
                    blockObj.preBoardBlockObject = boardController.BBDic[((int)blockObj.x, (int)blockObj.y)];
                    if (minX > blockObj.x) minX = (int)blockObj.x;
                    if (minY > blockObj.y) minY = (int)blockObj.y;
                    if (maxX < blockObj.x) maxX = (int)blockObj.x;
                    if (maxY < blockObj.y) maxY = (int)blockObj.y;
                }
            }

            dragHandler.horizon = maxX - minX + 1;
            dragHandler.vertical = maxY - minY + 1;
        }

        await Task.Yield();
    }
}
