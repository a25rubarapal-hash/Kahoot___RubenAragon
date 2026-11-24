using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FilaCreacionKahoot : MonoBehaviour
{
    [Header("Datos de la fila F")]
    [SerializeField] private TMP_Text nombreNivel;
    [SerializeField] private Button botonEditar;
    [SerializeField] private Button botonBorrar;

    private string ruta;
    private Action<string> alEditar;
    private Action<string> alBorrar;
    private TMP_Text textoBotonBorrar;
    private string etiquetaOriginalBorrar;
    private float confirmarHasta;

    private void Awake()
    {
        if (botonEditar != null) botonEditar.onClick.AddListener(Editar);
        if (botonBorrar != null)
        {
            botonBorrar.onClick.AddListener(SolicitarBorrado);
            textoBotonBorrar = botonBorrar.GetComponentInChildren<TMP_Text>(true);
            if (textoBotonBorrar != null)
                etiquetaOriginalBorrar = textoBotonBorrar.text;
        }
    }

    public void Configurar(CreacionGuardada creacion, Action<string> editar, Action<string> borrar)
    {
        if (creacion == null) return;
        ruta = creacion.Ruta;
        alEditar = editar;
        alBorrar = borrar;
        confirmarHasta = 0f;
        RestaurarBorrar();
        if (nombreNivel != null)
        {
            nombreNivel.text = creacion.Datos.titulo;
            nombreNivel.enableAutoSizing = true;
            nombreNivel.fontSizeMin = 16f;
            nombreNivel.fontSizeMax = 30f;
            nombreNivel.textWrappingMode = TextWrappingModes.NoWrap;
            nombreNivel.overflowMode = TextOverflowModes.Ellipsis;
        }
    }

    private void Editar()
    {
        Debug.Log("Editar Kahoot: " + ruta);
        if (!string.IsNullOrEmpty(ruta)) alEditar?.Invoke(ruta);
    }

    private void SolicitarBorrado()
    {
        if (string.IsNullOrEmpty(ruta)) return;
        if (confirmarHasta > 0f && Time.unscaledTime <= confirmarHasta)
        {
            confirmarHasta = 0f;
            alBorrar?.Invoke(ruta);
            return;
        }
        confirmarHasta = Time.unscaledTime + 4f;
        if (textoBotonBorrar != null) textoBotonBorrar.text = "Confirmar";
        Debug.LogWarning("Pulsa BORRAR otra vez en 4 segundos para eliminar: " + ruta);
    }

    private void Update()
    {
        if (confirmarHasta > 0f && Time.unscaledTime > confirmarHasta)
        {
            confirmarHasta = 0f;
            RestaurarBorrar();
        }
    }

    private void RestaurarBorrar()
    {
        if (textoBotonBorrar != null)
            textoBotonBorrar.text = etiquetaOriginalBorrar;
    }

    private void OnDestroy()
    {
        if (botonEditar != null) botonEditar.onClick.RemoveListener(Editar);
        if (botonBorrar != null) botonBorrar.onClick.RemoveListener(SolicitarBorrado);
    }
}

