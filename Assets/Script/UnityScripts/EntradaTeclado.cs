using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Permite probar el brazo sin hardware: flechas para mover y espacio para agarrar.
public class EntradaTeclado : EntradaBrazo
{
    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard teclado = Keyboard.current;
        if (teclado == null)
        {
            Eje = Vector2.zero;
            return;
        }

        float x = (teclado.rightArrowKey.isPressed ? 1f : 0f) - (teclado.leftArrowKey.isPressed ? 1f : 0f);
        float y = (teclado.upArrowKey.isPressed ? 1f : 0f) - (teclado.downArrowKey.isPressed ? 1f : 0f);
        Eje = new Vector2(x, y);

        if (teclado.spaceKey.wasPressedThisFrame)
        {
            RegistrarPulsacion();
        }
#else
        float x = (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
        float y = (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
        Eje = new Vector2(x, y);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RegistrarPulsacion();
        }
#endif
    }
}
