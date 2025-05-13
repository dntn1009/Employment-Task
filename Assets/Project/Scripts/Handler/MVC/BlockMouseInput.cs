using UnityEngine;

public class BlockMouseInput
{
    private Camera mainCamera;
    private float zDistanceToCamera;
    private Vector3 offset;

    public void Init(Camera camera)
    {
        mainCamera = camera;
    }

    public void BeginDrag(Transform blockTr)
    {
        zDistanceToCamera = Vector3.Distance(blockTr.position, mainCamera.transform.position);
        offset = blockTr.position - GetMouseWorldPosition();
    }

    public Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = zDistanceToCamera;
        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }

    public Vector3 GetTargetPosition() => GetMouseWorldPosition() + offset;
}
