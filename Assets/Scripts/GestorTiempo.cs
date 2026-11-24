
using UnityEngine;

public class GestorTiempo
{
    public float Duracion { get; private set; }
    public float Restante { get; private set; }
    public bool Corriendo { get; private set; }

    public void Iniciar(float duracion)
    {
        Duracion = Mathf.Max(0.01f, duracion);
        Restante = Duracion;
        Corriendo = true;
    }

    public bool Actualizar(float deltaTime)
    {
        if (!Corriendo)
            return false;

        Restante = Mathf.Max(
            0f,
            Restante - Mathf.Max(0f, deltaTime)
        );

        if (Restante > 0f)
            return false;

        Corriendo = false;
        return true;
    }

    public void Detener()
    {
        Corriendo = false;
    }

    public void Reiniciar()
    {
        Duracion = 0f;
        Restante = 0f;
        Corriendo = false;
    }
}
