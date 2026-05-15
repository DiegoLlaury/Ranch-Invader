using UnityEngine;

/// <summary>
/// Implemented by entities that react to a directional hit with a knockback.
/// </summary>
public interface IKnockbackable
{
    /// <summary>
    /// Applies a knockback impulse originating from the given world-space position.
    /// </summary>
    void ReceiveKnockback(Vector3 hitSourcePosition);
}
