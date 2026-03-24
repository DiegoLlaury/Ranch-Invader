using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ScreenSizeChangeNotifier : UIBehaviour
{
    [SerializeField] private UnityEvent notifyScreenSizeChange;

    protected override void OnRectTransformDimensionsChange()
    {
        // Ne pas notifier avant que le jeu soit initialisé :
        // CanvasScaler.OnEnable() peut déclencher cet event avec des dimensions invalides
        if (!Application.isPlaying || Screen.width <= 0 || Screen.height <= 0)
            return;

        notifyScreenSizeChange.Invoke();
    }
}
