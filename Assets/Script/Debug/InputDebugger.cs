using UnityEngine;
using StarterAssets;

public class InputDebugger : MonoBehaviour
{
    private StarterAssetsInputs _input;

    private void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
    }

    private void Update()
    {
        if (_input != null)
        {
            Debug.Log($"Look Input - X: {_input.look.x}, Y: {_input.look.y}");
        }
    }
}
