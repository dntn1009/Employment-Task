
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 블록 드래그와 이동 처리를 담당
/// 내부적으로 마우스 입력/충돌/아웃라인/물리 이동 등 세부 기능은 서브 클래스에게 위임
/// </summary>
public class BlockDragHandler : MonoBehaviour
{
    public int horizon = 1;
    public int vertical = 1;
    public int uniqueIndex;
    public List<ObjectPropertiesEnum.BlockGimmickType> gimmickType;
    public List<BlockObject> blocks = new List<BlockObject>();
    public List<Vector2> blockOffsets = new List<Vector2>();
    public bool Enabled = true;

    private Vector2 centerPos;
    private Rigidbody rb;
    private bool isDragging = false;
    private Camera mainCamera;

    // 서브 클래스: 충돌 감지 상태 관리
    private BlockCollisionState collisionState = new BlockCollisionState();
    // 서브 클래스: 블록 속도 계산 (충돌 유무 반영)
    private BlockMovementController movementController = new BlockMovementController();
    // 서브 클래스: 마우스 입력 관리 (z 거리, 위치 계산)
    private BlockMouseInput mouseInput = new BlockMouseInput();
    // 서브 클래스: 아웃라인 효과 관리
    private BlockOutlineController outlineController = new BlockOutlineController();

    public Collider col { get; set; }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        mainCamera = Camera.main;
        mouseInput.Init(mainCamera);
        outlineController.Init(gameObject);
    }

    void OnMouseDown()
    {
        if (!Enabled) return;

        isDragging = true;
        rb.isKinematic = false;
        outlineController.SetActive(true);

        mouseInput.BeginDrag(transform);
        collisionState.Reset();
    }

    void OnMouseUp()
    {
        isDragging = false;
        outlineController.SetActive(false);

        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        SetBlockPosition();
        collisionState.Reset();
    }

    void Update()
    {
        collisionState.UpdateCollision();
    }

    void FixedUpdate()
    {
        if (!Enabled || !isDragging) return;

        SetBlockPosition(false);

        Vector3 moveVector = mouseInput.GetTargetPosition() - transform.position;
        float distanceToMouse = Vector3.Distance(transform.position, mouseInput.GetTargetPosition());

        if (collisionState.IsColliding && distanceToMouse > 0.5f)
        {
            if (Vector3.Dot(moveVector.normalized, collisionState.CollisionNormal) > 0.1f)
                collisionState.Reset();
        }

        Vector3 velocity = movementController.CalculateVelocity(moveVector, collisionState.CollisionNormal, collisionState.IsColliding);
        if (!rb.isKinematic)
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, velocity, Time.fixedDeltaTime * 10f);
    }

    private void SetBlockPosition(bool mouseUp = true)
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 coordinate = hit.transform.position;
            Vector3 targetPos = new Vector3(coordinate.x, transform.position.y, coordinate.z);

            if (mouseUp) transform.position = targetPos;

            centerPos.x = Mathf.Round(transform.position.x / 0.79f);
            centerPos.y = Mathf.Round(transform.position.z / 0.79f);

            if (hit.collider.TryGetComponent(out BoardBlockObject boardBlockObject))
            {
                foreach (var blockObject in blocks)
                {
                    blockObject.SetCoordinate(centerPos);
                }
                foreach (var blockObject in blocks)
                {
                    boardBlockObject.CheckAdjacentBlock(blockObject, targetPos);
                    blockObject.CheckBelowBoardBlock(targetPos);
                }
            }
        }
        else
        {
            Debug.LogWarning("Nothing Detected");
        }
    }

    public void ReleaseInput()
    {
        if (col != null) col.enabled = false;
        isDragging = false;
        outlineController.SetActive(false);
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
    }

    void OnCollisionEnter(Collision collision) => collisionState.HandleCollision(collision, isDragging);
    void OnCollisionStay(Collision collision) => collisionState.HandleCollision(collision, isDragging);
    void OnCollisionExit(Collision collision) => collisionState.HandleExit(collision);

    public Vector3 GetCenterX()
    {
        if (blocks == null || blocks.Count == 0) return Vector3.zero;

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        foreach (var block in blocks)
        {
            float blockX = block.transform.position.x;
            minX = Mathf.Min(minX, blockX);
            maxX = Mathf.Max(maxX, blockX);
        }

        return new Vector3((minX + maxX) / 2f, transform.position.y, 0);
    }

    public Vector3 GetCenterZ()
    {
        if (blocks == null || blocks.Count == 0) return Vector3.zero;

        float minZ = float.MaxValue;
        float maxZ = float.MinValue;
        foreach (var block in blocks)
        {
            float blockZ = block.transform.position.z;
            minZ = Mathf.Min(minZ, blockZ);
            maxZ = Mathf.Max(maxZ, blockZ);
        }

        return new Vector3(transform.position.x, transform.position.y, (minZ + maxZ) / 2f);
    }

    private void ClearPreboardBlockObjects()
    {
        foreach (var b in blocks)
        {
            if (b.preBoardBlockObject != null)
            {
                b.preBoardBlockObject.playingBlock = null;
            }
        }
    }

    public void DestroyMove(Vector3 pos, ParticleSystem particle)
    {
        ClearPreboardBlockObjects();

        transform.DOMove(pos, 1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            Destroy(particle.gameObject);
            Destroy(gameObject);
        });
    }

    private void OnDisable() => transform.DOKill(true);
    private void OnDestroy() => transform.DOKill(true);
}