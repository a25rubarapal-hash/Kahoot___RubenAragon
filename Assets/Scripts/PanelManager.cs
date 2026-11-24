using UnityEngine;

public class PanelManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup panelActual;
    [SerializeField] private CanvasGroup panelSiguiente;

    void Start()
    {
        Mostrar(panelActual);
        Ocultar(panelSiguiente);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void CambiarPanel()
    {
        Ocultar(panelActual);
        Mostrar(panelSiguiente);
    }

    void Mostrar(CanvasGroup cg)
    {
        cg.gameObject.SetActive(true);
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    void Ocultar(CanvasGroup cg)
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        cg.gameObject.SetActive(false); // desaparece del todo
    }
}