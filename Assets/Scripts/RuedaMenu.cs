
using UnityEngine;

public class RuedaMenu : MonoBehaviour
{
    [Header("Rueda")]
    [SerializeField] private RectTransform rueda;
    [SerializeField] private OpcionRueda[] opciones;

    [Header("Pantalla principal")]
    [SerializeField] private CanvasGroup menuPrincipal;

    [Header("Configuración de rotación")]
    [SerializeField] private float velocidadRotacion = 480f;

    private RotacionRueda rotacion;
    private VisualRueda visual;
    private DetectorRaton detector;
    private ControlRueda control;
    private ControlMando mando;
    private GestorPaneles gestorPaneles;
    private TransicionMenu transicion;

    private bool inicializado;

    private void Start()
    {
        InicializarComponentes();
    }

    private void InicializarComponentes()
    {
        // Comprobar referencias principales.
        if (rueda == null ||
            opciones == null ||
            opciones.Length == 0 ||
            menuPrincipal == null)
        {
            Debug.LogError(
                "RuedaMenu: Asigna la rueda, las opciones y el menú principal."
            );

            enabled = false;
            return;
        }

        // Obtener componentes del mismo GameObject.
        rotacion = GetComponent<RotacionRueda>();
        visual = GetComponent<VisualRueda>();
        detector = GetComponent<DetectorRaton>();
        control = GetComponent<ControlRueda>();
        mando = GetComponent<ControlMando>();
        gestorPaneles = GetComponent<GestorPaneles>();
        transicion = GetComponent<TransicionMenu>();

        // Comprobar que todos los componentes necesarios existen.

        if (rotacion == null)
            Debug.LogError("Falta RotacionRueda.");

        if (visual == null)
            Debug.LogError("Falta VisualRueda.");

        if (detector == null)
            Debug.LogError("Falta DetectorRaton.");

        if (control == null)
            Debug.LogError("Falta ControlRueda.");

        if (mando == null)
            Debug.LogError("Falta ControlMando.");

        if (gestorPaneles == null)
            Debug.LogError("Falta GestorPaneles.");

        if (transicion == null)
            Debug.LogError("Falta TransicionMenu.");

        if (rotacion == null ||
            visual == null ||
            detector == null ||
            control == null ||
            mando == null ||
            gestorPaneles == null ||
            transicion == null)
        {
            enabled = false;
            return;
        }

       

        // Inicializar rotación.
        if (!rotacion.Inicializar(
            rueda,
            opciones,
            velocidadRotacion))
        {
            enabled = false;
            return;
        }

        // Inicializar efectos visuales.
        visual.Inicializar(opciones, rotacion);

        // Inicializar gestor de paneles.
        gestorPaneles.Inicializar(menuPrincipal);

        // Inicializar transiciones.
        transicion.Inicializar(
            rueda,
            menuPrincipal,
            gestorPaneles
        );

        // Inicializar controles de ratón y botones.
        control.Inicializar(
            opciones,
            rotacion,
            detector,
            gestorPaneles,
            transicion
        );

        // Inicializar controles del mando.
        mando.Inicializar(
            control.GirarDesdeMandoIzquierda,
            control.GirarDesdeMandoDerecha,
            control.ConfirmarDesdeMando
        );

        inicializado = true;

        Debug.Log("RuedaMenu: Inicializado correctamente.");
    }

    private void Update()
    {
        if (!inicializado)
            return;

        rotacion.Actualizar(Time.unscaledDeltaTime);
        visual.ActualizarVisual(Time.unscaledDeltaTime);
    }
}
