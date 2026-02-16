using UnityEngine;

public class Billboard : MonoBehaviour
{
    [Header("Billboard Settings")]
    [Tooltip("Ne suit que l'axe Y de la caméra (ignore pitch/roll)")]
    public bool lockToYAxis = true;

    [Header("Rotation Smoothing")]
    [Tooltip("Active la rotation progressive au lieu d'instantanée")]
    public bool useSmoothRotation = true;

    [Tooltip("Vitesse de rotation (plus petit = plus lent/fluide)")]
    [Range(0.1f, 50f)]
    public float rotationSpeed = 8f;

    [Tooltip("Latence avant de commencer à tourner (en degrés)")]
    [Range(0f, 45f)]
    public float rotationDeadZone = 5f;

    [Header("Advanced")]
    [Tooltip("Utilise une courbe d'accélération pour la rotation")]
    public bool useAccelerationCurve = false;

    public AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Camera cam;
    private Quaternion targetRotation;
    private float currentAngularVelocity = 0f;

    void Start()
    {
        cam = Camera.main;
        targetRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        if (lockToYAxis)
        {
            UpdateBillboardRotation();
        }
        else
        {
            // Mode classique (full rotation)
            Vector3 lookDirection = cam.transform.rotation * Vector3.forward;
            targetRotation = Quaternion.LookRotation(transform.position + lookDirection, cam.transform.rotation * Vector3.up);

            if (useSmoothRotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
    }

    void UpdateBillboardRotation()
    {
        // Calculer la direction vers la caméra
        Vector3 directionToCamera = cam.transform.position - transform.position;
        directionToCamera.y = 0;

        if (directionToCamera.sqrMagnitude < 0.001f)
            return;

        // Rotation cible
        targetRotation = Quaternion.LookRotation(directionToCamera);

        // Calculer l'angle de différence
        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

        if (useSmoothRotation)
        {
            // Si en dehors de la dead zone, appliquer la rotation smooth
            if (angleDifference > rotationDeadZone)
            {
                float effectiveSpeed = rotationSpeed;

                // Appliquer la courbe d'accélération si activée
                if (useAccelerationCurve)
                {
                    float normalizedAngle = Mathf.Clamp01((angleDifference - rotationDeadZone) / 45f);
                    float curveMultiplier = accelerationCurve.Evaluate(normalizedAngle);
                    effectiveSpeed *= curveMultiplier;
                }

                // Rotation progressive avec Slerp
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * effectiveSpeed
                );
            }
            // Sinon, dans la dead zone : ne rien faire (latence)
        }
        else
        {
            // Rotation instantanée
            transform.rotation = targetRotation;
        }
    }

    // Pour le debug
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || cam == null)
            return;

        // Dessiner la direction actuelle
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);

        // Dessiner la direction cible
        Gizmos.color = Color.red;
        Vector3 targetDir = cam.transform.position - transform.position;
        targetDir.y = 0;
        targetDir.Normalize();
        Gizmos.DrawRay(transform.position, targetDir * 2.5f);

        // Afficher la dead zone
        if (useSmoothRotation && rotationDeadZone > 0)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            // Dessiner un arc représentant la dead zone (simplifié)
            Vector3 leftBound = Quaternion.Euler(0, -rotationDeadZone, 0) * transform.forward;
            Vector3 rightBound = Quaternion.Euler(0, rotationDeadZone, 0) * transform.forward;
            Gizmos.DrawRay(transform.position, leftBound * 1.5f);
            Gizmos.DrawRay(transform.position, rightBound * 1.5f);
        }
    }
}
