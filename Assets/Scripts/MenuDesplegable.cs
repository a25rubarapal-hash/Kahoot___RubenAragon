
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AudioDropdown : MonoBehaviour
{
    [Header("Boton")]
    [SerializeField] private Button boton;
    [SerializeField] private RawImage imagenBoton;
    [SerializeField] private Texture imagenCerrada;
    [SerializeField] private Texture imagenAbierta;

    [Header("Sliders")]
    [SerializeField] private RectTransform sliderMusica;
    [SerializeField] private RectTransform sliderEfectos;

    [Header("Animacion")]
    [SerializeField] private float duracion = 0.3f;
    [SerializeField] private float distanciaDespliegue = 100f;

    private Vector2 posicionMusica;
    private Vector2 posicionEfectos;

    private bool abierto = false;
    private Coroutine animacion;

    private void Start()
    {
        // Guardamos las posiciones originales.
        posicionMusica = sliderMusica.anchoredPosition;
        posicionEfectos = sliderEfectos.anchoredPosition;

        // Empezamos plegados.
        sliderMusica.gameObject.SetActive(false);
        sliderEfectos.gameObject.SetActive(false);

        imagenBoton.texture = imagenCerrada;

        boton.onClick.AddListener(Alternar);
    }

    public void Alternar()
    {
        abierto = !abierto;

        if (animacion != null)
            StopCoroutine(animacion);

        animacion = StartCoroutine(Animar());
    }

    private IEnumerator Animar()
    {
        Vector2 inicioMusica = sliderMusica.anchoredPosition;
        Vector2 inicioEfectos = sliderEfectos.anchoredPosition;

        // Activamos los sliders antes de desplegar.
        if (abierto)
        {
            sliderMusica.gameObject.SetActive(true);
            sliderEfectos.gameObject.SetActive(true);
        }

        Vector2 destinoMusica = abierto
            ? posicionMusica + Vector2.down * distanciaDespliegue
            : posicionMusica;

        Vector2 destinoEfectos = abierto
            ? posicionEfectos + Vector2.down * distanciaDespliegue
            : posicionEfectos;

        // Imagen del boton.
        imagenBoton.texture = abierto
            ? imagenAbierta : imagenCerrada;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / duracion);
            t = Mathf.SmoothStep(0f, 1f, t);

            sliderMusica.anchoredPosition =
                Vector2.Lerp(inicioMusica, destinoMusica, t);

            sliderEfectos.anchoredPosition =
                Vector2.Lerp(inicioEfectos, destinoEfectos, t);

            yield return null;
        }

        sliderMusica.anchoredPosition = destinoMusica;
        sliderEfectos.anchoredPosition = destinoEfectos;

        if (!abierto)
        {
            sliderMusica.gameObject.SetActive(false);
            sliderEfectos.gameObject.SetActive(false);

            // Restauramos las posiciones originales.
            sliderMusica.anchoredPosition = posicionMusica;
            sliderEfectos.anchoredPosition = posicionEfectos;
        }

        animacion = null;
    }
}
