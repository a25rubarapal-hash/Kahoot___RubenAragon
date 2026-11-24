using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FilaPreguntaKahoot : MonoBehaviour
{
    [Header("Datos de la pregunta")]
    [SerializeField] private TMP_Text textoNumero;
    [SerializeField] private TMP_Dropdown tipoPregunta;
    [SerializeField] private TMP_InputField inputEnunciado;
    [SerializeField] private TMP_InputField inputDuracion;
    [SerializeField] private Button botonEliminar;

    [Header("Pregunta ABCD")]
    [SerializeField] private GameObject grupoABCD;
    [SerializeField] private TMP_InputField[] respuestasABCD = new TMP_InputField[4];
    [SerializeField] private Toggle[] correctasABCD = new Toggle[4];

    [Header("Pregunta True / False")]
    [SerializeField] private GameObject grupoTF;
    [SerializeField] private Toggle[] correctasTF = new Toggle[2];

    private Action<FilaPreguntaKahoot> alEliminar;

    private readonly Dictionary<Toggle, UnityAction<bool>> eventosToggle =
        new Dictionary<Toggle, UnityAction<bool>>();

    private bool eventosConectados;

    private void Awake()
    {
        BuscarTextoNumero();

        if (tipoPregunta != null)
        {
            tipoPregunta.ClearOptions();

            tipoPregunta.AddOptions(new List<string>
            {
                "A/B/C/D",
                "True/False"
            });

            tipoPregunta.SetValueWithoutNotify(0);
            tipoPregunta.RefreshShownValue();
        }

        LimpiarToggles(correctasABCD);
        LimpiarToggles(correctasTF);

        if (inputDuracion != null &&
            string.IsNullOrWhiteSpace(inputDuracion.text))
        {
            inputDuracion.SetTextWithoutNotify("20");
        }
    }

    private void OnEnable()
    {
        ConectarEventos();
        ActualizarGrupos();
    }

    private void OnDisable()
    {
        DesconectarEventos();
    }

    private void BuscarTextoNumero()
    {
        if (textoNumero != null)
            return;

        Transform objetoPregunta = transform.Find("Pregunta");

        if (objetoPregunta != null)
        {
            textoNumero = objetoPregunta.GetComponent<TMP_Text>();

            if (textoNumero == null)
                textoNumero = objetoPregunta.GetComponentInChildren<TMP_Text>(true);
        }
    }

    public void Inicializar(
        int numero,
        Action<FilaPreguntaKahoot> callbackEliminar)
    {
        alEliminar = callbackEliminar;

        BuscarTextoNumero();

        ConectarEventos();
        ActualizarNumero(numero);
        ActualizarGrupos();
    }

    public void ActualizarNumero(int numero)
    {
        BuscarTextoNumero();

        if (textoNumero == null)
        {
            Debug.LogError(
                "BaseC: no se ha encontrado el texto del numero.",
                this
            );

            return;
        }

        textoNumero.text = numero.ToString(CultureInfo.InvariantCulture);
        textoNumero.SetAllDirty();
    }

    private void ConectarEventos()
    {
        if (eventosConectados)
            return;

        if (tipoPregunta != null)
            tipoPregunta.onValueChanged.AddListener(CambiarTipo);

        if (botonEliminar != null)
            botonEliminar.onClick.AddListener(EliminarEstaFila);

        ConectarToggles(correctasABCD);
        ConectarToggles(correctasTF);

        eventosConectados = true;
    }

    private void ConectarToggles(Toggle[] toggles)
    {
        if (toggles == null)
            return;

        for (int i = 0; i < toggles.Length; i++)
        {
            Toggle toggle = toggles[i];

            if (toggle == null || eventosToggle.ContainsKey(toggle))
                continue;

            int indice = i;

            UnityAction<bool> accion = activo =>
                CambiarSeleccion(toggles, indice, activo);

            toggle.onValueChanged.AddListener(accion);

            eventosToggle.Add(toggle, accion);
        }
    }

    private void DesconectarEventos()
    {
        if (!eventosConectados)
            return;

        if (tipoPregunta != null)
            tipoPregunta.onValueChanged.RemoveListener(CambiarTipo);

        if (botonEliminar != null)
            botonEliminar.onClick.RemoveListener(EliminarEstaFila);

        foreach (var evento in eventosToggle)
        {
            if (evento.Key != null)
                evento.Key.onValueChanged.RemoveListener(evento.Value);
        }

        eventosToggle.Clear();
        eventosConectados = false;
    }

    private void CambiarTipo(int indice)
    {
        ActualizarGrupos();
    }

    private void ActualizarGrupos()
    {
        if (tipoPregunta == null)
            return;

        bool esABCD = tipoPregunta.value == 0;

        if (grupoABCD != null && grupoTF != null &&
            grupoABCD == grupoTF)
        {
            Debug.LogError(
                "BaseC: los grupos ABCD y TF no pueden ser el mismo objeto.",
                this
            );

            return;
        }

        if (grupoABCD != null)
            grupoABCD.SetActive(esABCD);

        if (grupoTF != null)
            grupoTF.SetActive(!esABCD);
    }

    private void CambiarSeleccion(
        Toggle[] toggles,
        int indice,
        bool activo)
    {
        if (toggles == null ||
            indice < 0 ||
            indice >= toggles.Length)
        {
            return;
        }

        if (activo)
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                if (i == indice || toggles[i] == null)
                    continue;

                toggles[i].SetIsOnWithoutNotify(false);
            }
        }
        else
        {
            bool hayOtroSeleccionado = false;

            for (int i = 0; i < toggles.Length; i++)
            {
                if (i != indice &&
                    toggles[i] != null &&
                    toggles[i].isOn)
                {
                    hayOtroSeleccionado = true;
                    break;
                }
            }

            if (!hayOtroSeleccionado && toggles[indice] != null)
                toggles[indice].SetIsOnWithoutNotify(true);
        }
    }

    private void LimpiarToggles(Toggle[] toggles)
    {
        if (toggles == null)
            return;

        foreach (Toggle toggle in toggles)
        {
            if (toggle != null)
                toggle.SetIsOnWithoutNotify(false);
        }
    }

    private int ObtenerRespuestaCorrecta(Toggle[] toggles)
    {
        if (toggles == null)
            return -1;

        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i] != null && toggles[i].isOn)
                return i;
        }

        return -1;
    }

    private bool ValidarToggles(Toggle[] toggles, int cantidad)
    {
        if (toggles == null || toggles.Length != cantidad)
            return false;

        for (int i = 0; i < cantidad; i++)
        {
            if (toggles[i] == null)
                return false;
        }

        return true;
    }

    public bool IntentarLeer(
        out PreguntaKahoot pregunta,
        out string error)
    {
        pregunta = null;
        error = null;

        if (tipoPregunta == null || inputEnunciado == null)
        {
            error = "Faltan las referencias de Modo o Enunciado.";
            return false;
        }

        string enunciado = inputEnunciado.text.Trim();

        if (string.IsNullOrWhiteSpace(enunciado))
        {
            error = "Debes escribir el enunciado.";
            return false;
        }

        float duracion = 20f;

        if (inputDuracion != null)
        {
            string textoTiempo = inputDuracion.text
                .Trim()
                .Replace(',', '.');

            if (!float.TryParse(
                textoTiempo,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out duracion) ||
                float.IsNaN(duracion) ||
                float.IsInfinity(duracion) ||
                duracion <= 0)
            {
                error = "La duracion debe ser mayor que cero.";
                return false;
            }
        }

        bool esABCD = tipoPregunta.value == 0;

        if (esABCD && !ValidarToggles(correctasABCD, 4))
        {
            error = "Configura los cuatro Toggles ABCD.";
            return false;
        }

        if (!esABCD && !ValidarToggles(correctasTF, 2))
        {
            error = "Configura los dos Toggles True/False.";
            return false;
        }

        int correcta = esABCD
            ? ObtenerRespuestaCorrecta(correctasABCD)
            : ObtenerRespuestaCorrecta(correctasTF);

        if (correcta < 0)
        {
            error = esABCD
                ? "Selecciona la respuesta correcta A, B, C o D."
                : "Selecciona True o False.";

            return false;
        }

        pregunta = new PreguntaKahoot
        {
            tipo = esABCD ? "ABCD" : "TF",
            enunciado = enunciado,
            duracion = duracion,
            respuestaCorrecta = correcta
        };

        if (esABCD)
        {
            if (respuestasABCD == null ||
                respuestasABCD.Length != 4)
            {
                error = "Configura las cuatro respuestas ABCD.";
                return false;
            }

            for (int i = 0; i < 4; i++)
            {
                if (respuestasABCD[i] == null ||
                    string.IsNullOrWhiteSpace(respuestasABCD[i].text))
                {
                    error = "Falta la respuesta " + "ABCD"[i];
                    return false;
                }

                pregunta.respuestas.Add(
                    respuestasABCD[i].text.Trim()
                );
            }
        }
        else
        {
            pregunta.respuestas.Add("True");
            pregunta.respuestas.Add("False");
        }

        return true;
    }


    // Metodos para reutilizar BaseC y editar cuestionarios existentes.
    public void LimpiarFormulario()
    {
        if (tipoPregunta != null)
        {
            tipoPregunta.SetValueWithoutNotify(0);
            tipoPregunta.RefreshShownValue();
        }
        if (inputEnunciado != null) inputEnunciado.SetTextWithoutNotify("");
        if (inputDuracion != null) inputDuracion.SetTextWithoutNotify("20");
        if (respuestasABCD != null)
        {
            foreach (TMP_InputField respuesta in respuestasABCD)
                if (respuesta != null) respuesta.SetTextWithoutNotify("");
        }
        LimpiarToggles(correctasABCD);
        LimpiarToggles(correctasTF);
        ActualizarGrupos();
    }

    public void CargarPregunta(PreguntaKahoot datos)
    {
        if (datos == null) return;
        LimpiarFormulario();
        bool esTF = datos.tipo == "TF";
        if (tipoPregunta != null)
        {
            tipoPregunta.SetValueWithoutNotify(esTF ? 1 : 0);
            tipoPregunta.RefreshShownValue();
        }
        if (inputEnunciado != null)
            inputEnunciado.SetTextWithoutNotify(datos.enunciado);
        if (inputDuracion != null)
            inputDuracion.SetTextWithoutNotify(datos.duracion.ToString(CultureInfo.InvariantCulture));
        if (!esTF && respuestasABCD != null && datos.respuestas != null)
        {
            for (int i = 0; i < respuestasABCD.Length && i < datos.respuestas.Count; i++)
                if (respuestasABCD[i] != null)
                    respuestasABCD[i].SetTextWithoutNotify(datos.respuestas[i]);
        }
        Toggle[] correctas = esTF ? correctasTF : correctasABCD;
        if (correctas != null && datos.respuestaCorrecta >= 0 &&
            datos.respuestaCorrecta < correctas.Length &&
            correctas[datos.respuestaCorrecta] != null)
            correctas[datos.respuestaCorrecta].SetIsOnWithoutNotify(true);
        ActualizarGrupos();
    }

    private void EliminarEstaFila()
    {
        alEliminar?.Invoke(this);
    }

    private void OnDestroy()
    {
        DesconectarEventos();
    }
}