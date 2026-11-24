
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ControlRueda : MonoBehaviour
{
    public enum ModoGiro
    {
        SoloBotones,
        SoloRaton,
        Ambos
    }

    [Header("Modo de control")]
    [SerializeField] private ModoGiro modo = ModoGiro.Ambos;

    [Header("Flechas")]
    [SerializeField] private Button flechaIzquierda;
    [SerializeField] private Button flechaDerecha;

    private OpcionRueda[] opciones;
    private RotacionRueda rotacion;
    private DetectorRaton detector;
    private GestorPaneles gestorPaneles;
    private TransicionMenu transicion;

    private UnityAction[] accionesBotones;

    public void Inicializar(
        OpcionRueda[] opciones,
        RotacionRueda rotacion,
        DetectorRaton detector,
        GestorPaneles gestorPaneles,
        TransicionMenu transicion)
    {
        if (opciones == null ||
            rotacion == null ||
            detector == null ||
            gestorPaneles == null ||
            transicion == null)
        {
            Debug.LogError("ControlRueda: Faltan referencias.");
            return;
        }

        this.opciones = opciones;
        this.rotacion = rotacion;
        this.detector = detector;
        this.gestorPaneles = gestorPaneles;
        this.transicion = transicion;

        ConfigurarBotones();
        ConfigurarFlechas();
        ConfigurarDetector();
    }

    private void ConfigurarBotones()
    {
        accionesBotones = new UnityAction[opciones.Length];

        for (int i = 0; i < opciones.Length; i++)
        {
            if (opciones[i] == null || opciones[i].boton == null)
                continue;

            int indice = i;

            accionesBotones[i] = () => ProcesarClick(indice);

            opciones[i].boton.onClick.AddListener(
                accionesBotones[i]
            );
        }
    }

    private void ConfigurarFlechas()
    {
        if (modo == ModoGiro.SoloRaton)
            return;

        if (flechaIzquierda != null)
            flechaIzquierda.onClick.AddListener(GirarIzquierda);

        if (flechaDerecha != null)
            flechaDerecha.onClick.AddListener(GirarDerecha);
    }

    private void ConfigurarDetector()
    {
        detector.ActivarDeteccion(
            modo != ModoGiro.SoloBotones
        );

        detector.Inicializar(
            opciones,
            rotacion,
            LlevarOpcionArriba
        );
    }

    private bool PuedeInteractuar()
    {
        return !MenuJugador.ModalAbierto && gestorPaneles != null && gestorPaneles.PanelActual != null &&
               transform.IsChildOf(gestorPaneles.PanelActual.transform) &&
               rotacion != null &&
               !rotacion.EstaGirando &&
               transicion != null &&
               !transicion.EnTransicion;
    }

    private void ProcesarClick(int indice)
    {
        if (!PuedeInteractuar())
            return;

        if (indice < 0 || indice >= opciones.Length)
            return;

        EjecutarAccion(opciones[indice]);
    }

    private void LlevarOpcionArriba(int indice)
    {
        if (!PuedeInteractuar())
            return;

        rotacion.LlevarArriba(indice);
    }

    private void GirarIzquierda()
    {
        if (!PuedeInteractuar())
            return;

        rotacion.GirarSiguiente(-1);
    }

    private void GirarDerecha()
    {
        if (!PuedeInteractuar())
            return;

        rotacion.GirarSiguiente(1);
    }

    private void EjecutarAccion(OpcionRueda opcion)
    {
        if (opcion == null)
            return;

        switch (opcion.tipoAccion)
        {
            case TipoAccion.AbrirPanel:

                if (opcion.panelDestino == null)
                {
                    Debug.LogError(
                        "ControlRueda: No se ha asignado el panel de destino."
                    );
                    return;
                }

                RectTransform botonSeleccionado =
                    opcion.boton.GetComponent<RectTransform>();

                transicion.AbrirPanel(
                    opcion.panelDestino,
                    botonSeleccionado
                );

                break;

            case TipoAccion.SalirJuego:

                transicion.SalirDelJuego();

                break;
        }
    }

    public void GirarDesdeMandoIzquierda()
    {
        GirarIzquierda();
    }

    public void GirarDesdeMandoDerecha()
    {
        GirarDerecha();
    }

    public void ConfirmarDesdeMando()
    {
        if (!PuedeInteractuar())
            return;

        int indice = rotacion.OpcionArriba();

        if (indice < 0 || indice >= opciones.Length)
            return;

        EjecutarAccion(opciones[indice]);
    }

    private void OnDestroy()
    {
        if (accionesBotones != null && opciones != null)
        {
            for (int i = 0; i < opciones.Length; i++)
            {
                if (opciones[i] != null &&
                    opciones[i].boton != null &&
                    accionesBotones[i] != null)
                {
                    opciones[i].boton.onClick.RemoveListener(
                        accionesBotones[i]
                    );
                }
            }
        }

        if (flechaIzquierda != null)
            flechaIzquierda.onClick.RemoveListener(GirarIzquierda);

        if (flechaDerecha != null)
            flechaDerecha.onClick.RemoveListener(GirarDerecha);
    }
}
