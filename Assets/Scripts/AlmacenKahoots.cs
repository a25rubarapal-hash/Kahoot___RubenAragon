using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class AlmacenKahoots
{
    public static string CarpetaKahoots =>
        Path.Combine(Application.persistentDataPath, "Kahoots");

    public static string CarpetaInformes =>
        Path.Combine(Application.persistentDataPath, "Informes");

    public static string Guardar(KahootDatos kahoot)
    {
        string error;
        if (!Validar(kahoot, out error))
            throw new InvalidDataException(error);

        Directory.CreateDirectory(CarpetaKahoots);

        string baseNombre = NombreSeguro(kahoot.titulo);
        string ruta = Path.Combine(CarpetaKahoots, baseNombre + ".json");
        int sufijo = 2;
        while (File.Exists(ruta))
            ruta = Path.Combine(CarpetaKahoots, baseNombre + "_" + sufijo++ + ".json");

        File.WriteAllText(ruta, JsonUtility.ToJson(kahoot, true), new UTF8Encoding(false));
        return ruta;
    }

    public static bool IntentarCargar(string ruta, out KahootDatos kahoot)
    {
        kahoot = null;
        try
        {
            string json = File.ReadAllText(ruta, Encoding.UTF8);
            kahoot = JsonUtility.FromJson<KahootDatos>(json);
            string error;
            if (!Validar(kahoot, out error))
                throw new InvalidDataException(error);
            kahoot.claveArchivo = "local_" + Path.GetFileNameWithoutExtension(ruta);
            return true;
        }
        catch (Exception ex)
        {
            RegistrarError("Error leyendo el JSON " + ruta, ex);
            Debug.LogError("No se pudo cargar el Kahoot: " + ruta + " / " + ex.Message);
            kahoot = null;
            return false;
        }
    }

    public static List<KahootDatos> LeerKahootsLocales()
    {
        var lista = new List<KahootDatos>();
        try
        {
            Directory.CreateDirectory(CarpetaKahoots);
            foreach (string ruta in Directory.GetFiles(CarpetaKahoots, "*.json"))
            {
                KahootDatos datos;
                if (IntentarCargar(ruta, out datos)) lista.Add(datos);
            }
        }
        catch (Exception ex)
        {
            RegistrarError("Error listando los Kahoots locales", ex);
            Debug.LogError(ex);
        }
        return lista;
    }

    // Los JSON predeterminados se colocan como TextAssets en Assets/Resources/Kahoots/.
    public static List<KahootDatos> LeerKahootsPredeterminados()
    {
        var lista = new List<KahootDatos>();
        foreach (TextAsset asset in Resources.LoadAll<TextAsset>("Kahoots/SistemaSolar"))
        {
            try
            {
                KahootDatos datos = JsonUtility.FromJson<KahootDatos>(asset.text);
                string error;
                if (!Validar(datos, out error)) throw new InvalidDataException(error);
                datos.claveArchivo = "asset_" + asset.name;
                lista.Add(datos);
            }
            catch (Exception ex)
            {
                RegistrarError("Error en recurso JSON " + asset.name, ex);
                Debug.LogError("JSON predeterminado incorrecto: " + asset.name);
            }
        }
        return lista;
    }

    public static bool Validar(KahootDatos kahoot, out string error)
    {
        if (kahoot == null) { error = "Objeto Kahoot nulo o JSON corrupto."; return false; }
        if (string.IsNullOrWhiteSpace(kahoot.titulo)) { error = "El Kahoot no tiene titulo."; return false; }
        if (kahoot.preguntas == null || kahoot.preguntas.Count == 0)
        { error = "El Kahoot no tiene preguntas."; return false; }

        for (int i = 0; i < kahoot.preguntas.Count; i++)
        {
            PreguntaKahoot p = kahoot.preguntas[i];
            string prefijo = "Pregunta " + (i + 1) + ": ";
            if (p == null) { error = prefijo + "es nula."; return false; }
            if (p.tipo != "ABCD" && p.tipo != "TF")
            { error = prefijo + "tipo invalido (ABCD/TF)."; return false; }
            if (string.IsNullOrWhiteSpace(p.enunciado))
            { error = prefijo + "falta enunciado."; return false; }
            if (float.IsNaN(p.duracion) || float.IsInfinity(p.duracion) || p.duracion <= 0)
            { error = prefijo + "duracion invalida."; return false; }
            int cantidad = p.tipo == "TF" ? 2 : 4;
            if (p.respuestas == null || p.respuestas.Count != cantidad)
            { error = prefijo + "cantidad de respuestas incorrecta."; return false; }
            for (int j = 0; j < cantidad; j++)
                if (string.IsNullOrWhiteSpace(p.respuestas[j]))
                { error = prefijo + "respuesta vacia en " + j; return false; }
            if (p.respuestaCorrecta < 0 || p.respuestaCorrecta >= cantidad)
            { error = prefijo + "indice de respuesta correcta invalido."; return false; }
        }
        error = null;
        return true;
    }

    public static void RegistrarError(string contexto, Exception excepcion)
    {
        try
        {
            Directory.CreateDirectory(CarpetaInformes);
            string nombre = "Error_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") +
                "_" + Guid.NewGuid().ToString("N").Substring(0, 6) + ".txt";
            string texto = "Fecha: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                Environment.NewLine + "Contexto: " + contexto +
                Environment.NewLine + "Tipo: " + excepcion.GetType().Name +
                Environment.NewLine + "Detalle: " + excepcion.Message +
                Environment.NewLine + Environment.NewLine + excepcion;
            File.WriteAllText(Path.Combine(CarpetaInformes, nombre), texto);
        }
        catch (Exception e) { Debug.LogError("No se pudo generar informe: " + e.Message); }
    }

    private static string NombreSeguro(string nombre)
    {
        var resultado = new StringBuilder();
        foreach (char c in nombre.Trim())
        {
            if (char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_')
                resultado.Append(c);
            else
                resultado.Append('_');
            if (resultado.Length >= 60) break;
        }
        string limpio = resultado.ToString().Trim().Trim('.');
        return string.IsNullOrEmpty(limpio) ? "Kahoot" : limpio;
    }
}
