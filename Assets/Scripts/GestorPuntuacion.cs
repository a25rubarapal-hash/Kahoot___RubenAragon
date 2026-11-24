
using UnityEngine;

public interface IReglaPuntuacion
{
    int Calcular(
        bool acierto,
        bool tiempoAgotado,
        float tiempoRestante,
        float tiempoTotal
    );
}

public class ReglaPuntuacionProporcional : IReglaPuntuacion
{
    private readonly int puntosMaximos;
    private readonly int porcentajePenalizacionFallo;
    private readonly int penalizacionTiempo;

    public ReglaPuntuacionProporcional(
        int maximos,
        int fallo,
        int tiempo)
    {
        puntosMaximos = Mathf.Max(1, maximos);

        porcentajePenalizacionFallo =
            Mathf.Clamp(fallo, 0, 100);

        penalizacionTiempo = Mathf.Max(0, tiempo);
    }

    public int Calcular(
        bool acierto,
        bool tiempoAgotado,
        float tiempoRestante,
        float tiempoTotal)
    {
        if (tiempoAgotado)
            return -penalizacionTiempo;

        float proporcion = Mathf.Clamp01(
            tiempoRestante / Mathf.Max(0.01f, tiempoTotal)
        );

        float factor = 0.5f + 0.5f * proporcion;

        int puntosPosibles = Mathf.Clamp(
            Mathf.RoundToInt(puntosMaximos * factor),
            1,
            puntosMaximos
        );

        if (acierto)
            return puntosPosibles;

        int penalizacion = Mathf.RoundToInt(
            puntosPosibles *
            (porcentajePenalizacionFallo / 100f)
        );

        return -penalizacion;
    }
}

public class GestorPuntuacion
{
    private readonly IReglaPuntuacion regla;

    public int Puntos { get; private set; }
    public int Aciertos { get; private set; }

    public GestorPuntuacion(IReglaPuntuacion regla)
    {
        this.regla = regla;
        Reiniciar();
    }

    public void Reiniciar()
    {
        Puntos = 0;
        Aciertos = 0;
    }

    public int Registrar(
        bool acierto,
        bool tiempoAgotado,
        float restante,
        float total)
    {
        int diferencia = regla.Calcular(
            acierto,
            tiempoAgotado,
            restante,
            total
        );

        Puntos += diferencia;

        if (acierto)
            Aciertos++;

        return diferencia;
    }
}
