
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectorKahoots : MonoBehaviour
{
    [Header("Lista de cuestionarios")]
    [SerializeField] private Transform contenidoLista;
    [SerializeField] private FilaSelectorKahoot filaBaseEscena;

    [Header("Navegacion")]
    [SerializeField] private NavegadorPartidaKahoot navegador;
    [SerializeField] private GestorPaneles gestorPaneles;
    [SerializeField] private CanvasGroup panelSelector;

    private readonly CatalogoKahoots catalogo =
        new CatalogoKahoots();

    private readonly List<FilaSelectorKahoot> copias =
        new List<FilaSelectorKahoot>();

    private bool estabaAbierto;
    private void OnEnable()
    {
        RefrescarListado();
        estabaAbierto = true;
    }

    private void Start()
    {
        RefrescarListado();
    }

    private void Update()
    {
        if (gestorPaneles == null || panelSelector == null)
            return;

        bool abierto =
            gestorPaneles.PanelActual == panelSelector;

        if (abierto && !estabaAbierto)
            RefrescarListado();

        estabaAbierto = abierto;
    }

    public void RefrescarListado()
    {
        if (contenidoLista == null || filaBaseEscena == null)
        {
            Debug.LogError(
                "SelectorKahoots: faltan Content o Bloque.",
                this
            );
            return;
        }

        if (filaBaseEscena.transform.parent != contenidoLista)
        {
            Debug.LogError(
                "Bloque debe ser hijo directo de Content.",
                this
            );
            return;
        }

        foreach (FilaSelectorKahoot copia in copias)
        {
            if (copia == null)
                continue;

            copia.gameObject.SetActive(false);
            Destroy(copia.gameObject);
        }

        copias.Clear();

        List<KahootDatos> cuestionarios =
            catalogo.ObtenerDisponibles();

        if (cuestionarios == null ||
            cuestionarios.Count == 0)
        {
            filaBaseEscena.gameObject.SetActive(false);
            Debug.LogWarning(
                "No hay cuestionarios JSON disponibles."
            );
            return;
        }

        filaBaseEscena.gameObject.SetActive(true);
        filaBaseEscena.transform.SetAsFirstSibling();

        filaBaseEscena.Configurar(
            cuestionarios[0],
            SeleccionarQuiz
        );

        for (int i = 1; i < cuestionarios.Count; i++)
        {
            KahootDatos datos = cuestionarios[i];

            if (datos == null)
                continue;

            FilaSelectorKahoot nueva = Instantiate(
                filaBaseEscena,
                contenidoLista,
                false
            );

            nueva.transform.SetAsLastSibling();
            nueva.gameObject.SetActive(true);

            nueva.Configurar(
                datos,
                SeleccionarQuiz
            );

            copias.Add(nueva);
        }

        if (contenidoLista is RectTransform rect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        Debug.Log(
            "Selector actualizado: " +
            cuestionarios.Count +
            " Kahoots disponibles."
        );

        Debug.Log(
            "Carpeta JSON: " +
            AlmacenKahoots.CarpetaKahoots
        );
    }

    private void SeleccionarQuiz(KahootDatos datos)
    {
        if (navegador == null)
        {
            Debug.LogError(
                "Falta NavegadorPartidaKahoot.",
                this
            );
            return;
        }

        navegador.Abrir(datos);
    }
}
