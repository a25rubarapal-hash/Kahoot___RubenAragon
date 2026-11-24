
using System.Collections;
using UnityEngine;

public class NavegadorPartidaKahoot : MonoBehaviour
{
    [Header("Navegacion")]
    [SerializeField] private TransicionMenu transicionMenu;
    [SerializeField] private GestorPaneles gestorPaneles;

    [Header("Pantallas")]
    [SerializeField] private CanvasGroup panelSelector;
    [SerializeField] private GameObject panelPartida;

    [Header("Partida")]
    [SerializeField] private PartidaKahoot partida;

    private bool abriendo;

    public void Abrir(KahootDatos datos)
    {
        if (abriendo)
            return;

        if (datos == null)
        {
            Debug.LogError("El Kahoot seleccionado es nulo.");
            return;
        }

        if (gestorPaneles == null ||
            panelPartida == null ||
            partida == null ||
            panelSelector == null)
        {
            Debug.LogError(
                "NavegadorPartidaKahoot: faltan referencias.",
                this
            );
            return;
        }

        if (transicionMenu != null &&
            transicionMenu.EnTransicion)
            return;

        StartCoroutine(AbrirPartida(datos));
    }

    private IEnumerator AbrirPartida(KahootDatos datos)
    {
        abriendo = true;

        Debug.Log("Abriendo Kahoot: " + datos.titulo);

        CanvasGroup destino =
            panelPartida.GetComponent<CanvasGroup>();

        if (destino == null)
        {
            Debug.LogError(
                "PlantillaGeneral no tiene CanvasGroup."
            );
            abriendo = false;
            yield break;
        }

        CanvasGroup origen = gestorPaneles.PanelActual;

        if (origen == destino)
        {
            partida.Comenzar(datos);
            abriendo = false;
            yield break;
        }

        // Preparar la plantilla sin mostrarla todavía.
        panelPartida.SetActive(true);

        destino.alpha = 0f;
        destino.interactable = false;
        destino.blocksRaycasts = false;

        // Cargar todas las preguntas del Kahoot.
        partida.Comenzar(datos);

        // Utilizar las transiciones actuales del proyecto.
        if (transicionMenu != null && origen != null)
        {
            transicionMenu.AbrirPanelDesdeOtroPanel(
                panelPartida
            );

            yield return null;

            while (transicionMenu.EnTransicion)
                yield return null;
        }

        // Garantizar que el destino quede seleccionado.
        if (gestorPaneles.PanelActual != destino)
        {
            Debug.LogWarning(
                "La transición no completó el cambio. " +
                "Mostrando PlantillaGeneral directamente."
            );

            gestorPaneles.MostrarPanel(destino);
        }

        // Garantizar que el selector desaparezca.
        panelSelector.alpha = 0f;
        panelSelector.interactable = false;
        panelSelector.blocksRaycasts = false;

        // Garantizar que la partida sea visible.
        destino.alpha = 1f;
        destino.interactable = true;
        destino.blocksRaycasts = true;

        Debug.Log(
            "PlantillaGeneral visible. Partida iniciada: " +
            datos.titulo
        );

        abriendo = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        abriendo = false;
    }
}
