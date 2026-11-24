
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransicionMenu : MonoBehaviour
{
    [Header("Elementos")]
    [SerializeField] private RectTransform rueda;
    [SerializeField] private CanvasGroup menuPrincipal;

    [Header("Gestor")]
    [SerializeField] private GestorPaneles gestorPaneles;

    [Header("Zoom")]
    [SerializeField] private float duracionZoom = 0.7f;
    [SerializeField] private float escalaFinal = 2.5f;

    [Header("Desvanecimiento")]
    [SerializeField] private float duracionFade = 0.35f;

    [Header("Salida")]
    [SerializeField] private float duracionSalida = 0.7f;
    [SerializeField] private float escalaSalida = 0.5f;

    private readonly Stack<CanvasGroup> historial =
        new Stack<CanvasGroup>();

    private bool enTransicion;
    private Vector3 escalaInicial;
    private Vector2 posicionInicial;

    public bool EnTransicion => enTransicion;

    public void Inicializar(
        RectTransform referenciaRueda,
        CanvasGroup principal,
        GestorPaneles gestor)
    {
        rueda = referenciaRueda;
        menuPrincipal = principal;
        gestorPaneles = gestor;

        if (rueda == null ||
            menuPrincipal == null ||
            gestorPaneles == null)
        {
            Debug.LogError(
                "TransicionMenu: faltan referencias.",
                this
            );
            return;
        }

        escalaInicial = rueda.localScale;
        posicionInicial = rueda.anchoredPosition;
        historial.Clear();
        enTransicion = false;
    }

    public void AbrirPanel(
        GameObject panelDestino,
        RectTransform botonSeleccionado)
    {
        if (gestorPaneles == null ||
            gestorPaneles.PanelActual != menuPrincipal)
            return;

        if (botonSeleccionado == null)
            return;

        SolicitarCambio(
            panelDestino,
            botonSeleccionado,
            true
        );
    }

    public void AbrirPanelDesdeOtroPanel(
        GameObject panelDestino)
    {
        SolicitarCambio(
            panelDestino,
            null,
            false
        );
    }

    private void SolicitarCambio(
        GameObject objeto,
        RectTransform boton,
        bool conZoom)
    {
        if (enTransicion ||
            objeto == null ||
            gestorPaneles == null)
            return;

        CanvasGroup destino =
            objeto.GetComponent<CanvasGroup>();

        if (destino == null)
        {
            Debug.LogError(
                "TransicionMenu: el destino no tiene CanvasGroup.",
                this
            );
            return;
        }

        if (!gestorPaneles.EstaRegistrado(destino))
        {
            Debug.LogError(
                "Pantalla no registrada: " + destino.name,
                this
            );
            return;
        }

        CanvasGroup origen = gestorPaneles.PanelActual;

        if (origen == null || origen == destino)
            return;

        StartCoroutine(
            Cambiar(origen, destino, boton, conZoom)
        );
    }

    private IEnumerator Cambiar(
        CanvasGroup origen,
        CanvasGroup destino,
        RectTransform boton,
        bool conZoom)
    {
        enTransicion = true;

        Debug.Log(
            "Navegacion: " + origen.name +
            " -> " + destino.name
        );

        origen.interactable = false;
        origen.blocksRaycasts = false;

        if (conZoom && rueda != null && boton != null)
        {
            Vector3 escalaObjetivo =
                escalaInicial * escalaFinal;

            Vector3 posicionObjetivo =
                CalcularPosicionObjetivo(boton);

            yield return AnimarZoom(
                escalaObjetivo,
                posicionObjetivo,
                duracionZoom
            );
        }

        yield return AnimarAlpha(
            origen,
            origen.alpha,
            0f,
            duracionFade
        );

        historial.Push(origen);

        // Oculta TODAS las pantallas salvo el destino.
        gestorPaneles.MostrarPanel(destino);

        // El nuevo panel entra con un fade.
        destino.alpha = 0f;
        destino.interactable = false;
        destino.blocksRaycasts = false;

        yield return AnimarAlpha(
            destino,
            0f,
            1f,
            duracionFade
        );

        destino.interactable = true;
        destino.blocksRaycasts = true;

        enTransicion = false;
    }

    public void VolverPanelAnterior()
    {
        if (enTransicion || historial.Count == 0)
            return;

        StartCoroutine(Volver());
    }

    private IEnumerator Volver()
    {
        enTransicion = true;

        CanvasGroup origen = gestorPaneles.PanelActual;
        CanvasGroup destino = historial.Pop();

        if (origen == null || destino == null)
        {
            enTransicion = false;
            yield break;
        }

        origen.interactable = false;
        origen.blocksRaycasts = false;

        yield return AnimarAlpha(
            origen,
            origen.alpha,
            0f,
            duracionFade
        );

        gestorPaneles.MostrarPanel(destino);

        if (destino == menuPrincipal && rueda != null)
        {
            rueda.localScale = escalaInicial;
            rueda.anchoredPosition = posicionInicial;
        }

        destino.alpha = 0f;
        destino.interactable = false;
        destino.blocksRaycasts = false;

        yield return AnimarAlpha(
            destino,
            0f,
            1f,
            duracionFade
        );

        destino.interactable = true;
        destino.blocksRaycasts = true;

        enTransicion = false;

        Debug.Log("Regreso a: " + destino.name);
    }

    private Vector3 CalcularPosicionObjetivo(
        RectTransform boton)
    {
        Transform padre = rueda.parent;

        Vector3 centro =
            padre.InverseTransformPoint(rueda.position);

        Vector3 posicionBoton =
            padre.InverseTransformPoint(boton.position);

        Vector3 objetivo =
            centro +
            (centro - posicionBoton) *
            (escalaFinal - 1f);

        return padre.TransformPoint(objetivo);
    }

    private IEnumerator AnimarZoom(
        Vector3 escalaObjetivo,
        Vector3 posicionObjetivo,
        float duracion)
    {
        Vector3 escalaOrigen = rueda.localScale;
        Vector3 posicionOrigen = rueda.position;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                tiempo / Mathf.Max(0.01f, duracion)
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            rueda.localScale = Vector3.Lerp(
                escalaOrigen,
                escalaObjetivo,
                t
            );

            rueda.position = Vector3.Lerp(
                posicionOrigen,
                posicionObjetivo,
                t
            );

            yield return null;
        }

        rueda.localScale = escalaObjetivo;
        rueda.position = posicionObjetivo;
    }

    private IEnumerator AnimarAlpha(
        CanvasGroup panel,
        float desde,
        float hasta,
        float duracion)
    {
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                tiempo / Mathf.Max(0.01f, duracion)
            );

            t = Mathf.SmoothStep(0f, 1f, t);
            panel.alpha = Mathf.Lerp(desde, hasta, t);

            yield return null;
        }

        panel.alpha = hasta;
    }

    public void SalirDelJuego()
    {
        if (enTransicion || rueda == null)
            return;

        StartCoroutine(ZoomSalida());
    }

    private IEnumerator ZoomSalida()
    {
        enTransicion = true;

        Vector3 escalaObjetivo =
            rueda.localScale * escalaSalida;

        Vector3 posicionObjetivo =
            rueda.position + Vector3.up * 300f;

        yield return AnimarZoom(
            escalaObjetivo,
            posicionObjetivo,
            duracionSalida
        );

        if (menuPrincipal != null)
        {
            yield return AnimarAlpha(
                menuPrincipal,
                menuPrincipal.alpha,
                0f,
                duracionFade
            );
        }

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
