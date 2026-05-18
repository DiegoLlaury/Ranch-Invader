using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Blocks all player input during a cinematic by switching to an empty action map
/// and zeroing out StarterAssetsInputs values.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class CinematicInputBlocker : MonoBehaviour
{
    private const string PlayerActionMap = "Player";
    private const string UIActionMap = "UI";

    private PlayerInput _playerInput;
    private StarterAssetsInputs _starterInputs;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _starterInputs = GetComponent<StarterAssetsInputs>();
    }

    /// <summary>Disables all player inputs. Call this at cinematic start.</summary>
    public void Block()
    {
        if (_playerInput != null)
            _playerInput.SwitchCurrentActionMap(UIActionMap);

        if (_starterInputs != null)
        {
            _starterInputs.move = Vector2.zero;
            _starterInputs.look = Vector2.zero;
            _starterInputs.jump = false;
            _starterInputs.sprint = false;
            _starterInputs.cursorInputForLook = false;
            _starterInputs.cursorLocked = false;
            _starterInputs.SetCursorState(false);
        }
    }

    /// <summary>Re-enables player inputs. Call this at cinematic end.</summary>
    public void Unblock()
    {
        if (_starterInputs != null)
        {
            _starterInputs.cursorInputForLook = true;
            _starterInputs.cursorLocked = true;
            _starterInputs.SetCursorState(true);
        }

        if (_playerInput != null)
            _playerInput.SwitchCurrentActionMap(PlayerActionMap);
    }
}
