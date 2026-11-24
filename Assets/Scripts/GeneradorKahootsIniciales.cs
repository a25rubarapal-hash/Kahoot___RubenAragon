
using System;
using System.IO;
using System.Text;
using UnityEngine;

public class GeneradorKahootsIniciales : MonoBehaviour
{
    public static string RutaSencilla =>
        Path.Combine(
            AlmacenKahoots.CarpetaKahoots,
            "Quiz_Ejemplo_Sencillo.json"
        );

    public static string RutaSecreta =>
        Path.Combine(
            Application.persistentDataPath,
            "KahootsOcultos",
            "Quiz_Prueba_Completa_F5.json"
        );

    private void Awake()
    {


        GenerarSiNoExiste(
            KahootsEjemplo.CrearPruebaCompleta(),
            RutaSecreta
        );
    }

    private void GenerarSiNoExiste(
        KahootDatos datos,
        string ruta)
    {
        try
        {
            if (File.Exists(ruta))
                return;

            if (!AlmacenKahoots.Validar(
                datos,
                out string error))
            {
                throw new InvalidDataException(error);
            }

            string carpeta = Path.GetDirectoryName(ruta);

            Directory.CreateDirectory(carpeta);

            string json = JsonUtility.ToJson(
                datos,
                true
            );

            File.WriteAllText(
                ruta,
                json,
                new UTF8Encoding(false)
            );

            Debug.Log(
                "Kahoot generado: " + ruta
            );
        }
        catch (Exception ex)
        {
            AlmacenKahoots.RegistrarError(
                "Error creando Kahoot inicial",
                ex
            );

            Debug.LogError(
                "No se pudo generar el Kahoot: " +
                ex.Message
            );
        }
    }
}
