
using UnityEngine;

public class VisualRueda : MonoBehaviour
{
    [Header("Escalado")]
    [SerializeField] private float escalaNormal = 0.8f;
    [SerializeField] private float escalaSeleccionada = 1.4f;

    [Header("Movimiento ambiental")]
    [SerializeField] private bool efectoActivo = true;
    [SerializeField] private float intensidadPulso = 0.025f;
    [SerializeField] private float velocidadPulso = 1.5f;

    private OpcionRueda[] opciones;
    private RotacionRueda rotacion;

    private RectTransform[] botones;
    private float[] angulosBase;

    private float tiempo;

    public void Inicializar(
        OpcionRueda[] opciones,
        RotacionRueda rotacion)
    {
        this.opciones = opciones;
        this.rotacion = rotacion;

        if (opciones == null || rotacion == null)
        {
            Debug.LogError("VisualRueda: Referencias incorrectas.");
            return;
        }

        angulosBase = rotacion.AngulosBase;
        botones = new RectTransform[opciones.Length];

        for (int i = 0; i < opciones.Length; i++)
        {
            if (opciones[i] == null || opciones[i].boton == null)
                continue;

            botones[i] = opciones[i].boton.GetComponent<RectTransform>();
        }
    }

    public void ActualizarVisual(float deltaTime)
    {
        if (botones == null || rotacion == null)
            return;

        tiempo += deltaTime;

        float anguloActual = rotacion.AnguloActual;

        for (int i = 0; i < botones.Length; i++)
        {
            if (botones[i] == null ||
                i >= angulosBase.Length ||
                float.IsNaN(angulosBase[i]))
                continue;

            RectTransform boton = botones[i];

            // Mantener el botón siempre recto
            boton.localRotation = Quaternion.Euler(
                0f,
                0f,
                -anguloActual
            );

            // Calcular cercanía a la posición superior
            float distancia = Mathf.Abs(
                Mathf.DeltaAngle(
                    angulosBase[i] + anguloActual,
                    90f
                )
            );

            float cercania = 1f - Mathf.Clamp01(distancia / 90f);

            // Escalado según selección
            float escala = Mathf.Lerp(
                escalaNormal,
                escalaSeleccionada,
                cercania
            );

            // Pequeño efecto ambiental
            if (efectoActivo)
            {
                float pulso = 1f + Mathf.Sin(
                    tiempo * velocidadPulso + i * 0.5f
                ) * intensidadPulso;

                escala *= pulso;
            }

            boton.localScale = Vector3.one * escala;
        }
    }
}
