
using System.Collections.Generic;
using UnityEngine;

public class GestorPreguntas
{
    private readonly List<PreguntaKahoot> preguntas =
        new List<PreguntaKahoot>();

    public int Cantidad => preguntas.Count;

    public void Cargar(KahootDatos datos)
    {
        preguntas.Clear();

        if (datos == null || datos.preguntas == null)
            return;

        preguntas.AddRange(datos.preguntas);
        Barajar();
    }

    public PreguntaKahoot Obtener(int indice)
    {
        if (indice < 0 || indice >= preguntas.Count)
            return null;

        return preguntas[indice];
    }

    public void Limpiar()
    {
        preguntas.Clear();
    }

    private void Barajar()
    {
        for (int i = preguntas.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            PreguntaKahoot temporal = preguntas[i];
            preguntas[i] = preguntas[j];
            preguntas[j] = temporal;
        }
    }
}
