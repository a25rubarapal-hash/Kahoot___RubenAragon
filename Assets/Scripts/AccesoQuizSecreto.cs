
using UnityEngine;
using UnityEngine.InputSystem;

public class AccesoQuizSecreto : MonoBehaviour
{
    [Header("Menu principal")]
    [SerializeField] private GestorPaneles gestorPaneles;
    [SerializeField] private CanvasGroup menuPrincipal;

    [Header("Navegacion")]
    [SerializeField] private NavegadorPartidaKahoot navegador;
    [SerializeField] private TransicionMenu transicionMenu;

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.f5Key.wasPressedThisFrame)
            return;

        if (gestorPaneles == null ||
            menuPrincipal == null ||
            navegador == null ||
            transicionMenu == null)
        {
            Debug.LogError(
                "AccesoQuizSecreto: faltan referencias.",
                this
            );
            return;
        }

        if (gestorPaneles.PanelActual != menuPrincipal)
            return;

        if (transicionMenu.EnTransicion)
            return;

        if (AlmacenKahoots.IntentarCargar(
            GeneradorKahootsIniciales.RutaSecreta,
            out KahootDatos datos))
        {
            navegador.Abrir(datos);
        }
    }
}
