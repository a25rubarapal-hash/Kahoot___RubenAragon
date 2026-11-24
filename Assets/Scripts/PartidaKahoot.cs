
using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PartidaKahoot : MonoBehaviour
{
    private enum Estado
    {
        Inactivo,
        Aviso,
        Pregunta,
        Resultado,
        Final
    }

    [Header("Interfaces de preguntas")]
    [SerializeField] private InterfazPregunta interfazABCD;
    [SerializeField] private InterfazPregunta interfazTF;

    [Header("Interfaces compartidas")]
    [SerializeField] private InterfazHUD interfazHUD;
    [SerializeField] private InterfazResultados interfazResultados;
    [SerializeField] private GestorAviso gestorAviso;

    [Header("Configuracion de puntos")]
    [SerializeField, Min(1)] private int puntosMaximos = 1000;
    [SerializeField, Min(0)] private int penalizacionFallo = 75;
    [SerializeField, Min(0)] private int penalizacionTiempo = 250;

    [Header("Tiempo para mostrar la respuesta")]
    [SerializeField, Range(4f, 5f)]
    private float segundosMostrarSolucion = 4.5f;

    private readonly GestorPreguntas gestorPreguntas =
        new GestorPreguntas();

    private readonly GestorRespuestas gestorRespuestas =
        new GestorRespuestas();

    private readonly GestorTiempo gestorTiempo =
        new GestorTiempo();

    private GestorPuntuacion gestorPuntuacion;

    private PreguntaPreparada preguntaActual;
    private KahootDatos kahootOriginal;
    private InterfazPregunta interfazActiva;
    private CanvasGroup grupoPrincipal;

    private Estado estado = Estado.Inactivo;
    private int indicePregunta;
    private float segundosRespondidos;

    private Coroutine rutinaResultado;
    private bool resultadoListo;

    public event Action<KahootDatos, int, int> PartidaFinalizada;

    private void Awake()
    {
        grupoPrincipal = GetComponent<CanvasGroup>();

        if (interfazResultados != null)
            interfazResultados.Inicializar(Siguiente);

        OcultarInterfaces();
    }

    public void ComenzarDesdeArchivo(string ruta)
    {
        if (AlmacenKahoots.IntentarCargar(
            ruta,
            out KahootDatos datos))
        {
            Comenzar(datos);
        }
    }

    public void Comenzar(KahootDatos datos)
    {
        if (!AlmacenKahoots.Validar(datos, out string error))
        {
            Debug.LogError("PartidaKahoot: " + error, this);
            return;
        }

        if (!ReferenciasCorrectas())
        {
            Debug.LogError(
                "PartidaKahoot: faltan referencias.",
                this
            );
            return;
        }

        CancelarPartida();

        kahootOriginal = datos;
        indicePregunta = 0;
        segundosRespondidos = 0;

        gestorPreguntas.Cargar(datos);

        gestorPuntuacion = new GestorPuntuacion(
            new ReglaPuntuacionProporcional(
                puntosMaximos,
                penalizacionFallo,
                penalizacionTiempo
            )
        );

        PrepararSiguientePregunta();
    }

    private bool ReferenciasCorrectas()
    {
        return interfazABCD != null &&
               interfazTF != null &&
               interfazHUD != null &&
               interfazResultados != null &&
               gestorAviso != null;
    }

    private void PrepararSiguientePregunta()
    {
        if (indicePregunta >= gestorPreguntas.Cantidad)
        {
            Finalizar();
            return;
        }

        PreguntaKahoot datos =
            gestorPreguntas.Obtener(indicePregunta);

        preguntaActual = gestorRespuestas.Preparar(datos);

        if (preguntaActual == null ||
            preguntaActual.CorrectaVisible < 0)
        {
            Debug.LogError(
                "No se pudo preparar la pregunta.",
                this
            );

            CancelarPartida();
            return;
        }

        interfazActiva = preguntaActual.EsTF
            ? interfazTF
            : interfazABCD;

        OcultarInterfaces();

        bool preparada = interfazActiva.Configurar(
            preguntaActual,
            RecibirRespuesta
        );

        if (!preparada)
        {
            CancelarPartida();
            return;
        }

        interfazHUD.Preparar(
            preguntaActual.Original.enunciado,
            indicePregunta + 1,
            gestorPuntuacion.Puntos,
            preguntaActual.Original.duracion
        );

        resultadoListo = false;
        estado = Estado.Aviso;

        string tipo = preguntaActual.EsTF
            ? "True / False"
            : "Multiple Choice";

        gestorAviso.IniciarAviso(
            tipo,
            MostrarPregunta,
            PantallaVisible
        );
    }

    private void MostrarPregunta()
    {
        if (estado != Estado.Aviso ||
            preguntaActual == null)
        {
            return;
        }

        gestorTiempo.Iniciar(
            preguntaActual.Original.duracion
        );

        interfazHUD.ActualizarTiempo(
            gestorTiempo.Restante
        );

        interfazHUD.Mostrar(true);
        interfazActiva.Mostrar(true);

        estado = Estado.Pregunta;
    }

    private void Update()
    {
        if (estado != Estado.Pregunta ||
            !PantallaVisible())
        {
            return;
        }

        bool agotado = gestorTiempo.Actualizar(
            Time.deltaTime
        );

        interfazHUD.ActualizarTiempo(
            gestorTiempo.Restante
        );

        if (agotado)
            RecibirRespuesta(-1);
    }

    private bool PantallaVisible()
    {
        return isActiveAndEnabled &&
               gameObject.activeInHierarchy &&
               grupoPrincipal != null &&
               grupoPrincipal.alpha >= 0.99f &&
               grupoPrincipal.interactable;
    }

    private void RecibirRespuesta(int seleccion)
    {
        if (estado != Estado.Pregunta ||
            !PantallaVisible())
        {
            return;
        }

        estado = Estado.Resultado;
        resultadoListo = false;

        gestorTiempo.Detener();

        segundosRespondidos += gestorTiempo.Duracion - gestorTiempo.Restante;
        bool tiempoAgotado = seleccion < 0;

        bool acertada = gestorRespuestas.EsCorrecta(
            preguntaActual,
            seleccion
        );

        int diferencia = gestorPuntuacion.Registrar(
            acertada,
            tiempoAgotado,
            gestorTiempo.Restante,
            gestorTiempo.Duracion
        );

        // Mostrar colores y bloquear los botones.
        interfazActiva.MostrarSolucion(
            seleccion,
            preguntaActual.CorrectaVisible
        );

        interfazHUD.ActualizarPuntos(
            gestorPuntuacion.Puntos
        );

        string correcta = preguntaActual.ObtenerRespuesta(
            preguntaActual.CorrectaVisible
        );

        string elegida = tiempoAgotado
            ? "Sin respuesta"
            : preguntaActual.ObtenerRespuesta(seleccion);

        // No mostrar Bien/Mal inmediatamente.
        interfazResultados.Ocultar();

        if (rutinaResultado != null)
            StopCoroutine(rutinaResultado);

        rutinaResultado = StartCoroutine(
            EsperarYMostrarResultado(
                acertada,
                tiempoAgotado,
                diferencia,
                correcta,
                elegida
            )
        );
    }

    private IEnumerator EsperarYMostrarResultado(
        bool acertada,
        bool tiempoAgotado,
        int diferencia,
        string correcta,
        string elegida)
    {
        float transcurrido = 0f;

        // Mantener visibles los colores antes de abrir Bien/Mal.
        while (transcurrido < segundosMostrarSolucion)
        {
            if (estado != Estado.Resultado)
            {
                rutinaResultado = null;
                yield break;
            }

            if (PantallaVisible())
                transcurrido += Time.unscaledDeltaTime;

            yield return null;
        }

        if (estado != Estado.Resultado)
        {
            rutinaResultado = null;
            yield break;
        }

        if (acertada)
        {
            interfazResultados.MostrarAcierto(
                diferencia,
                correcta
            );
        }
        else
        {
            interfazResultados.MostrarFallo(
                diferencia,
                correcta,
                elegida,
                tiempoAgotado
            );
        }

        resultadoListo = true;
        rutinaResultado = null;
    }

    public void Siguiente()
    {
        if (estado != Estado.Resultado ||
            !resultadoListo ||
            !PantallaVisible())
        {
            return;
        }

        resultadoListo = false;

        indicePregunta++;
        PrepararSiguientePregunta();
    }

    private void Finalizar()
    {
        estado = Estado.Final;

        gestorTiempo.Detener();
        gestorAviso.Cancelar();

        interfazABCD.Mostrar(false);
        interfazTF.Mostrar(false);
        interfazHUD.Mostrar(false);

        interfazResultados.MostrarFinal(
            gestorPuntuacion.Aciertos,
            gestorPreguntas.Cantidad,
            gestorPuntuacion.Puntos,
            ClasificacionKahoot.Guardar(kahootOriginal, gestorPuntuacion.Puntos, segundosRespondidos)
        );

        PartidaFinalizada?.Invoke(
            kahootOriginal,
            gestorPuntuacion.Puntos,
            gestorPuntuacion.Aciertos
        );
    }

    private void OcultarInterfaces()
    {
        if (interfazABCD != null)
            interfazABCD.Mostrar(false);

        if (interfazTF != null)
            interfazTF.Mostrar(false);

        if (interfazHUD != null)
            interfazHUD.Mostrar(false);

        if (interfazResultados != null)
            interfazResultados.Ocultar();

        if (gestorAviso != null)
            gestorAviso.Cancelar();
    }

    public void CancelarPartida()
    {
        estado = Estado.Inactivo;
        resultadoListo = false;

        if (rutinaResultado != null)
        {
            StopCoroutine(rutinaResultado);
            rutinaResultado = null;
        }

        gestorTiempo.Reiniciar();
        gestorPreguntas.Limpiar();

        preguntaActual = null;
        interfazActiva = null;
        kahootOriginal = null;

        OcultarInterfaces();
    }

    private void OnDisable()
    {
        CancelarPartida();
    }
}
