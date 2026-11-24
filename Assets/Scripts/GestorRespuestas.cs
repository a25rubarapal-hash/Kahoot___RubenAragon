
using System.Collections.Generic;
using UnityEngine;

public class PreguntaPreparada
{
    public PreguntaKahoot Original { get; }
    public IReadOnlyList<string> Respuestas { get; }
    public int CorrectaVisible { get; }

    public bool EsTF => Original.tipo == "TF";

    public PreguntaPreparada(
        PreguntaKahoot original,
        List<string> respuestas,
        int correctaVisible)
    {
        Original = original;
        Respuestas = respuestas.AsReadOnly();
        CorrectaVisible = correctaVisible;
    }

    public string ObtenerRespuesta(int indice)
    {
        if (indice < 0 || indice >= Respuestas.Count)
            return "?";

        if (EsTF)
            return Respuestas[indice];

        return "ABCD"[indice] + " - " + Respuestas[indice];
    }
}

public class GestorRespuestas
{
    public PreguntaPreparada Preparar(PreguntaKahoot pregunta)
    {
        if (pregunta == null || pregunta.respuestas == null)
            return null;

        int cantidad = pregunta.respuestas.Count;

        List<int> indices = new List<int>();

        for (int i = 0; i < cantidad; i++)
            indices.Add(i);

        if (pregunta.tipo == "ABCD")
        {
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);

                int temporal = indices[i];
                indices[i] = indices[j];
                indices[j] = temporal;
            }
        }

        List<string> respuestas = new List<string>();
        int correctaVisible = -1;

        for (int i = 0; i < cantidad; i++)
        {
            int indiceOriginal = indices[i];

            respuestas.Add(
                pregunta.respuestas[indiceOriginal]
            );

            if (indiceOriginal == pregunta.respuestaCorrecta)
                correctaVisible = i;
        }

        return new PreguntaPreparada(
            pregunta,
            respuestas,
            correctaVisible
        );
    }

    public bool EsCorrecta(
        PreguntaPreparada pregunta,
        int seleccion)
    {
        return pregunta != null &&
               seleccion >= 0 &&
               seleccion == pregunta.CorrectaVisible;
    }
}
