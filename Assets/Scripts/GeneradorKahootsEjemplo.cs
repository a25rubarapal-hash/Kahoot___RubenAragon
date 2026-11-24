
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class GeneradorKahootsEjemplo : MonoBehaviour
{
    [Header("Sistema de cuestionarios")]
    [SerializeField] private SelectorKahoots selector;
    [SerializeField] private PartidaKahoot partida;

    [Header("Navegacion")]
    [SerializeField] private TransicionMenu transicionMenu;
    [SerializeField] private GestorPaneles gestorPaneles;
    [SerializeField] private CanvasGroup panelSelector;
    [SerializeField] private GameObject panelPartida;

    private bool abriendoPrueba;

    private string RutaBase =>
        Path.Combine(
            AlmacenKahoots.CarpetaKahoots,
            "Base_General_10_Preguntas.json"
        );

    private string RutaPrueba =>
        Path.Combine(
            Application.persistentDataPath,
            "KahootsOcultos",
            "Prueba_Oculta.json"
        );

    private IEnumerator Start()
    {

        GuardarSiNoExiste(CrearPruebaOculta(), RutaPrueba);

        yield return null;

        if (selector != null)
            selector.RefrescarListado();
    }

    private void Update()
    {
        Keyboard teclado = Keyboard.current;

        if (teclado == null)
            return;

        bool control =
            teclado.leftCtrlKey.isPressed ||
            teclado.rightCtrlKey.isPressed;

        bool shift =
            teclado.leftShiftKey.isPressed ||
            teclado.rightShiftKey.isPressed;

        if (control && shift && teclado.tKey.wasPressedThisFrame)
            AbrirPruebaOculta();
    }

    private void GuardarSiNoExiste(KahootDatos datos, string ruta)
    {
        try
        {
            if (File.Exists(ruta))
                return;

            if (!AlmacenKahoots.Validar(datos, out string error))
                throw new InvalidDataException(error);

            Directory.CreateDirectory(Path.GetDirectoryName(ruta));

            File.WriteAllText(
                ruta,
                JsonUtility.ToJson(datos, true),
                new UTF8Encoding(false)
            );

            Debug.Log("Cuestionario creado: " + ruta);
        }
        catch (Exception ex)
        {
            AlmacenKahoots.RegistrarError(
                "Error creando cuestionario de ejemplo",
                ex
            );

            Debug.LogError(ex.Message);
        }
    }

    private void AbrirPruebaOculta()
    {
        if (abriendoPrueba)
            return;

        if (partida == null ||
            transicionMenu == null ||
            gestorPaneles == null ||
            panelSelector == null ||
            panelPartida == null)
        {
            Debug.LogError(
                "GeneradorKahootsEjemplo: faltan referencias."
            );
            return;
        }

        if (gestorPaneles.PanelActual != panelSelector)
            return;

        if (transicionMenu.EnTransicion)
            return;

        CanvasGroup grupoPartida =
            panelPartida.GetComponent<CanvasGroup>();

        if (grupoPartida == null)
        {
            Debug.LogError(
                "El panel de partida necesita CanvasGroup."
            );
            return;
        }

        if (!AlmacenKahoots.IntentarCargar(
            RutaPrueba,
            out KahootDatos prueba))
        {
            return;
        }

        abriendoPrueba = true;

        transicionMenu.AbrirPanelDesdeOtroPanel(panelPartida);

        StartCoroutine(
            EsperarTransicionYComenzar(prueba, grupoPartida)
        );
    }

    private IEnumerator EsperarTransicionYComenzar(
        KahootDatos prueba,
        CanvasGroup grupoPartida)
    {
        yield return null;

        while (transicionMenu != null &&
               transicionMenu.EnTransicion)
        {
            yield return null;
        }

        if (gestorPaneles != null &&
            gestorPaneles.PanelActual == grupoPartida &&
            partida != null)
        {
            partida.Comenzar(prueba);
        }

        abriendoPrueba = false;
    }

    private KahootDatos CrearPruebaOculta()
    {
        KahootDatos quiz = new KahootDatos
        {
            titulo = "Prueba oculta"
        };

        quiz.preguntas.Add(ABCD(
            "¿Cuanto es 2 + 2?\n<size=55%><color=#777777>Respuesta correcta: C - 4</color></size>",
            "2", "3", "4", "5",
            2, 30f
        ));

        quiz.preguntas.Add(TF(
            "La Tierra gira alrededor del Sol.\n<size=55%><color=#777777>Respuesta correcta: True</color></size>",
            true, 30f
        ));

        return quiz;
    }

    private KahootDatos CrearBaseGeneral()
    {
        KahootDatos quiz = new KahootDatos
        {
            titulo = "Base General - 10 Preguntas"
        };

        quiz.preguntas.Add(ABCD(
            "¿Cual es la capital de Francia?",
            "Roma", "Berlin", "Paris", "Madrid",
            2, 20f
        ));

        quiz.preguntas.Add(ABCD(
            "¿Cuanto es 5 multiplicado por 3?",
            "10", "15", "20", "25",
            1, 20f
        ));

        quiz.preguntas.Add(TF(
            "El Sol es una estrella.",
            true, 15f
        ));

        quiz.preguntas.Add(ABCD(
            "¿Que planeta es conocido como el planeta rojo?",
            "Venus", "Marte", "Jupiter", "Saturno",
            1, 20f
        ));

        quiz.preguntas.Add(TF(
            "Un kilometro equivale a 100 metros.",
            false, 15f
        ));

        quiz.preguntas.Add(ABCD(
            "¿Cual es el oceano mas grande?",
            "Atlantico", "Pacifico", "Indico", "Artico",
            1, 25f
        ));

        quiz.preguntas.Add(ABCD(
            "¿Cuantos lados tiene un hexagono?",
            "4", "5", "6", "8",
            2, 20f
        ));

        quiz.preguntas.Add(TF(
            "La Luna es un satelite natural de la Tierra.",
            true, 15f
        ));

        quiz.preguntas.Add(ABCD(
            "¿Que color resulta de mezclar azul y amarillo en pintura?",
            "Rojo", "Verde", "Morado", "Naranja",
            1, 20f
        ));

        quiz.preguntas.Add(ABCD(
            "¿En que continente esta Japon?",
            "Asia", "Europa", "Africa", "Oceania",
            0, 20f
        ));

        return quiz;
    }

    private PreguntaKahoot ABCD(
        string enunciado,
        string a,
        string b,
        string c,
        string d,
        int correcta,
        float tiempo)
    {
        return new PreguntaKahoot
        {
            tipo = "ABCD",
            enunciado = enunciado,
            respuestas = new List<string> { a, b, c, d },
            respuestaCorrecta = correcta,
            duracion = tiempo
        };
    }

    private PreguntaKahoot TF(
        string enunciado,
        bool correcta,
        float tiempo)
    {
        return new PreguntaKahoot
        {
            tipo = "TF",
            enunciado = enunciado,
            respuestas = new List<string> { "True", "False" },
            respuestaCorrecta = correcta ? 0 : 1,
            duracion = tiempo
        };
    }
}
