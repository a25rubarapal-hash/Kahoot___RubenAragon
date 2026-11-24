using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class DetectorRaton : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private float radioDeteccion = 150f;
    [SerializeField] private float retardo = 1f;

    private Canvas canvas;
    private Vector2? ultimoGiro;
    private OpcionRueda[] opciones;
    private RotacionRueda rotacion;

    private Action<int> alDetectarOpcion;

    private float temporizador;
    private int candidata = -1;
    private bool deteccionActiva = true;

    public void Inicializar(
        OpcionRueda[] opciones,
        RotacionRueda rotacion,
        Action<int> alDetectarOpcion)
    {
        canvas = GetComponentInParent<Canvas>().rootCanvas;
        this.opciones = opciones;
        this.rotacion = rotacion;
        this.alDetectarOpcion = alDetectarOpcion;

        if (opciones == null || rotacion == null ||
            alDetectarOpcion == null)
        {
            Debug.LogError("DetectorRaton: Faltan referencias.");
            deteccionActiva = false;
        }
    }

    private void Update()
    {
        if (!deteccionActiva)
            return;

        if (Mouse.current == null || rotacion == null)
        {
            ReiniciarDeteccion();
            return;
        }

        // No detectar mientras la rueda está girando.
        if (rotacion.EstaGirando)
        {
            ReiniciarDeteccion();
            return;
        }

        Vector2 posicionRaton =
            Mouse.current.position.ReadValue();

        if (ultimoGiro.HasValue && (posicionRaton - ultimoGiro.Value).sqrMagnitude < 9f)
        {
            ReiniciarDeteccion();
            return;
        }
        ultimoGiro = null;

        int opcionArriba = rotacion.OpcionArriba();
        int opcionCercana = BuscarOpcionCercana(posicionRaton);

        // No hacer nada si no hay una opción cercana
        // o si ya está arriba.
        if (opcionCercana < 0 || opcionCercana == opcionArriba)
        {
            ReiniciarDeteccion();
            return;
        }

        // Si cambia la opción, reiniciar el temporizador.
        if (opcionCercana != candidata)
        {
            candidata = opcionCercana;
            temporizador = 0f;
        }

        temporizador += Time.unscaledDeltaTime;

        // Esperar a que transcurra el retardo.
        if (temporizador < retardo)
            return;

        int opcionSeleccionada = candidata;
        ultimoGiro = posicionRaton;

        ReiniciarDeteccion();

        // Avisar al controlador de la rueda.
        alDetectarOpcion?.Invoke(opcionSeleccionada);
    }

    private int BuscarOpcionCercana(Vector2 posicionRaton)
    {
        int cercana = -1;
        float menorDistancia = radioDeteccion * canvas.scaleFactor;
        Camera camara = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        for (int i = 0; i < opciones.Length; i++)
        {
            if (opciones[i] == null || opciones[i].boton == null)
                continue;

            RectTransform rect = (RectTransform)opciones[i].boton.transform;
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, posicionRaton, camara))
                return i;

            Vector2 centroBoton =
                RectTransformUtility.WorldToScreenPoint(
                    camara,
                    rect.TransformPoint(rect.rect.center)
                );

            float distancia =
                Vector2.Distance(posicionRaton, centroBoton);

            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                cercana = i;
            }
        }

        return cercana;
    }

    private void ReiniciarDeteccion()
    {
        temporizador = 0f;
        candidata = -1;
    }

    public void ActivarDeteccion(bool activa)
    {
        deteccionActiva = activa;
        ReiniciarDeteccion();
    }
}
