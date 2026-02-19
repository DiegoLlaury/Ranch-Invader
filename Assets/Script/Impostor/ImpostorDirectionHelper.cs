using UnityEngine;

public static class ImpostorDirectionHelper
{
    public static int GetDirectionIndexForRotatingEntity(Transform entityTransform, Vector3 playerPos, Vector3 meshRotationOffset)
    {
        Vector3 fromEntityToPlayer = (playerPos - entityTransform.position).normalized;
        fromEntityToPlayer.y = 0;

        Quaternion meshWorldRotation = entityTransform.rotation * Quaternion.Euler(meshRotationOffset);
        Vector3 meshForward = meshWorldRotation * Vector3.forward;
        meshForward.y = 0;
        meshForward.Normalize();

        float angleFromMeshToPlayer = Mathf.Atan2(fromEntityToPlayer.x, fromEntityToPlayer.z) * Mathf.Rad2Deg;
        float meshAngle = Mathf.Atan2(meshForward.x, meshForward.z) * Mathf.Rad2Deg;

        float relativeAngle = angleFromMeshToPlayer - meshAngle + 180f;

        if (relativeAngle < 0) relativeAngle += 360;
        if (relativeAngle >= 360) relativeAngle -= 360;

        return Mathf.RoundToInt(relativeAngle / 45f) % 8;
    }

    public static void GetDirectionBlendForRotatingEntity(Transform entityTransform, Vector3 playerPos, Vector3 meshRotationOffset, out int dirIndex, out int nextDirIndex, out float blendFactor)
    {
        Vector3 fromEntityToPlayer = (playerPos - entityTransform.position).normalized;
        fromEntityToPlayer.y = 0;

        Quaternion meshWorldRotation = entityTransform.rotation * Quaternion.Euler(meshRotationOffset);
        Vector3 meshForward = meshWorldRotation * Vector3.forward;
        meshForward.y = 0;
        meshForward.Normalize();

        float angleFromMeshToPlayer = Mathf.Atan2(fromEntityToPlayer.x, fromEntityToPlayer.z) * Mathf.Rad2Deg;
        float meshAngle = Mathf.Atan2(meshForward.x, meshForward.z) * Mathf.Rad2Deg;

        float relativeAngle = angleFromMeshToPlayer - meshAngle + 180f;

        if (relativeAngle < 0) relativeAngle += 360;
        if (relativeAngle >= 360) relativeAngle -= 360;

        float exactIndex = relativeAngle / 45f;
        dirIndex = Mathf.FloorToInt(exactIndex) % 8;
        nextDirIndex = (dirIndex + 1) % 8;
        blendFactor = exactIndex - Mathf.Floor(exactIndex);
    }


    public static int GetDirectionIndexFromRotation(Transform entityTransform, Vector3 playerPos, Vector3 meshRotationOffset)
    {
        Vector3 toPlayer = (playerPos - entityTransform.position).normalized;
        toPlayer.y = 0;

        Quaternion meshWorldRotation = entityTransform.rotation * Quaternion.Euler(meshRotationOffset);
        Vector3 meshForward = meshWorldRotation * Vector3.forward;
        meshForward.y = 0;
        meshForward.Normalize();

        float angle = Vector3.SignedAngle(meshForward, toPlayer, Vector3.up);
        if (angle < 0) angle += 360;

        return Mathf.RoundToInt(angle / 45f) % 8;
    }

    public static void GetDirectionBlendFromRotation(Transform entityTransform, Vector3 playerPos, Vector3 meshRotationOffset, out int dirIndex, out int nextDirIndex, out float blendFactor)
    {
        Vector3 toPlayer = (playerPos - entityTransform.position).normalized;
        toPlayer.y = 0;

        Quaternion meshWorldRotation = entityTransform.rotation * Quaternion.Euler(meshRotationOffset);
        Vector3 meshForward = meshWorldRotation * Vector3.forward;
        meshForward.y = 0;
        meshForward.Normalize();

        float angle = Vector3.SignedAngle(meshForward, toPlayer, Vector3.up);
        if (angle < 0) angle += 360;

        float exactIndex = angle / 45f;
        dirIndex = Mathf.FloorToInt(exactIndex) % 8;
        nextDirIndex = (dirIndex + 1) % 8;
        blendFactor = exactIndex - Mathf.Floor(exactIndex);
    }

    public static int GetDirectionIndex(Vector3 impostorPos, Vector3 playerPos)
    {
        Vector3 dir = impostorPos - playerPos;
        dir.y = 0;

        float angle = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
        if (angle < 0) angle += 360;

        return Mathf.RoundToInt(angle / 45f) % 8;
    }

    public static void GetDirectionBlend(Vector3 impostorPos, Vector3 playerPos, out int dirIndex, out int nextDirIndex, out float blendFactor)
    {
        Vector3 dir = impostorPos - playerPos;
        dir.y = 0;

        float angle = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
        if (angle < 0) angle += 360;

        float exactIndex = angle / 45f;

        dirIndex = Mathf.FloorToInt(exactIndex) % 8;
        nextDirIndex = (dirIndex + 1) % 8;
        blendFactor = exactIndex - Mathf.Floor(exactIndex);
    }
}
