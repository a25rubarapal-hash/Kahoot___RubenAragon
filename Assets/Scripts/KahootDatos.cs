using System;
using System.Collections.Generic;

// JsonUtility serializa campos publicos de estas clases.
[Serializable]
public class PreguntaKahoot
{
    public string tipo = "ABCD";        // "ABCD" o "TF"
    public string enunciado = "";
    public List<string> respuestas = new List<string>();
    public int respuestaCorrecta = 0;  // 0-3 (ABCD) o 0-1 (TF)
    public float duracion = 20f;
}

[Serializable]
public class KahootDatos
{
    [NonSerialized] public string claveArchivo;
    public string titulo = "";
    public List<PreguntaKahoot> preguntas = new List<PreguntaKahoot>();
}
