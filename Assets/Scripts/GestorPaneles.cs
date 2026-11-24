
using UnityEngine;

public class GestorPaneles : MonoBehaviour
{
    [Header("Todas las pantallas principales")]
    [SerializeField] private CanvasGroup[] paneles;

    private CanvasGroup panelActual;

    public CanvasGroup PanelActual => panelActual;

    public void Inicializar(CanvasGroup panelInicial)
    {
        if (panelInicial == null)
        {
            Debug.LogError(
                "GestorPaneles: Falta el panel inicial.",
                this
            );
            return;
        }

        if (!EstaRegistrado(panelInicial))
        {
            Debug.LogError(
                "El panel inicial no esta en el array Paneles.",
                this
            );
            return;
        }

        foreach (CanvasGroup panel in paneles)
        {
            if (panel != null && panel != panelInicial)
                Ocultar(panel);
        }

        Activar(panelInicial);
        panelActual = panelInicial;

        Debug.Log(
            "Pantalla inicial: " + panelInicial.name
        );
    }

    public void MostrarPanel(GameObject objeto)
    {
        if (objeto == null)
            return;

        CanvasGroup destino =
            objeto.GetComponent<CanvasGroup>();

        MostrarPanel(destino);
    }

    public void MostrarPanel(CanvasGroup destino)
    {
        if (destino == null)
        {
            Debug.LogError(
                "GestorPaneles: El destino necesita CanvasGroup.",
                this
            );
            return;
        }

        if (!EstaRegistrado(destino))
        {
            Debug.LogError(
                "La pantalla " + destino.name +
                " no esta registrada en GestorPaneles.",
                this
            );
            return;
        }

        foreach (CanvasGroup panel in paneles)
        {
            if (panel == null || panel == destino)
                continue;

            Ocultar(panel);
        }

        Activar(destino);
        panelActual = destino;

        Debug.Log(
            "Pantalla activa: " + destino.name
        );
    }

    private void Activar(CanvasGroup panel)
    {
        panel.gameObject.SetActive(true);

        panel.alpha = 1f;
        panel.interactable = true;
        panel.blocksRaycasts = true;
    }

    private void Ocultar(CanvasGroup panel)
    {
        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;

        // Nunca desactivar el objeto que contiene
        // el gestor ni ninguno de sus padres.
        if (!transform.IsChildOf(panel.transform))
            panel.gameObject.SetActive(false);
    }

    public bool EstaRegistrado(CanvasGroup panel)
    {
        if (panel == null || paneles == null)
            return false;

        foreach (CanvasGroup registrado in paneles)
        {
            if (registrado == panel)
                return true;
        }

        return false;
    }
}
