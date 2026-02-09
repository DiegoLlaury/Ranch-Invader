using UnityEngine;

[System.Serializable]
public class WeaponAnimationFrame
{
    [Header("Visuel")]
    public Sprite sprite;

    [Header("Position (offset par rapport à la position idle)")]
    public Vector2 positionOffset = Vector2.zero;

    [Header("Rotation")]
    public float rotationAngle = 0f;

    [Header("Échelle")]
    public Vector3 scaleMultiplier = Vector3.one;

    [Header("Timing")]
    [Tooltip("Durée pour atteindre cette frame depuis la précédente")]
    public float duration = 0.1f;

    [Tooltip("Type d'interpolation")]
    public AnimationCurveType curveType = AnimationCurveType.EaseInOut;
}

public enum AnimationCurveType
{
    Linear,
    EaseIn,
    EaseOut,
    EaseInOut,
    Bounce
}
