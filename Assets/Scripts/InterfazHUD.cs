
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class InterfazHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text textoTiempo;
    [SerializeField] private TMP_Text textoPuntos;
    [SerializeField] private TMP_Text textoNumero;
    [SerializeField] private TMP_Text textoEnunciado;

    private CanvasGroup grupo;

    private void Awake()
    {
        grupo = GetComponent<CanvasGroup>();
        Mostrar(false);
    }

    public void Preparar(
        string enunciado,
        int numero,
        int puntos,
        float tiempo)
    {
        if (textoEnunciado != null)
            textoEnunciado.text = enunciado;

        if (textoNumero != null)
            textoNumero.text = numero.ToString();

        ActualizarPuntos(puntos);
        ActualizarTiempo(tiempo);
    }

    public void ActualizarTiempo(float tiempo)
    {
        if (textoTiempo != null)
            textoTiempo.text =
                Mathf.CeilToInt(tiempo) + " s";
    }

    public void ActualizarPuntos(int puntos)
    {
        if (textoPuntos != null)
            textoPuntos.text = puntos + " puntos";
    }

    public void Mostrar(bool visible)
    {
        if (grupo == null)
            grupo = GetComponent<CanvasGroup>();

        grupo.alpha = visible ? 1f : 0f;
        grupo.interactable = visible;
        grupo.blocksRaycasts = visible;
    }
}
