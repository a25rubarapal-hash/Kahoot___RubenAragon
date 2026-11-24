using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CatalogoKahoots
{
    public List<KahootDatos> ObtenerDisponibles()
    {
        var unicos = new Dictionary<string, KahootDatos>(StringComparer.OrdinalIgnoreCase);
        foreach (KahootDatos baseQuiz in AlmacenKahoots.LeerKahootsPredeterminados())
            Agregar(unicos, baseQuiz);
        try
        {
            Directory.CreateDirectory(AlmacenKahoots.CarpetaKahoots);
            foreach (string ruta in Directory.GetFiles(AlmacenKahoots.CarpetaKahoots, "*.json"))
            {
                // No mostrar el ejemplo obsoleto generado por el script anterior.
                if (string.Equals(Path.GetFileName(ruta), "Base_General_10_Preguntas.json",
                    StringComparison.OrdinalIgnoreCase)) continue;
                if (AlmacenKahoots.IntentarCargar(ruta, out KahootDatos quiz))
                    {
                        if (quiz.claveArchivo == "local_Quiz_Ejemplo_Sencillo" && JsonUtility.ToJson(quiz) == JsonUtility.ToJson(KahootsEjemplo.CrearSencillo())) continue;
                        Agregar(unicos, quiz);
                    }
            }
        }
        catch (Exception ex)
        {
            AlmacenKahoots.RegistrarError("Error cargando catalogo", ex);
            Debug.LogError(ex);
        }
        var lista = new List<KahootDatos>(unicos.Values);
        lista.Sort((a,b) => string.Compare(a.titulo, b.titulo, StringComparison.OrdinalIgnoreCase));
        int ejemplo = lista.FindIndex(q => q.claveArchivo == "asset_SistemaSolar");
        if (ejemplo > 0) { var quiz = lista[ejemplo]; lista.RemoveAt(ejemplo); lista.Insert(0, quiz); }
        return lista;
    }

    private static void Agregar(Dictionary<string, KahootDatos> unicos, KahootDatos quiz)
    {
        if (quiz == null || string.IsNullOrWhiteSpace(quiz.titulo)) return;
        string clave = quiz.titulo.Trim();
        if (!unicos.ContainsKey(clave)) unicos.Add(clave, quiz);
        else Debug.LogWarning("Catalogo: titulo duplicado, omitido visualmente: " + clave);
    }
}
