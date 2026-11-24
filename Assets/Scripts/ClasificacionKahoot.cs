using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public static class ClasificacionKahoot
{
    public static string Jugador = "Jugador";
    static string Ruta(KahootDatos quiz) => Path.Combine(Application.persistentDataPath,
        "Resultados", quiz.claveArchivo + ".xml");

    static XElement Leer(KahootDatos quiz)
    {
        string ruta = Ruta(quiz);
        if (!File.Exists(ruta)) return new XElement("resultados");
        var raiz = XElement.Load(ruta);
        if (raiz.Name != "resultados") throw new InvalidDataException("Raiz XML incorrecta");
        foreach (var fila in raiz.Elements("partida"))
        {
            if (fila.Attribute("jugador") == null || fila.Attribute("puntos") == null ||
                fila.Attribute("segundos") == null) throw new InvalidDataException("Partida incompleta");
            _ = (int)fila.Attribute("puntos");
            _ = (double)fila.Attribute("segundos");
        }
        return raiz;
    }

    static IOrderedEnumerable<XElement> Ordenar(XElement raiz) =>
        raiz.Elements("partida").OrderByDescending(x => (int)x.Attribute("puntos"))
            .ThenBy(x => (double)x.Attribute("segundos"));

    public static string Guardar(KahootDatos quiz, int puntos, float segundos)
    {
        string ruta = Ruta(quiz);
        string temporal = ruta + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ruta));
            var raiz = Leer(quiz);
            var fila = new XElement("partida", new XAttribute("jugador",
                string.IsNullOrWhiteSpace(Jugador) ? "Jugador" : Jugador.Trim()),
                new XAttribute("puntos", puntos), new XAttribute("segundos", segundos));
            raiz.Add(fila);
            raiz.Save(temporal);
            if (File.Exists(ruta)) File.Replace(temporal, ruta, null);
            else File.Move(temporal, ruta);
            return "Puesto " + (Array.IndexOf(Ordenar(raiz).ToArray(), fila) + 1) + " de " + raiz.Elements("partida").Count();
        }
        catch (Exception ex)
        {
            AlmacenKahoots.RegistrarError("Guardando resultados " + ruta, ex);
            return "No se pudo guardar la puntuacion.";
        }
        finally { if (File.Exists(temporal)) File.Delete(temporal); }
    }

    public sealed class Marca
    {
        public string Jugador;
        public int Puntos;
        public double Segundos;
    }

    public static System.Collections.Generic.List<Marca> LeerTabla(KahootDatos quiz, out string estado)
    {
        estado = null;
        try
        {
            return Ordenar(Leer(quiz)).Select(x => new Marca {
                Jugador = (string)x.Attribute("jugador"),
                Puntos = (int)x.Attribute("puntos"),
                Segundos = (double)x.Attribute("segundos")
            }).ToList();
        }
        catch (Exception ex)
        {
            AlmacenKahoots.RegistrarError("Leyendo resultados " + Ruta(quiz), ex);
            estado = "No se pudo leer la clasificación.";
            return new System.Collections.Generic.List<Marca>();
        }
    }

    public static string Consultar(KahootDatos quiz)
    {
        try
        {
            var filas = Ordenar(Leer(quiz)).Select((x, i) =>
                $"{i + 1}. {(string)x.Attribute("jugador")}   {(int)x.Attribute("puntos")} pts   {(double)x.Attribute("segundos"):0.0} s");
            string tabla = string.Join("\n", filas);
            return tabla.Length == 0 ? "Todavia no hay partidas." : tabla;
        }
        catch (Exception ex)
        {
            AlmacenKahoots.RegistrarError("Leyendo resultados " + Ruta(quiz), ex);
            return "No se pudo leer la clasificacion.";
        }
    }
}
