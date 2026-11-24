using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class ControlMando : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float sensibilidadStick = 0.7f;
    [SerializeField] private float zonaMuertaStick = 0.3f;

    private Action alGirarIzquierda;
    private Action alGirarDerecha;
    private Action alConfirmar;

    private bool stickBloqueado;

    public void Inicializar(
        Action izquierda,
        Action derecha,
        Action confirmar)
    {
        alGirarIzquierda = izquierda;
        alGirarDerecha = derecha;
        alConfirmar = confirmar;
    }

    private void Update()
    {
        if (MenuJugador.EscribiendoNombre) return;
        DetectarTeclado();
        DetectarMando();
    }

    private void DetectarTeclado()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            alGirarIzquierda?.Invoke();

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            alGirarDerecha?.Invoke();

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            alConfirmar?.Invoke();
        }
    }

    private void DetectarMando()
    {
        if (Gamepad.current == null)
            return;

        Gamepad mando = Gamepad.current;

        // Cruceta
        if (mando.dpad.left.wasPressedThisFrame)
            alGirarIzquierda?.Invoke();

        if (mando.dpad.right.wasPressedThisFrame)
            alGirarDerecha?.Invoke();

        // Stick izquierdo
        float horizontal = mando.leftStick.x.ReadValue();

        if (Mathf.Abs(horizontal) < zonaMuertaStick)
        {
            stickBloqueado = false;
        }

        if (!stickBloqueado)
        {
            if (horizontal < -sensibilidadStick)
            {
                alGirarIzquierda?.Invoke();
                stickBloqueado = true;
            }
            else if (horizontal > sensibilidadStick)
            {
                alGirarDerecha?.Invoke();
                stickBloqueado = true;
            }
        }

        // Botón A de Xbox / X de PlayStation
        if (mando.buttonSouth.wasPressedThisFrame)
            alConfirmar?.Invoke();
    }
}