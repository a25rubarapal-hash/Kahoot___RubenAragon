
using UnityEngine;

public class RotacionRueda : MonoBehaviour
{
    private RectTransform rueda;
    private OpcionRueda[] opciones;

    private float[] angulosBase;

    private float anguloActual;
    private float anguloObjetivo;
    private float velocidad;

    public float AnguloActual => anguloActual;

    public bool EstaGirando =>
        Mathf.Abs(anguloObjetivo - anguloActual) > 0.5f;

    public float[] AngulosBase => angulosBase;


    // INICIALIZACIÓN

    public bool Inicializar(
        RectTransform rueda,
        OpcionRueda[] opciones,
        float velocidad)
    {
        if (rueda == null || opciones == null || opciones.Length == 0)
        {
            Debug.LogError("RotacionRueda: Faltan referencias.");
            return false;
        }

        this.rueda = rueda;
        this.opciones = opciones;
        this.velocidad = Mathf.Max(0f, velocidad);

        angulosBase = new float[opciones.Length];

        for (int i = 0; i < opciones.Length; i++)
        {
            if (opciones[i] == null || opciones[i].boton == null)
            {
                angulosBase[i] = float.NaN;
                continue;
            }

            Transform boton = opciones[i].boton.transform;
            Vector3 posicion = boton.parent == rueda
                ? boton.localPosition
                : rueda.InverseTransformPoint(boton.position);

            if (posicion.sqrMagnitude < 0.001f)
            {
                angulosBase[i] = float.NaN;
                continue;
            }

            angulosBase[i] = Mathf.Atan2(
                posicion.y,
                posicion.x
            ) * Mathf.Rad2Deg;
        }

        anguloActual = 0f;
        anguloObjetivo = 0f;

        return true;
    }


    // ACTUALIZACIÓN DE LA ROTACIÓN

    public void Actualizar(float deltaTime)
    {
        if (rueda == null)
            return;

        anguloActual = Mathf.MoveTowards(
            anguloActual,
            anguloObjetivo,
            velocidad * deltaTime
        );

        rueda.localRotation = Quaternion.Euler(
            0f,
            0f,
            anguloActual
        );
    }


    // DETECTAR OPCIÓN SUPERIOR

    public int OpcionArriba()
    {
        int mejor = -1;
        float menor = float.MaxValue;

        for (int i = 0; i < angulosBase.Length; i++)
        {
            if (float.IsNaN(angulosBase[i]))
                continue;

            float distancia = Mathf.Abs(
                Mathf.DeltaAngle(
                    angulosBase[i] + anguloObjetivo,
                    90f
                )
            );

            if (distancia < menor)
            {
                menor = distancia;
                mejor = i;
            }
        }

        return mejor;
    }


    // GIRAR HACIA UNA OPCIÓN

    public void LlevarArriba(int indice)
    {
        if (EstaGirando)
            return;

        if (indice < 0 || indice >= angulosBase.Length)
            return;

        if (float.IsNaN(angulosBase[indice]))
            return;

        float delta = Mathf.DeltaAngle(
            angulosBase[indice] + anguloObjetivo,
            90f
        );

        anguloObjetivo += delta;
    }


    // GIRAR A IZQUIERDA O DERECHA

    public void GirarSiguiente(int direccion)
    {
        if (EstaGirando)
            return;

        if (direccion != 1 && direccion != -1)
            return;

        float mejorDelta = 0f;
        float menorDistancia = float.MaxValue;

        for (int i = 0; i < angulosBase.Length; i++)
        {
            if (float.IsNaN(angulosBase[i]))
                continue;

            float angulo =
                angulosBase[i] + anguloObjetivo;

            float delta;

            if (direccion > 0)
            {
                delta = Mathf.Repeat(90f - angulo, 360f);
            }
            else
            {
                delta = -Mathf.Repeat(angulo - 90f, 360f);
            }

            if (Mathf.Abs(delta) < 0.5f)
                continue;

            if (Mathf.Abs(delta) < menorDistancia)
            {
                menorDistancia = Mathf.Abs(delta);
                mejorDelta = delta;
            }
        }

        if (menorDistancia != float.MaxValue)
            anguloObjetivo += mejorDelta;
    }
}
