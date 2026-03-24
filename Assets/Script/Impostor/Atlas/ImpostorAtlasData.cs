using UnityEngine;

[System.Serializable]
public class ImpostorAtlasData
{
    public RenderTexture atlas;
    public int gridSize = 4; // 4x2

    public int GetCellCount() => 8;

    public Vector2 GetUVOffset(int index)
    {
        int x = index % 4;
        int y = index / 4;

        return new Vector2(x / 4f, y / 2f);
    }

    public Vector2 GetUVScale()
    {
        return new Vector2(1f / 4f, 1f / 2f);
    }
}