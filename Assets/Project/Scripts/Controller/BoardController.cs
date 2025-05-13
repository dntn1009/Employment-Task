using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public partial class BoardController : MonoBehaviour
{
    public static BoardController Instance;
    
    [Header("Create Factory Var")] //Board 생성에 필요한 클래스
    [SerializeField] private BlockFactory blockFactory; // 블록 생성
    [SerializeField] private WallFactory wallFactory; // 벽 생성
    [SerializeField] private PlayingBlockSpawner playingBlockSpawner; // 플레이 블록 생성
    [SerializeField] private BoardMaskRenderer boardMaskRenderer;


    [SerializeField] ParticleSystem destroyParticle;
    public ParticleSystem destroyParticlePrefab => destroyParticle;
    public List<SequentialCubeParticleSpawner> particleSpawners;

    /// <summary>
    /// 블록, 벽, 플레이블록 생성에 필요한 Factory 간 의존성을 연결하기 위해
    ///  BoardController에서 관련 데이터를 중계하는 프로퍼티입니다.
    ///  각 Factory에서 직접 참조하지 않고, BoardController를 통해 필요한 정보를 전달받습니다.
    /// </summary>
    public Dictionary<(int x, int y), Dictionary<(DestroyWallDirection, ColorType), int>> GetWCIDic { get { return wallFactory.wallCoorInfoDic; } }
    public Material[] WallMaterials { get { return wallFactory.WallMaterials; } }
    public Dictionary<(int x, int y), BoardBlockObject> BBDic { get { return blockFactory.boardBlockDic; } }
    public Dictionary<int, List<BoardBlockObject>> CBGDic { get { return blockFactory.CheckBlockGroupDic; } }
    public Transform GetBoardTransform { get { return blockFactory.boardParent.transform; } }
    public Transform GetPlayingBlockTransform { get { return playingBlockSpawner.playingBlockParent.transform; } }

    //필드 캐시용 Dictionary
    private Dictionary<string, GameObject> cachedPrefabs = new Dictionary<string, GameObject>();
    
    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        LoadStage();
    }

    /// <summary>
    /// 캐싱 로직 메서드
    /// </summary>
    public async Task<GameObject> LoadPrefabAsync(string key)
    {
        if (!cachedPrefabs.TryGetValue(key, out GameObject prefab))
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(key);
            prefab = await handle.Task;

            if (prefab == null)
            {
                Debug.LogError($"Addressables: {key} 로딩 실패");
                return null;
            }

            cachedPrefabs[key] = prefab;
        }

        return cachedPrefabs[key];
    }
}