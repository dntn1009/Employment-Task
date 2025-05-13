using UnityEngine;

public class BlockCollisionState
{
    public bool IsColliding { get; private set; } = false;
    public Vector3 CollisionNormal { get; private set; } = Vector3.zero;
    private float collisionResetTime = 0.1f;
    private float lastCollisionTime;

    public void Reset()
    {
        IsColliding = false;
        CollisionNormal = Vector3.zero;
    }

    public void UpdateCollision()
    {
        if (IsColliding && Time.time - lastCollisionTime > collisionResetTime)
        {
            Reset();
        }
    }

    public void HandleCollision(Collision collision, bool isDragging)
    {
        if (!isDragging) return;
        if (collision.contactCount > 0 && collision.gameObject.layer != LayerMask.NameToLayer("Board"))
        {
            Vector3 normal = collision.contacts[0].normal;
            if (Vector3.Dot(normal, Vector3.up) < 0.8f)
            {
                IsColliding = true;
                CollisionNormal = normal;
                lastCollisionTime = Time.time;
            }
        }
    }

    public void HandleExit(Collision collision)
    {
        if (collision.contactCount > 0)
        {
            Vector3 normal = collision.contacts[0].normal;
            if (Vector3.Dot(normal, CollisionNormal) > 0.8f)
            {
                Reset();
            }
        }
    }
}
