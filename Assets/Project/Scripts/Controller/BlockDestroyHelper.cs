
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Helpers.Block
{
    public static class BlockDestroyCheckHelper
    {
        /// <summary>
        /// 주어진 블록이 유효한 체크 그룹에 속해 있는지 검사.
        /// </summary>
        private static bool IsValidCheckBlock(BoardBlockObject boardBlock)
        {
            foreach (var idx in boardBlock.checkGroupIdx)
            {
                if (!boardBlock.isCheckBlock && !BoardController.Instance.CBGDic.ContainsKey(idx))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 블록들의 x, y 최소/최대 범위를 계산하여 반환.
        /// </summary>
        private static void GetBlockBounds(List<BlockObject> blocks, out int minX, out int maxX, out int minY, out int maxY)
        {
            minX = BoardController.Instance.boardWidth;
            maxX = -1;
            minY = BoardController.Instance.boardHeight;
            maxY = -1;

            foreach (var b in blocks)
            {
                minX = Mathf.Min(minX, (int)b.x);
                maxX = Mathf.Max(maxX, (int)b.x);
                minY = Mathf.Min(minY, (int)b.y);
                maxY = Mathf.Max(maxY, (int)b.y);
            }
        }

        /// <summary>
        /// 현재 보드 블록이 속한 그룹을 가로/세로 방향으로 나누어 반환.
        /// </summary>
        private static (List<BoardBlockObject> horizon, List<BoardBlockObject> vertical) GetGroupedBlocks(BoardBlockObject boardBlock)
        {
            List<BoardBlockObject> hor = new List<BoardBlockObject>();
            List<BoardBlockObject> ver = new List<BoardBlockObject>();

            foreach (var checkIndex in boardBlock.checkGroupIdx)
            {
                foreach (var boardBlockObj in BoardController.Instance.CBGDic[checkIndex])
                {
                    foreach (var horizon in boardBlockObj.isHorizon)
                    {
                        if (horizon) hor.Add(boardBlockObj);
                        else ver.Add(boardBlockObj);
                    }
                }
            }

            return (hor, ver);
        }

        /// <summary>
        /// 가로 방향으로 블록이 파괴 가능한지 검사.
        /// 범위 및 색상 충돌 여부를 포함한 판정을 수행.
        /// </summary>
        private static bool CheckHorizontalDestroy(List<BoardBlockObject> group, BlockObject block, int minX, int maxX)
        {
            int groupMinX = group.Min(b => b.x);
            int groupMaxX = group.Max(b => b.x);

            if (minX < groupMinX - BoardController.Instance.blockDistance / 2 || maxX > groupMaxX + BoardController.Instance.blockDistance / 2)
                return false;

            var checkCoords = new (int, int)[group.Count];

            for (int i = 0; i < group.Count; i++)
            {
                var b = group[i];
                int maxY = int.MinValue;

                foreach (var p in block.dragHandler.blocks)
                {
                    if (p.y == b.y)
                        maxY = Mathf.Max(maxY, (int)p.y);
                }

                checkCoords[i] = (b.x, maxY);

                for (int y = checkCoords[i].Item2; y <= b.y; y++)
                {
                    if (checkCoords[i].Item1 < minX || checkCoords[i].Item1 > maxX) continue;

                    var key = (checkCoords[i].Item1, y);

                    if (BoardController.Instance.BBDic.TryGetValue(key, out var board) &&
                        board.playingBlock != null &&
                        board.playingBlock.colorType != b.horizonColorType)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 세로 방향으로 블록이 파괴 가능한지 검사.
        /// 범위 및 색상 충돌 여부를 포함한 판정을 수행.
        /// </summary>
        private static bool CheckVerticalDestroy(List<BoardBlockObject> group, BlockObject block, int minY, int maxY)
        {
            int groupMinY = group.Min(b => b.y);
            int groupMaxY = group.Max(b => b.y);

            if (minY < groupMinY - BoardController.Instance.blockDistance / 2 || maxY > groupMaxY + BoardController.Instance.blockDistance / 2)
                return false;

            var checkCoords = new (int, int)[group.Count];

            for (int i = 0; i < group.Count; i++)
            {
                var b = group[i];
                if (b.x <= BoardController.Instance.boardWidth / 2)
                {
                    int maxX = int.MinValue;

                    foreach (var p in block.dragHandler.blocks)
                    {
                        if (p.y == b.y)
                            maxX = Mathf.Max(maxX, (int)p.x);
                    }

                    checkCoords[i] = (maxX, b.y);

                    for (int x = maxX; x >= b.x; x--)
                    {
                        if (b.y < minY || b.y > maxY) continue;

                        var key = (x, b.y);

                        if (BoardController.Instance.BBDic.TryGetValue(key, out var board) &&
                            board.playingBlock != null &&
                            board.playingBlock.colorType != b.verticalColorType)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    int minX = int.MaxValue;

                    foreach (var p in block.dragHandler.blocks)
                    {
                        if (p.y == b.y)
                            minX = Mathf.Min(minX, (int)p.x);
                    }

                    checkCoords[i] = (minX, b.y);

                    for (int x = minX; x <= b.x; x++)
                    {
                        if (b.y < minY || b.y > maxY) continue;

                        var key = (x, b.y);

                        if (BoardController.Instance.BBDic.TryGetValue(key, out var board) &&
                            board.playingBlock != null &&
                            board.playingBlock.colorType != b.verticalColorType)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 외부에서 호출되는 메인 메서드.
        /// 블록이 파괴 가능한지 여부를 가로/세로 방향 및 조건에 따라 최종 판정.
        /// </summary>
        public static bool CheckCanDestroy(BoardBlockObject boardBlock, BlockObject block)
        {
            if (!IsValidCheckBlock(boardBlock))
                return false;

            GetBlockBounds(block.dragHandler.blocks, out int minX, out int maxX, out int minY, out int maxY);

            var (horizonBlocks, verticalBlocks) = GetGroupedBlocks(boardBlock);

            int matchingIndex = boardBlock.colorType.FindIndex(c => c == block.colorType);
            bool isHorizontal = boardBlock.isHorizon[matchingIndex];

            if (isHorizontal)
                return CheckHorizontalDestroy(horizonBlocks, block, minX, maxX);
            else
                return CheckVerticalDestroy(verticalBlocks, block, minY, maxY);

        }

        /// <summary>
        /// 벽 색상 머티리얼 반환용 유틸 메서드.
        /// </summary>
        public static Material GetTargetMaterial(int index)
        {
            return BoardController.Instance.WallMaterials[index];
        }
    }

    
}