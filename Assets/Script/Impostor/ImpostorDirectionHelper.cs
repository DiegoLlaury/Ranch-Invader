using UnityEngine;

public static class ImpostorDirectionHelper
{
    public static int GetDirectionIndex(Vector3 impostorPos, Vector3 playerPos)
    {
        Vector3 dir = impostorPos - playerPos;
        dir.y = 0;

        float angle = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
        if (angle < 0) angle += 360;

        return Mathf.RoundToInt(angle / 45f) % 8;
    }

    //  NOUVEAU : Retourne l'index, l'index suivant et le facteur de blend
    public static void GetDirectionBlend(Vector3 impostorPos, Vector3 playerPos, out int dirIndex, out int nextDirIndex, out float blendFactor)
    {
        Vector3 dir = impostorPos - playerPos;
        dir.y = 0;

        float angle = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
        if (angle < 0) angle += 360;

        // Calculer l'index exact (avec décimales)
        float exactIndex = angle / 45f;

        // Index principal (floor)
        dirIndex = Mathf.FloorToInt(exactIndex) % 8;

        // Index suivant (ceil)
        nextDirIndex = (dirIndex + 1) % 8;

        // Facteur de blend (0 à 1 entre les deux directions)
        blendFactor = exactIndex - Mathf.Floor(exactIndex);
    }
}
