using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ParpadeoTexto : MonoBehaviour
{
    [SerializeField] private float velocidad = 2f;
    [SerializeField] private float alphaMin = 0.2f;
    [SerializeField] private float alphaMax = 1f;

    private TextMeshProUGUI texto;

    void Awake()
    {
        texto = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.unscaledTime * velocidad, 1f);
        Color c = texto.color;
        c.a = Mathf.Lerp(alphaMin, alphaMax, t);
        texto.color = c;
    }
}