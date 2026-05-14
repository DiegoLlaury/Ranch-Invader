#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Draws all BoxColliders, SphereColliders, and CapsuleColliders in the Scene view.
/// Toggle via Tools > Collider Debug Drawer. Zero runtime cost.
/// </summary>
[InitializeOnLoad]
public static class ColliderDebugDrawer
{
    private const string MenuPath = "Tools/Collider Debug Drawer";
    private const string PreferenceKey = "ColliderDebugDrawer_Enabled";

    // ── Colors ───────────────────────────────────────────────────────────────
    private static readonly Color ColorActiveFill = new Color(0f, 1f, 0f, 0.08f);
    private static readonly Color ColorTriggerFill = new Color(0f, 0.6f, 1f, 0.08f);
    private static readonly Color ColorDisabledFill = new Color(1f, 0f, 0f, 0.05f);
    private static readonly Color ColorActiveWire = new Color(0f, 1f, 0f, 0.9f);
    private static readonly Color ColorTriggerWire = new Color(0f, 0.6f, 1f, 0.9f);
    private static readonly Color ColorDisabledWire = new Color(1f, 0f, 0f, 0.5f);

    private static bool IsEnabled => EditorPrefs.GetBool(PreferenceKey, true);

    static ColliderDebugDrawer()
    {
        SceneView.duringSceneGui += OnSceneGui;
    }

    [MenuItem(MenuPath)]
    private static void ToggleDrawer()
    {
        EditorPrefs.SetBool(PreferenceKey, !IsEnabled);
        SceneView.RepaintAll();
    }

    [MenuItem(MenuPath, validate = true)]
    private static bool ValidateToggleDrawer()
    {
        Menu.SetChecked(MenuPath, IsEnabled);
        return true;
    }

    private static void OnSceneGui(SceneView sceneView)
    {
        if (!IsEnabled)
            return;

        // FindObjectsByType<T>() sans FindObjectsSortMode — API non dépréciée
        Collider[] colliders = Object.FindObjectsByType<Collider>(FindObjectsInactive.Include);

        foreach (Collider col in colliders)
            DrawCollider(col);
    }

    private static void DrawCollider(Collider col)
    {
        bool isTrigger = col.isTrigger;
        bool isEnabled = col.enabled && col.gameObject.activeInHierarchy;

        Color fill = isEnabled ? (isTrigger ? ColorTriggerFill : ColorActiveFill) : ColorDisabledFill;
        Color wire = isEnabled ? (isTrigger ? ColorTriggerWire : ColorActiveWire) : ColorDisabledWire;

        switch (col)
        {
            case BoxCollider box:
                DrawBox(box, fill, wire);
                break;
            case SphereCollider sphere:
                DrawSphere(sphere, fill, wire);
                break;
            case CapsuleCollider capsule:
                DrawCapsule(capsule, fill, wire);
                break;
        }
    }

    // ── BoxCollider ──────────────────────────────────────────────────────────
    private static void DrawBox(BoxCollider box, Color fill, Color wire)
    {
        Transform t = box.transform;
        Vector3 center = t.TransformPoint(box.center);
        Matrix4x4 matrix = Matrix4x4.TRS(center, t.rotation, t.lossyScale);

        using (new Handles.DrawingScope(fill, matrix))
            Handles.DrawAAConvexPolygon(GetBoxFaces(box.size));

        using (new Handles.DrawingScope(wire, matrix))
            Handles.DrawWireCube(Vector3.zero, box.size);
    }

