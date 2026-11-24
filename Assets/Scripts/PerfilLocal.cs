using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerfilLocal : MonoBehaviour
{
    const string Preferencia = "Kahoot.PerfilLocal";
    string nombreActual;
    static string Archivo => Path.Combine(Application.persistentDataPath,"PerfilLocal.txt");
    TMP_InputField campo;
    Button aceptar, cerrarSesion;

    public static PerfilLocal Crear(RectTransform espacio)
    {
        var perfil = espacio.gameObject.AddComponent<PerfilLocal>();
        perfil.Preparar();
        return perfil;
    }

    void Preparar()
    {
        nombreActual = PlayerPrefs.GetString(Preferencia,"");
        try { if (File.Exists(Archivo)) nombreActual = File.ReadAllText(Archivo).Trim(); }
        catch (IOException e) { Debug.LogWarning(e.Message); }
        var caja = VistaQuiz.Panel(transform,"Nombre",0,.48f,1,1,VistaQuiz.Tarjeta);
        campo = caja.gameObject.AddComponent<TMP_InputField>();
        campo.targetGraphic = caja.GetComponent<Image>();
        var visor = VistaQuiz.Rect(caja,"Entrada",.04f,.05f,.96f,.95f);
        visor.gameObject.AddComponent<RectMask2D>();
        campo.textViewport = visor;
        campo.textComponent = (TextMeshProUGUI)VistaQuiz.Texto(visor,"",0,0,1,1,26);
        campo.placeholder = VistaQuiz.Texto(visor,"Tu nombre",0,0,1,1,24);
        campo.characterLimit = 30;
        cerrarSesion = VistaQuiz.Boton(transform,"Cerrar sesión",0,0,.45f,.38f,CerrarSesion);
        aceptar = VistaQuiz.Boton(transform,"Guardar",.55f,0,1,.38f,Entrar);
        campo.onValueChanged.AddListener(valor => aceptar.interactable = !string.IsNullOrWhiteSpace(valor));
        campo.onSubmit.AddListener(_ => Entrar());
        Refrescar();
    }

    public void AbrirFormulario()
    {
        Refrescar();
        campo.Select();
        campo.ActivateInputField();
    }

    public void Refrescar()
    {
        string nombre = nombreActual;
        ClasificacionKahoot.Jugador = nombre.Length == 0 ? "Jugador" : nombre;
        campo.SetTextWithoutNotify(nombre);
        aceptar.interactable = !string.IsNullOrWhiteSpace(nombre);
        cerrarSesion.gameObject.SetActive(nombre.Length > 0);
    }

    void Entrar()
    {
        string nombre = campo.text.Trim();
        if (nombre.Length == 0) return;
        GuardarNombre(nombre);
        Refrescar();
        MenuJugador.CerrarVentana();
    }

    void GuardarNombre(string nombre)
    {
        nombreActual = nombre;
        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(Archivo,nombre);
        }
        catch (System.Exception ex) when (ex is IOException || ex is System.UnauthorizedAccessException)
        {
            Debug.LogWarning("El nombre solo se conservará durante esta sesión. " + ex.Message);
        }
    }

    void CerrarSesion()
    {
        GuardarNombre("");
        Refrescar();
        MenuJugador.CerrarVentana();
    }
}
