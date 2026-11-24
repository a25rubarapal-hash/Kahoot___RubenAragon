
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InterfazResultados : MonoBehaviour
{
    [Header("Panel Bien")]
    [SerializeField] private CanvasGroup panelBien;
    [SerializeField] private TMP_Text textoBien;
    [SerializeField] private TMP_Text puntosBien;
    [SerializeField] private Button siguienteBien;

    [Header("Panel Mal")]
    [SerializeField] private CanvasGroup panelMal;
    [SerializeField] private TMP_Text textoMal;
    [SerializeField] private TMP_Text puntosMal;
    [SerializeField] private Button siguienteMal;

    private Action alSiguiente;
    private bool inicializado;

    public void Inicializar(Action callback)
    {
        alSiguiente = callback;

        if (!inicializado)
        {
            if (siguienteBien != null)
                siguienteBien.onClick.AddListener(Siguiente);

            if (siguienteMal != null)
                siguienteMal.onClick.AddListener(Siguiente);

            inicializado = true;
        }

        Ocultar();
    }

    private void Siguiente()
    {
        alSiguiente?.Invoke();
    }

    public void MostrarAcierto(int puntos, string respuesta)
    {
        Ocultar();

        if (textoBien != null)
            textoBien.text = "¡Correcto!\n" + respuesta;

        if (puntosBien != null)
            puntosBien.text = "+" + puntos + " puntos";

        if (siguienteBien != null)
            siguienteBien.gameObject.SetActive(true);

        MostrarGrupo(panelBien, true);
    }

    public void MostrarFallo(
        int puntos,
        string correcta,
        string elegida,
        bool tiempoAgotado)
    {
        Ocultar();

        if (textoMal != null)
        {
            textoMal.text =
                (tiempoAgotado
                    ? "Tiempo agotado"
                    : "Respuesta incorrecta") +
                "\nTu respuesta: " + elegida +
                "\nCorrecta: " + correcta;
        }

        if (puntosMal != null)
            puntosMal.text = puntos + " puntos";

        if (siguienteMal != null)
            siguienteMal.gameObject.SetActive(true);

        MostrarGrupo(panelMal, true);
    }

    public void MostrarFinal(
        int aciertos,
        int cantidad,
        int puntos, string puesto)
    {
        Ocultar();

        if (final == null)
        {
            final = VistaQuiz.Panel(transform,"Resultado final",0,0,1,1,VistaQuiz.Fondo);
            var tarjeta = VistaQuiz.Panel(final,"Resumen",.17f,.14f,.83f,.86f,VistaQuiz.Tarjeta);
            VistaQuiz.Texto(tarjeta,"Cuestionario terminado",.05f,.78f,.95f,.95f,60);
            totalFinal = VistaQuiz.Texto(tarjeta,"",.05f,.48f,.95f,.77f,96);
            aciertosFinal = VistaQuiz.Texto(tarjeta,"",.06f,.32f,.94f,.47f,38);
            puestoFinal = VistaQuiz.Texto(tarjeta,"",.06f,.21f,.94f,.32f,28);
            botonVolver = VistaQuiz.Boton(tarjeta,"Salir",.32f,.045f,.68f,.18f,() =>
            {
                var navegacion = FindFirstObjectByType<EscKey>(FindObjectsInactive.Include);
                if (navegacion != null) navegacion.Volver();
            });
        }
        totalFinal.text = puntos + " puntos";
        aciertosFinal.text = "Has acertado " + aciertos + " de " + cantidad + " preguntas";
        puestoFinal.text = puesto;
        final.SetAsLastSibling();
        final.gameObject.SetActive(true);
        botonVolver.Select();
    }

    private RectTransform final;
    private TMP_Text totalFinal, aciertosFinal, puestoFinal;
    private Button botonVolver;

    public void Ocultar()
    {
        if (final != null) final.gameObject.SetActive(false);
        MostrarGrupo(panelBien, false);
        MostrarGrupo(panelMal, false);
    }

    private void MostrarGrupo(CanvasGroup grupo, bool visible)
    {
        if (grupo == null)
            return;

        grupo.alpha = visible ? 1f : 0f;
        grupo.interactable = visible;
        grupo.blocksRaycasts = visible;
    }

    private void OnDestroy()
    {
        if (siguienteBien != null)
            siguienteBien.onClick.RemoveListener(Siguiente);

        if (siguienteMal != null)
            siguienteMal.onClick.RemoveListener(Siguiente);
    }
}
