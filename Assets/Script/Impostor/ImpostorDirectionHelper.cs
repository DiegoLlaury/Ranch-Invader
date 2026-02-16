using UnityEngine;

public static class ImpostorDirectionHelper
{
    public static int GetDirectionIndex(Vector3 impostorPos, Vector3 playerPos)
    {
        Vector3 dir = playerPos - impostorPos;
        dir.y = 0;

        float angle = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
        if (angle < 0) angle += 360;

        return Mathf.RoundToInt(angle / 45f) % 8;
    }
}
