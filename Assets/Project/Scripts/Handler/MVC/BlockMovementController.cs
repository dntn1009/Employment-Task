using UnityEngine;

public class BlockMovementController
{
    private float moveSpeed = 25f;
    private float followSpeed = 30f;
    private float maxSpeed = 20f;

    public Vector3 CalculateVelocity(Vector3 moveVector, Vector3 collisionNormal, bool isColliding)
    {
        Vector3 velocity;
        if (isColliding)
        {
            Vector3 projectedMove = Vector3.ProjectOnPlane(moveVector, collisionNormal);
            velocity = projectedMove * moveSpeed;
        }
        else
        {
            velocity = moveVector * followSpeed;
        }

        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }

        return velocity;
    }
}
