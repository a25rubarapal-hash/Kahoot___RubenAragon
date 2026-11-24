using UnityEngine;
using UnityEngine.UI;

public enum TipoAccion
{
    AbrirPanel,
    SalirJuego
}

[System.Serializable]
public class OpcionRueda
{
    [Header("Botón")]
    public Button boton;

    [Header("Acción")]
    public TipoAccion tipoAccion;

    [Header("Canvas que se abrirá (opcional)")]
    public GameObject panelDestino;
}