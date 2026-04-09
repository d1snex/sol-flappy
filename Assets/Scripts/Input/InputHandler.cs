using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public static event Action OnTap;

    private void Update()
    {
        // Touch input (mobile) — use primaryTouch to avoid multi-fire
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
            if (!IsPointerOverUI(touchId))
            {
                OnTap?.Invoke();
                return;
            }
        }

        // Mouse fallback (editor)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!IsPointerOverUI(-1))
                OnTap?.Invoke();
        }
    }

    private bool IsPointerOverUI(int pointerId)
    {
        if (EventSystem.current == null)
            return false;

        if (pointerId >= 0)
            return EventSystem.current.IsPointerOverGameObject(pointerId);

        return EventSystem.current.IsPointerOverGameObject();
    }
}
