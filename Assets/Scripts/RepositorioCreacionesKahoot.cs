using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public sealed class CreacionGuardada
{
    public string Ruta { get; private set; }
    public KahootDatos Datos { get; private set; }

    public CreacionGuardada(string ruta, KahootDatos datos)
    {
        Ruta = ruta;
        Datos = datos;
    }
}

public static class RepositorioCreacionesKahoot
{
    public const int MaxCaracteresTitulo = 50;

    public static bool EsEditable(string ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta)) return false;
        try
        {
            string carpeta = Path.GetFullPath(AlmacenKahoots.CarpetaKahoots)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string completa = Path.GetFullPath(ruta);
            if (!completa.StartsWith(carpeta, StringComparison.OrdinalIgnoreCase))
                return false;
            if (!string.Equals(Path.GetExtension(completa), ".json", StringComparison.OrdinalIgnoreCase))
                return false;
            string nombre = Path.GetFileName(completa);
            return !string.Equals(nombre, "Quiz_Ejemplo_Sencillo.json", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(nombre, "Base_General_10_Preguntas.json", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception) { return false; }
    }

    public static List<CreacionGuardada> Obtener()
    {
        var lista = new List<CreacionGuardada>();
        try
        {
            Directory.CreateDirectory(AlmacenKahoots.CarpetaKahoots);
            foreach (string ruta in Directory.GetFiles(AlmacenKahoots.CarpetaKahoots, "*.json"))
            {
                if (!EsEditable(ruta)) continue;
                KahootDatos datos;
                if (AlmacenKahoots.IntentarCargar(ruta, out datos))
                    lista.Add(new CreacionGuardada(ruta, datos));
            }
            lista.Sort((a, b) => string.Compare(a.Datos.titulo, b.Datos.titulo,
                StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            AlmacenKahoots.RegistrarError("Error leyendo mis creaciones", ex);
            Debug.LogError("RepositorioCreacionesKahoot: " + ex.Message);
        }
        return lista;
    }

    public static bool IntentarAbrir(string ruta, out KahootDatos datos)
    {
        datos = null;
        return EsEditable(ruta) && AlmacenKahoots.IntentarCargar(ruta, out datos);
    }

    public static bool TituloEnUso(string titulo, string excluirRuta = null)
    {
        foreach (CreacionGuardada item in Obtener())
        {
            if (!string.Equals(item.Datos.titulo.Trim(), titulo.Trim(),
                StringComparison.OrdinalIgnoreCase)) continue;
            if (excluirRuta != null && string.Equals(item.Ruta, excluirRuta,
                StringComparison.OrdinalIgnoreCase)) continue;
            return true;
        }
        return false;
    }

    public static void Actualizar(string ruta, KahootDatos datos)
    {
        if (!EsEditable(ruta) || !File.Exists(ruta))
            throw new IOException("Este Kahoot no es editable o no existe.");
        if (!AlmacenKahoots.Validar(datos, out string error))
            throw new InvalidDataException(error);
        if (datos.titulo.Trim().Length > MaxCaracteresTitulo)
            throw new InvalidDataException("El titulo supera los 50 caracteres.");
        File.WriteAllText(ruta, JsonUtility.ToJson(datos, true), new UTF8Encoding(false));
    }

    public static bool Eliminar(string ruta)
    {
        if (!EsEditable(ruta))
        {
            Debug.LogError("No se permite borrar este Kahoot: " + ruta);
            return false;
        }
        try
        {
            if (!File.Exists(ruta))
            {
                Debug.LogWarning("Ya no existe el JSON: " + ruta);
                return false;
            }
            File.Delete(ruta);
            Debug.Log("Kahoot eliminado del disco: " + ruta);
            return true;
        }
        catch (Exception ex)
        {
            AlmacenKahoots.RegistrarError("Error al borrar Kahoot: " + ruta, ex);
            Debug.LogError("No se pudo eliminar el Kahoot: " + ex.Message);
            return false;
        }
    }
}
