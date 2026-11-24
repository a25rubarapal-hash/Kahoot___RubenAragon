
using System.Collections.Generic;

public static class KahootsEjemplo
{
    public static KahootDatos CrearSencillo()
    {
        KahootDatos quiz = new KahootDatos
        {
            titulo = "Mini Quiz - Cultura General"
        };

        quiz.preguntas.Add(ABCD(
            "¿Cuanto es 2 + 3?",
            "4", "5", "6", "7",
            1, 20f
        ));

        quiz.preguntas.Add(TF(
            "El Sol es una estrella.",
            true, 15f
        ));

        quiz.preguntas.Add(ABCD(
            "¿Cual es la capital de Italia?",
            "Paris", "Roma", "Madrid", "Atenas",
            1, 20f
        ));

        return quiz;
    }

    public static KahootDatos CrearPruebaCompleta()
    {
        KahootDatos quiz = new KahootDatos
        {
            titulo = "Prueba Completa F5"
        };

        quiz.preguntas.Add(ABCD(
            Pista("¿Cuanto es 2 + 2?", "4"),
            "2", "3", "4", "5",
            2, 20f
        ));

        quiz.preguntas.Add(TF(
            Pista("Las ballenas son peces.", "False"),
            false, 15f
        ));

        quiz.preguntas.Add(ABCD(
            Pista("¿Cual es la capital de Francia?", "Paris"),
            "Roma", "Paris", "Lisboa", "Madrid",
            1, 120f
        ));

        quiz.preguntas.Add(TF(
            Pista("El Sol es una estrella.", "True"),
            true, 15f
        ));

        quiz.preguntas.Add(ABCD(
            Pista("¿Cuantos lados tiene un triangulo?", "3"),
            "2", "3", "4", "5",
            1, 30f
        ));

        quiz.preguntas.Add(TF(
            Pista("Un kilometro equivale a 100 metros.", "False"),
            false, 60f
        ));

        quiz.preguntas.Add(ABCD(
            Pista("¿Cual es el planeta rojo?", "Marte"),
            "Venus", "Jupiter", "Marte", "Mercurio",
            2, 40f
        ));

        quiz.preguntas.Add(TF(
            Pista("La Tierra gira alrededor del Sol.", "True"),
            true, 20f
        ));

        quiz.preguntas.Add(ABCD(
            Pista("¿Cuanto es 9 x 9?", "81"),
            "81", "72", "90", "99",
            0, 10f
        ));

        quiz.preguntas.Add(ABCD(
            Pista(
                "¿Que instrumento mide la temperatura?",
                "Termometro"
            ),
            "Regla", "Termometro", "Brujula", "Reloj",
            1, 120f
        ));

        return quiz;
    }

    private static string Pista(
        string enunciado,
        string respuesta)
    {
        return enunciado +
            "\n<size=55%><color=#AAAAAA>Correcta: " +
            respuesta +
            "</color></size>";
    }

    private static PreguntaKahoot ABCD(
        string enunciado,
        string a,
        string b,
        string c,
        string d,
        int correcta,
        float duracion)
    {
        return new PreguntaKahoot
        {
            tipo = "ABCD",
            enunciado = enunciado,
            respuestas = new List<string>
            {
                a, b, c, d
            },
            respuestaCorrecta = correcta,
            duracion = duracion
        };
    }

    private static PreguntaKahoot TF(
        string enunciado,
        bool correcta,
        float duracion)
    {
        return new PreguntaKahoot
        {
            tipo = "TF",
            enunciado = enunciado,
            respuestas = new List<string>
            {
                "True", "False"
            },
            respuestaCorrecta = correcta ? 0 : 1,
            duracion = duracion
        };
    }
}
