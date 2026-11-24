using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

internal static class VistaQuiz
{
    public static readonly Color Fondo = new Color32(34, 20, 62, 255);
    public static readonly Color Tarjeta = new Color32(59, 39, 94, 255);
    public static readonly Color Acento = new Color32(106, 62, 180, 255);

    public static RectTransform Rect(Transform padre, string nombre, float x, float y, float x2, float y2)
    {
        var r = new GameObject(nombre, typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(padre, false);
        r.gameObject.layer = padre.gameObject.layer;
        r.anchorMin = new Vector2(x,y);
        r.anchorMax = new Vector2(x2,y2);
        r.offsetMin = r.offsetMax = Vector2.zero;
        return r;
    }

    public static RectTransform Panel(Transform padre, string nombre, float x, float y, float x2, float y2, Color color)
    {
        var r = Rect(padre,nombre,x,y,x2,y2);
        r.gameObject.AddComponent<Image>().color = color;
        return r;
    }

    public static TMP_Text Texto(Transform padre, string valor, float x, float y, float x2, float y2, int tam)
    {
        var r = Rect(padre,"Texto",x,y,x2,y2);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = valor; t.richText = false; t.alignment = TextAlignmentOptions.Midline;
        t.enableAutoSizing = true; t.fontSizeMin = 12; t.fontSizeMax = tam;
        t.overflowMode = TextOverflowModes.Ellipsis; t.raycastTarget = false;
        return t;
    }

    public static Button Boton(Transform padre, string texto, float x, float y, float x2, float y2, UnityAction accion)
    {
        var r = Panel(padre,texto,x,y,x2,y2,Acento);
        var b = r.gameObject.AddComponent<Button>();
        b.targetGraphic = r.GetComponent<Image>();
        var colores = b.colors;
        colores.selectedColor = colores.highlightedColor = new Color(1,.82f,.58f);
        b.colors = colores; b.onClick.AddListener(accion);
        Texto(r,texto,.04f,.06f,.96f,.94f,26);
        return b;
    }
}
