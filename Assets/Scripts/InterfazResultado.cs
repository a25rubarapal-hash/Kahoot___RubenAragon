
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InterfazResultado : MonoBehaviour
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
        int puntos)
    {
        Ocultar();

        if (textoBien != null)
        {
            textoBien.text =
                "¡Quiz terminado!" +
                "\nAciertos: " + aciertos + "/" + cantidad;
        }

        if (puntosBien != null)
            puntosBien.text = "Total: " + puntos + " puntos";

        if (siguienteBien != null)
            siguienteBien.gameObject.SetActive(false);

        MostrarGrupo(panelBien, true);
    }

    public void Ocultar()
    {
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
