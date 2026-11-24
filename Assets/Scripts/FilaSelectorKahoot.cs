
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FilaSelectorKahoot : MonoBehaviour
{
    [SerializeField] private TMP_Text nombreQuiz;
    [SerializeField] private Button botonPlay;

    private KahootDatos datos;
    private Action<KahootDatos> alSeleccionar;

    public void Configurar(
        KahootDatos quiz,
        Action<KahootDatos> callback)
    {
        transform.localScale = Vector3.one;
        var layout = GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 80;
        if (nombreQuiz != null)
        {
            Ajustar(nombreQuiz.rectTransform, .025f, .70f);
            nombreQuiz.enableAutoSizing = true;
            nombreQuiz.fontSizeMin = 16;
            nombreQuiz.fontSizeMax = 30;
            nombreQuiz.overflowMode = TextOverflowModes.Ellipsis;
        }
        if (botonPlay != null) Ajustar((RectTransform)botonPlay.transform, .73f, .97f);
        datos = quiz;
        alSeleccionar = callback;

        if (nombreQuiz != null)
            nombreQuiz.text = datos != null
                ? datos.titulo
                : "Sin cuestionario";

        if (botonPlay == null)
        {
            Debug.LogError(
                "Falta PlayButton en " + gameObject.name,
                this
            );
            return;
        }

        botonPlay.onClick.RemoveListener(Seleccionar);
        botonPlay.onClick.AddListener(Seleccionar);
        botonPlay.interactable = datos != null;
    }

    private static void Ajustar(RectTransform rect, float izquierda, float derecha)
    {
        rect.localScale = Vector3.one;
        rect.anchorMin = new Vector2(izquierda, .12f);
        rect.anchorMax = new Vector2(derecha, .88f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private void Seleccionar()
    {
        if (datos == null || alSeleccionar == null)
        {
            Debug.LogError(
                "La fila no tiene un Kahoot configurado.",
                this
            );
            return;
        }

        Debug.Log("PLAY pulsado: " + datos.titulo);

        alSeleccionar.Invoke(datos);
    }

    private void OnDestroy()
    {
        if (botonPlay != null)
            botonPlay.onClick.RemoveListener(Seleccionar);
    }
}
