using UnityEngine;
using UnityEngine.InputSystem;

public class CursorLock : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        Cursor.lockState = CursorLockMode.None;
#else
        Cursor.lockState = CursorLockMode.Locked;
#endif
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Cursor.lockState = CursorLockMode.None;

#if UNITY_EDITOR
        if (Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState == CursorLockMode.None)
            Cursor.lockState = CursorLockMode.Locked;
#endif
    }
}
