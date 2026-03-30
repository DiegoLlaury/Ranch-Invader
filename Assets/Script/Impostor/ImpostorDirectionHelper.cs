using UnityEngine;

public static class ImpostorDirectionHelper
{
    // Atlas layout dans CaptureAtlas (directions monde absolues de la caméra) :
    // 0 = caméra depuis +Z  → face Nord
    // 1 = caméra depuis +Z+X → diagonale NE
    // 2 = caméra depuis +X  → face Est
    // 3 = caméra depuis +X-Z → diagonale SE
    // 4 = caméra depuis -Z  → face Sud
    // 5 = caméra depuis -Z-X → diagonale SO
    // 6 = caméra depuis -X  → face Ouest
    // 7 = caméra depuis -X+Z → diagonale NO
    //
    // Règle : on cherche depuis quelle direction monde le joueur observe l'entité.
    // Cette direction = fromEntityToPlayer (entité → joueur), car la caméra du booth
    // est positionnée dans cette même direction pour capturer la face visible par le joueur.

    private static int AngleToIndex(float angleDeg)
    {
        if (angleDeg < 0f) angleDeg += 360f;
        return Mathf.FloorToInt((angleDeg + 22.5f) / 45f) % 8;
    }

    private static Vector3 GetViewDirection(Vector3 entityPos, Vector3 playerPos, Vector3 meshRotationOffset)
    {
        // fromEntityToPlayer : direction depuis l'entité vers le joueur
        // = direction depuis laquelle le joueur voit l'entité
        // = direction depuis laquelle la caméra booth a capturé la face correspondante
        Vector3 dir = playerPos - entityPos;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return Vector3.forward;
        dir.Normalize();

        // Compenser un FBX orienté différemment de l'axe +Z
        if (meshRotationOffset != Vector3.zero)
            dir = Quaternion.Inverse(Quaternion.Euler(meshRotationOffset)) * dir;

        return dir;
    }

    /// <summary>
    /// Retourne l'index de cellule (0-7) pour une entité ROTATIVE (IA NavMesh).
    /// La capture intègre déjà la rotation réelle de l'ennemi via captureRotation,
    /// donc la sélection se fait en espace monde pur — sans compensation d'offset.
    /// Le meshRotationOffset n'affecte que la capture (CaptureImpostor), pas la sélection.
    /// </summary>
    public static int GetDirectionIndexForRotatingEntity(Transform entityTransform, Vector3 playerPos, Vector3 meshRotationOffset)
    {
        Vector3 dir = playerPos - entityTransform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return 0;
        dir.Normalize();
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        return AngleToIndex(angle);
    }

    /// <summary>
    /// Variante blend pour entité rotative.
    /// </summary>
    public static void GetDirectionBlendForRotatingEntity(Transform entityTransform, Vector3 playerPos, Vector3 meshRotationOffset, out int dirIndex, out int nextDirIndex, out float blendFactor)
    {
        Vector3 dir = playerPos - entityTransform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) { dirIndex = 0; nextDirIndex = 1; blendFactor = 0f; return; }
        dir.Normalize();
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;
        float exactIndex = angle / 45f;
        dirIndex     = Mathf.FloorToInt(exactIndex) % 8;
        nextDirIndex = (dirIndex + 1) % 8;
        blendFactor  = exactIndex - Mathf.Floor(exactIndex);
    }

    /// <summary>
    /// Index de direction pour une entité statique (sans rotation IA).
    /// </summary>
    public static int GetDirectionIndexFromRotation(Transform entityTransform, Vector3 playerPos, Vector3 meshRotationOffset)
    {
        Vector3 dir = GetViewDirection(entityTransform.position, playerPos, meshRotationOffset);
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        return AngleToIndex(angle);
    }

    /// <summary>
    /// Variante blend pour entité statique.
    /// </summary>
    public static void GetDirectionBlendFromRotation(Transform entityTransform, Vector3 playerPos, Vector3 meshRotationOffset, out int dirIndex, out int nextDirIndex, out float blendFactor)
    {
        Vector3 dir = GetViewDirection(entityTransform.position, playerPos, meshRotationOffset);
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;
        float exactIndex = angle / 45f;
        dirIndex     = Mathf.FloorToInt(exactIndex) % 8;
        nextDirIndex = (dirIndex + 1) % 8;
        blendFactor  = exactIndex - Mathf.Floor(exactIndex);
    }

    /// <summary>
    /// Index de direction pour un impostor sans rotation propre (statique monde).
    /// </summary>
    public static int GetDirectionIndex(Vector3 impostorPos, Vector3 playerPos)
    {
        Vector3 dir = playerPos - impostorPos;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return 0;
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        return AngleToIndex(angle);
    }

    /// <summary>
    /// Variante blend pour impostor statique monde.
    /// </summary>
    public static void GetDirectionBlend(Vector3 impostorPos, Vector3 playerPos, out int dirIndex, out int nextDirIndex, out float blendFactor)
    {
        Vector3 dir = playerPos - impostorPos;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f)
        {
            dirIndex = 0; nextDirIndex = 1; blendFactor = 0f;
            return;
        }
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;
        float exactIndex = angle / 45f;
        dirIndex     = Mathf.FloorToInt(exactIndex) % 8;
        nextDirIndex = (dirIndex + 1) % 8;
        blendFactor  = exactIndex - Mathf.Floor(exactIndex);
    }
}
