
using System;
using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class GestorAviso : MonoBehaviour
{
    [SerializeField] private TMP_Text textoPrincipal;
    [SerializeField] private TMP_Text textoSombra;
    [SerializeField, Min(0.1f)] private float duracion = 2f;

    private CanvasGroup grupo;
    private Coroutine rutina;

    private void Awake()
    {
        grupo = GetComponent<CanvasGroup>();
        Ocultar();
    }

    public void IniciarAviso(
        string tipo,
        Action alFinalizar,
        Func<bool> puedeAvanzar)
    {
        Cancelar();

        if (textoPrincipal != null)
            textoPrincipal.text = tipo;

        if (textoSombra != null)
            textoSombra.text = tipo;

        Mostrar(true);

        rutina = StartCoroutine(
            Esperar(alFinalizar, puedeAvanzar)
        );
    }

    private IEnumerator Esperar(
        Action alFinalizar,
        Func<bool> puedeAvanzar)
    {
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            if (puedeAvanzar == null || puedeAvanzar())
                tiempo += Time.unscaledDeltaTime;

            yield return null;
        }

        rutina = null;

        Ocultar();
        alFinalizar?.Invoke();
    }

    public void Cancelar()
    {
        if (rutina != null)
            StopCoroutine(rutina);

        rutina = null;
        Ocultar();
    }

    public void Ocultar()
    {
        Mostrar(false);
    }

    private void Mostrar(bool visible)
    {
        if (grupo == null)
            grupo = GetComponent<CanvasGroup>();

        grupo.alpha = visible ? 1f : 0f;
        grupo.interactable = visible;
        grupo.blocksRaycasts = visible;
    }
}
