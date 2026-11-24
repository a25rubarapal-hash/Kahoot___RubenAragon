using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreadorKahoots : MonoBehaviour
{
    [Header("Cabecera")]
    [SerializeField] private TMP_InputField inputTitulo;
    [SerializeField] private TMP_InputField inputCantidadInicial;

    [Header("Filas del editor (NO el Content de Mis creaciones)")]
    [SerializeField] private FilaPreguntaKahoot prefabFila;
    [SerializeField] private Transform contenidoScrollView;
    [SerializeField] private Button botonGenerarCantidad;
    [SerializeField] private Button botonAnadirFila;
    [SerializeField] private Button botonGuardar;
    [SerializeField] private TMP_Text textoEstado;

    [Header("Selector de Kahoots: Jugar")]
    [SerializeField] private SelectorKahoots selector;

    [Header("Opciones")]
    [SerializeField] private bool reiniciarTrasGuardar = true;

    private readonly List<FilaPreguntaKahoot> filas = new List<FilaPreguntaKahoot>();
    private bool inicializado;
    private bool guardando;
    private int ultimoFrameGuardado = -1;
    private int ultimoFrameAnadir = -1;
    private string rutaEdicion;

    public event Action<KahootDatos> KahootGuardado;

    private void Awake()
    {
        if (inputTitulo != null) inputTitulo.characterLimit = RepositorioCreacionesKahoot.MaxCaracteresTitulo;
        if (botonGenerarCantidad != null) botonGenerarCantidad.onClick.AddListener(AjustarCantidadInicial);
        if (botonAnadirFila != null) botonAnadirFila.onClick.AddListener(AnadirPregunta);
        if (botonGuardar != null) botonGuardar.onClick.AddListener(Guardar);
    }

    private void Start() { PrepararFilas(); }

    private bool PrepararFilas()
    {
        if (inicializado) return true;
        if (contenidoScrollView == null)
        {
            Debug.LogError("CreadorKahoots: falta Content de preguntas (NO de Mis creaciones).", this);
            return false;
        }
        FilaPreguntaKahoot primera = null;
        var sobrantes = new List<GameObject>();
        foreach (Transform hijo in contenidoScrollView)
        {
            FilaPreguntaKahoot fila = hijo.GetComponent<FilaPreguntaKahoot>();
            if (fila == null) continue;
            if (primera == null) primera = fila;
            else sobrantes.Add(fila.gameObject);
        }
        foreach (GameObject obj in sobrantes) { obj.SetActive(false); Destroy(obj); }
        filas.Clear();
        if (primera != null)
        {
            primera.gameObject.SetActive(true);
            filas.Add(primera);
            primera.Inicializar(1, EliminarPregunta);
        }
        else if (CrearFila() == null)
        {
            Debug.LogError("CreadorKahoots: falta BaseC en Content y/o prefabFila.", this);
            return false;
        }
        if (inputCantidadInicial != null) inputCantidadInicial.SetTextWithoutNotify("1");
        inicializado = true;
        ActualizarNumeracion();
        ActualizarLayout();
        Debug.Log("CreadorKahoots listo: 1 pregunta inicial.");
        return true;
    }

    private FilaPreguntaKahoot CrearFila()
    {
        if (contenidoScrollView == null) return null;
        FilaPreguntaKahoot origen = prefabFila != null ? prefabFila : (filas.Count > 0 ? filas[0] : null);
        if (origen == null) return null;
        FilaPreguntaKahoot fila = Instantiate(origen, contenidoScrollView, false);
        fila.name = "Pregunta_" + (filas.Count + 1);
        fila.gameObject.SetActive(true);
        fila.LimpiarFormulario();
        fila.transform.SetAsLastSibling();
        filas.Add(fila);
        fila.Inicializar(filas.Count, EliminarPregunta);
        return fila;
    }

    public void AnadirPregunta()
    {
        if (!PrepararFilas() || ultimoFrameAnadir == Time.frameCount) return;
        ultimoFrameAnadir = Time.frameCount;
        if (CrearFila() == null)
        {
            Informar("No se pudo crear una pregunta. Revisa BaseC.");
            return;
        }
        ActualizarNumeracion();
        ActualizarLayout();
        Debug.Log("Pregunta añadida: " + filas.Count);
    }

    public void AjustarCantidadInicial()
    {
        if (!PrepararFilas()) return;
        if (inputCantidadInicial == null || !int.TryParse(inputCantidadInicial.text, out int cantidad)
            || cantidad < 1 || cantidad > 100)
        {
            Informar("Introduce un numero de preguntas entre 1 y 100.");
            return;
        }
        if (cantidad < filas.Count)
        {
            Informar("Elimina las preguntas sobrantes con su boton.");
            return;
        }
        while (filas.Count < cantidad) if (CrearFila() == null) break;
        ActualizarNumeracion();
        ActualizarLayout();
    }

    private void EliminarPregunta(FilaPreguntaKahoot fila)
    {
        if (fila == null || !filas.Contains(fila)) return;
        if (filas.Count <= 1) { fila.LimpiarFormulario(); return; }
        filas.Remove(fila);
        fila.gameObject.SetActive(false);
        Destroy(fila.gameObject);
        ActualizarNumeracion();
        ActualizarLayout();
    }

    private void ActualizarNumeracion()
    {
        for (int i = 0; i < filas.Count; i++)
            if (filas[i] != null) filas[i].ActualizarNumero(i + 1);
    }

    private void LimpiarPreguntas()
    {
        for (int i = filas.Count - 1; i >= 1; i--)
        {
            if (filas[i] != null) { filas[i].gameObject.SetActive(false); Destroy(filas[i].gameObject); }
            filas.RemoveAt(i);
        }
        if (filas.Count == 0) CrearFila();
        if (filas.Count > 0 && filas[0] != null) filas[0].LimpiarFormulario();
        ActualizarNumeracion();
        ActualizarLayout();
    }

    public void NuevaCreacion()
    {
        if (!PrepararFilas()) return;
        rutaEdicion = null;
        LimpiarPreguntas();
        if (inputTitulo != null) inputTitulo.SetTextWithoutNotify("");
        if (inputCantidadInicial != null) inputCantidadInicial.SetTextWithoutNotify("1");
        Informar("");
        Debug.Log("Editor abierto: nuevo Kahoot con Pregunta 1.");
    }

    public void ReiniciarFormulario() { NuevaCreacion(); }

    public bool EditarCreacion(string ruta)
    {
        if (!PrepararFilas()) return false;
        if (!RepositorioCreacionesKahoot.IntentarAbrir(ruta, out KahootDatos datos))
        {
            Informar("No se pudo abrir este Kahoot.");
            return false;
        }
        LimpiarPreguntas();
        rutaEdicion = ruta;
        if (inputTitulo != null) inputTitulo.SetTextWithoutNotify(datos.titulo);
        for (int i = 0; i < datos.preguntas.Count; i++)
        {
            FilaPreguntaKahoot fila = i == 0 ? filas[0] : CrearFila();
            if (fila == null) return false;
            fila.CargarPregunta(datos.preguntas[i]);
        }
        if (inputCantidadInicial != null)
            inputCantidadInicial.SetTextWithoutNotify(datos.preguntas.Count.ToString());
        ActualizarNumeracion();
        ActualizarLayout();
        Debug.Log("Editando Kahoot: " + datos.titulo);
        return true;
    }

    public void Guardar()
    {
        Debug.Log("CreadorKahoots: pulsado Guardar");
        if (!PrepararFilas() || guardando || ultimoFrameGuardado == Time.frameCount) return;
        if (inputTitulo == null)
        {
            Informar("Falta Input Titulo en CreadorKahoots.");
            return;
        }
        string titulo = inputTitulo.text.Trim();
        if (titulo.Length < 1 || titulo.Length > RepositorioCreacionesKahoot.MaxCaracteresTitulo)
        {
            Informar("El titulo debe tener entre 1 y 50 caracteres.");
            return;
        }
        if (filas.Count < 1) { Informar("Falta la primera pregunta."); return; }
        guardando = true;
        try
        {
            KahootDatos datos = new KahootDatos { titulo = titulo };
            for (int i = 0; i < filas.Count; i++)
            {
                if (filas[i] == null || !filas[i].IntentarLeer(out PreguntaKahoot pregunta, out string error))
                {
                    Informar("Pregunta " + (i + 1) + " incorrecta. Revisa respuestas, tiempo y correcta.");
                    return;
                }
                datos.preguntas.Add(pregunta);
            }
            if (!AlmacenKahoots.Validar(datos, out string motivo))
            {
                Informar(motivo);
                return;
            }
            if (RepositorioCreacionesKahoot.TituloEnUso(titulo, rutaEdicion))
            {
                Informar("Ya existe una creacion con ese titulo. Usa Editar.");
                return;
            }
            foreach (KahootDatos baseQuiz in AlmacenKahoots.LeerKahootsPredeterminados())
            {
                if (string.Equals(baseQuiz.titulo.Trim(), titulo, StringComparison.OrdinalIgnoreCase))
                {
                    Informar("Ese titulo pertenece a un cuestionario predeterminado.");
                    return;
                }
            }
            string ruta;
            if (string.IsNullOrEmpty(rutaEdicion)) ruta = AlmacenKahoots.Guardar(datos);
            else
            {
                RepositorioCreacionesKahoot.Actualizar(rutaEdicion, datos);
                ruta = rutaEdicion;
            }
            ultimoFrameGuardado = Time.frameCount;
            Debug.Log("KAHOOT GUARDADO EN DISCO: " + ruta);

            SelectorKahoots destino = selector;
            if (destino == null)
                destino = FindFirstObjectByType<SelectorKahoots>(FindObjectsInactive.Include);
            if (destino != null) destino.RefrescarListado();
            else Debug.LogWarning("Guardado correcto, pero no hay SelectorKahoots asignado.");

            if (reiniciarTrasGuardar) NuevaCreacion();
            else rutaEdicion = ruta;
            KahootGuardado?.Invoke(datos);
            Informar("Guardado: " + titulo);
        }
        catch (Exception ex)
        {
            AlmacenKahoots.RegistrarError("Error guardando Kahoot", ex);
            Debug.LogError("ERROR AL GUARDAR: " + ex, this);
            Informar("Error al guardar: " + ex.Message);
        }
        finally { guardando = false; }
    }

    private void ActualizarLayout()
    {
        if (contenidoScrollView is RectTransform rect)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
    }

    private void Informar(string mensaje)
    {
        if (textoEstado != null) textoEstado.text = mensaje;
        if (!string.IsNullOrEmpty(mensaje)) Debug.Log("CreadorKahoots: " + mensaje);
    }

    private void OnDestroy()
    {
        if (botonGenerarCantidad != null)
            botonGenerarCantidad.onClick.RemoveListener(AjustarCantidadInicial);
        if (botonAnadirFila != null)
            botonAnadirFila.onClick.RemoveListener(AnadirPregunta);
        if (botonGuardar != null)
            botonGuardar.onClick.RemoveListener(Guardar);
    }
}
