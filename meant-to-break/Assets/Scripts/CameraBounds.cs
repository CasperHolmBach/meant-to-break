using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    public Vector3 minBounds;
    public Vector3 maxBounds;

    void LateUpdate()
    {
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, minBounds.z, maxBounds.z);

        transform.position = clampedPosition;
    }
}
