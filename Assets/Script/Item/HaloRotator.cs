using UnityEngine;

/// <summary>
/// Rotates this GameObject slowly around a given local axis.
/// Attach directly on the halo sprite child object.
/// </summary>
public class HaloRotator : MonoBehaviour
{
    [Header("Rotation")]
    [Tooltip("Degrees per second.")]
    public float rotationSpeed = 30f;

    [Tooltip("Local axis to rotate around. Y = flat spin on ground plane.")]
    public Vector3 rotationAxis = Vector3.up;

    [Header("Bobbing (optional)")]
    [Tooltip("Adds a gentle up/down float to the parent object.")]
    public bool enableBobbing = true;

    [Tooltip("Amplitude of the bob in world units.")]
    public float bobAmplitude = 0.08f;

    [Tooltip("Speed of the bob cycle.")]
    public float bobSpeed = 1.2f;

    private Vector3 initialLocalPosition;
    public float offSetY = 0.5f;

    private void Start()
    {
        initialLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        // Rotate the halo around its local axis
        transform.Rotate(rotationAxis.normalized, rotationSpeed * Time.deltaTime, Space.Self);

        // Optional gentle bob on the parent (the beer itself)
        if (enableBobbing && transform.parent != null)
        {
            Vector3 pos = transform.parent.localPosition;
            pos.y = initialLocalPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude + offSetY;
            transform.parent.localPosition = pos;
        }
    }
}