    // ── SphereCollider ───────────────────────────────────────────────────────
    private static void DrawSphere(SphereCollider sphere, Color fill, Color wire)
    {
        Transform t = sphere.transform;
        Vector3 center = t.TransformPoint(sphere.center);
        float scale = Mathf.Max(
            Mathf.Abs(t.lossyScale.x),
            Mathf.Abs(t.lossyScale.y),
            Mathf.Abs(t.lossyScale.z)
        );
        float radius = sphere.radius * scale;

        using (new Handles.DrawingScope(fill))
            Handles.SphereHandleCap(0, center, Quaternion.identity, radius * 2f, EventType.Repaint);

        using (new Handles.DrawingScope(wire))
        {
            Handles.DrawWireDisc(center, Vector3.up, radius);
            Handles.DrawWireDisc(center, Vector3.right, radius);
            Handles.DrawWireDisc(center, Vector3.forward, radius);
        }
    }

    // ── CapsuleCollider ──────────────────────────────────────────────────────
    private static void DrawCapsule(CapsuleCollider capsule, Color fill, Color wire)
    {
        Transform t = capsule.transform;
        Vector3 center = t.TransformPoint(capsule.center);
        Vector3 lossyScale = t.lossyScale;

        Vector3 axis = capsule.direction switch
        {
            0 => t.right,
            1 => t.up,
            2 => t.forward,
            _ => t.up
        };

        float radiusScale = capsule.direction switch
        {
            0 => Mathf.Max(Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z)),
            1 => Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.z)),
            2 => Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y)),
            _ => 1f
        };
        float heightScale = capsule.direction switch
        {
            0 => Mathf.Abs(lossyScale.x),
            1 => Mathf.Abs(lossyScale.y),
            2 => Mathf.Abs(lossyScale.z),
            _ => 1f
        };

        float radius = capsule.radius * radiusScale;
        float halfHeight = Mathf.Max(capsule.height * 0.5f * heightScale, radius);
        float bodyHalf = halfHeight - radius;

        Vector3 topCenter = center + axis * bodyHalf;
        Vector3 bottomCenter = center - axis * bodyHalf;

        using (new Handles.DrawingScope(fill))
        {
            Handles.SphereHandleCap(0, topCenter, Quaternion.identity, radius * 2f, EventType.Repaint);
            Handles.SphereHandleCap(0, bottomCenter, Quaternion.identity, radius * 2f, EventType.Repaint);
        }

        using (new Handles.DrawingScope(wire))
        {
            Handles.DrawWireDisc(topCenter, axis, radius);
            Handles.DrawWireDisc(bottomCenter, axis, radius);

            Vector3 perp1 = Vector3.Cross(axis, Vector3.up).normalized;
            if (perp1.sqrMagnitude < 0.001f)
                perp1 = Vector3.Cross(axis, Vector3.right).normalized;
            Vector3 perp2 = Vector3.Cross(axis, perp1).normalized;

            Handles.DrawLine(topCenter + perp1 * radius, bottomCenter + perp1 * radius);
            Handles.DrawLine(topCenter - perp1 * radius, bottomCenter - perp1 * radius);
            Handles.DrawLine(topCenter + perp2 * radius, bottomCenter + perp2 * radius);
            Handles.DrawLine(topCenter - perp2 * radius, bottomCenter - perp2 * radius);
        }
    }

    // ── Helper ───────────────────────────────────────────────────────────────
    private static Vector3[] GetBoxFaces(Vector3 size)
    {
        float x = size.x * 0.5f;
        float y = size.y * 0.5f;
        float z = size.z * 0.5f;

        return new Vector3[]
        {
            new(-x, -y,  z), new( x, -y,  z), new( x,  y,  z), new(-x,  y,  z),
            new( x, -y, -z), new(-x, -y, -z), new(-x,  y, -z), new( x,  y, -z),
            new(-x, -y, -z), new(-x, -y,  z), new(-x,  y,  z), new(-x,  y, -z),
            new( x, -y,  z), new( x, -y, -z), new( x,  y, -z), new( x,  y,  z),
            new(-x,  y,  z), new( x,  y,  z), new( x,  y, -z), new(-x,  y, -z),
            new(-x, -y, -z), new( x, -y, -z), new( x, -y,  z), new(-x, -y,  z),
        };
    }
}
#endif
