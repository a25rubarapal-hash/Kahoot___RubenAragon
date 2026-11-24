
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class InterfazPregunta : MonoBehaviour
{
    [Header("Pregunta")]
    [SerializeField] private TMP_Text textoEnunciado;
    [SerializeField] private Button[] botones;
    [SerializeField] private TMP_Text[] textosRespuestas;

    [Header("Colores")]
    [SerializeField]
    private Color colorCorrecto =
        new Color(0.12f, 0.8f, 0.25f, 1f);

    [SerializeField]
    private Color colorIncorrecto =
        new Color(0.9f, 0.15f, 0.15f, 1f);

    private struct Aspecto
    {
        public Color color;
        public Selectable.Transition transicion;
    }

    private CanvasGroup grupo;
    private Action<int> alResponder;

    private readonly Dictionary<Button, Aspecto> aspectos =
        new Dictionary<Button, Aspecto>();

    private readonly Dictionary<Button, UnityAction> eventos =
        new Dictionary<Button, UnityAction>();

    private void Awake()
    {
        grupo = GetComponent<CanvasGroup>();

        if (botones == null)
            return;

        for (int i = 0; i < botones.Length; i++)
        {
            Button boton = botones[i];

            if (boton == null || eventos.ContainsKey(boton))
                continue;

            aspectos.Add(boton, new Aspecto
            {
                color = boton.targetGraphic != null
                    ? boton.targetGraphic.color
                    : Color.white,

                transicion = boton.transition
            });

            int indice = i;

            UnityAction accion = () =>
                alResponder?.Invoke(indice);

            boton.onClick.AddListener(accion);
            eventos.Add(boton, accion);
        }

        Mostrar(false);
    }

    public bool Configurar(
        PreguntaPreparada pregunta,
        Action<int> callback)
    {
        if (pregunta == null || botones == null ||
            botones.Length != pregunta.Respuestas.Count)
        {
            Debug.LogError(
                "InterfazPregunta: cantidad de respuestas incorrecta.",
                this
            );

            return false;
        }

        alResponder = callback;

        if (textoEnunciado != null)
            textoEnunciado.text = pregunta.Original.enunciado;

        for (int i = 0; i < botones.Length; i++)
        {
            Button boton = botones[i];

            if (boton == null)
                return false;

            TMP_Text texto =
                textosRespuestas != null &&
                i < textosRespuestas.Length
                    ? textosRespuestas[i]
                    : null;

            if (texto == null)
                texto = boton.GetComponentInChildren<TMP_Text>(true);

            if (texto == null)
            {
                Debug.LogError(
                    "Falta el texto de una respuesta.",
                    this
                );

                return false;
            }

            texto.text = pregunta.Respuestas[i];

            if (aspectos.TryGetValue(boton, out Aspecto aspecto))
            {
                boton.transition = aspecto.transicion;

                if (boton.targetGraphic != null)
                    boton.targetGraphic.color = aspecto.color;
            }

            boton.interactable = true;
        }

        return true;
    }

    public void MostrarSolucion(int seleccion, int correcta)
    {
        if (botones == null)
            return;

        grupo.interactable = false;
        grupo.blocksRaycasts = false;

        for (int i = 0; i < botones.Length; i++)
        {
            Button boton = botones[i];

            if (boton == null)
                continue;

            boton.interactable = false;
            boton.transition = Selectable.Transition.None;

            if (boton.targetGraphic == null)
                continue;

            if (i == correcta)
                boton.targetGraphic.color = colorCorrecto;
            else if (i == seleccion)
                boton.targetGraphic.color = colorIncorrecto;
        }
    }

    public void Mostrar(bool visible)
    {
        if (grupo == null)
            grupo = GetComponent<CanvasGroup>();

        grupo.alpha = visible ? 1f : 0f;
        grupo.interactable = visible;
        grupo.blocksRaycasts = visible;
    }

    private void OnDestroy()
    {
        foreach (var evento in eventos)
        {
            if (evento.Key != null)
                evento.Key.onClick.RemoveListener(evento.Value);
        }

        eventos.Clear();
    }
}
