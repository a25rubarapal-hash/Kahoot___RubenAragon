
using UnityEngine;
using UnityEngine.InputSystem;

public class EscKey : MonoBehaviour
{
    [SerializeField] private TransicionMenu transicionMenu;
    [SerializeField] private GestorPaneles gestorPaneles;
    [SerializeField] private CanvasGroup menuPrincipal;

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Volver();
    }

    public void Volver()
    {
        if (MenuJugador.CerrarVentana()) return;
        if (transicionMenu == null ||
            gestorPaneles == null ||
            menuPrincipal == null)
        {
            return;
        }

        if (transicionMenu.EnTransicion)
            return;

        CanvasGroup panelActual = gestorPaneles.PanelActual;

        if (panelActual == null || panelActual == menuPrincipal)
            return;

        var creaciones = panelActual.GetComponent<GestorCreacionesUI>();
        if (creaciones != null && creaciones.VolverDesdeEditor()) return;

        transicionMenu.VolverPanelAnterior();
    }
}
