using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static VistaQuiz;

public class MenuJugador : MonoBehaviour
{
    enum Seccion { Clasificacion, Informes }
    static MenuJugador actual;
    [SerializeField] RectTransform capa, tabla, acceso, creditos, navegacion;
    [SerializeField] Button botonTabla, botonInformes;
    [SerializeField] CanvasGroup selector;
    [SerializeField] Button cambiarJugador;
    Transform padreVentanas;
    bool abiertoDesdeSelector;
    PerfilLocal perfil;
    List<KahootDatos> quizzes;
    int indice;
    RectTransform contenido, cabecera;
    ScrollRect listado;
    CanvasGroup grupo;
    TMP_Text titulo;
    bool visible;

    public static bool ModalAbierto => actual != null && actual.capa != null && actual.capa.gameObject.activeInHierarchy;
    public static bool EscribiendoNombre => ModalAbierto ||
        (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null &&
        EventSystem.current.currentSelectedGameObject.GetComponentInParent<Button>()?.transform.parent.name == "Accesos superiores");

    public static bool CerrarVentana()
    {
        if (!ModalAbierto) return false;
        actual.capa.gameObject.SetActive(false);
        if (actual.abiertoDesdeSelector)
        {
            actual.capa.SetParent(actual.padreVentanas, false);
            actual.selector.interactable = true;
            actual.abiertoDesdeSelector = false;
            actual.cambiarJugador.Select();
            return true;
        }
        EventSystem.current?.SetSelectedGameObject(null);
        return true;
    }

    void Awake()
    {
        actual = this;
        padreVentanas = capa.parent;
        var grupoVentana = capa.GetComponent<CanvasGroup>();
        if (grupoVentana == null) grupoVentana = capa.gameObject.AddComponent<CanvasGroup>();
        grupoVentana.ignoreParentGroups = true;
        grupo = GetComponent<CanvasGroup>();
        perfil = PerfilLocal.Crear(Rect(acceso,"Perfil",.04f,.18f,.96f,.54f));
        var panel = tabla;
        titulo = Texto(navegacion,"",.14f,0,.86f,1,25);
        cabecera = Rect(panel,"Columnas",.04f,.59f,.90f,.68f);
        Columnas(cabecera,"#","Jugador","Puntos","Tiempo");
        var zona = Rect(panel,"Listado",.04f,.04f,.96f,.59f);
        listado = zona.gameObject.AddComponent<ScrollRect>();
        listado.horizontal = false; listado.movementType = ScrollRect.MovementType.Clamped;
        listado.scrollSensitivity = 32;
        var viewport = Panel(zona,"Viewport",0,0,.935f,1,Tarjeta);
        viewport.gameObject.AddComponent<RectMask2D>();
        contenido = Rect(viewport,"Content",0,1,1,1);
        contenido.pivot = new Vector2(.5f,1);
        var filas = contenido.gameObject.AddComponent<VerticalLayoutGroup>();
        filas.spacing = 4; filas.childForceExpandHeight = false;
        contenido.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        listado.viewport = viewport; listado.content = contenido;
        var scrollBar = DefaultControls.CreateScrollbar(new DefaultControls.Resources()).GetComponent<Scrollbar>();
        scrollBar.transform.SetParent(zona,false);
        var r = (RectTransform)scrollBar.transform;
        r.anchorMin = new Vector2(.945f,0); r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
        scrollBar.SetDirection(Scrollbar.Direction.BottomToTop,false);
        scrollBar.targetGraphic.color = new Color(.75f,.64f,.95f);
        listado.verticalScrollbar = scrollBar;
        capa.gameObject.SetActive(false);
    }

    void Update()
    {
        bool abierto = grupo.interactable && grupo.alpha >= .99f;
        if (abierto && !visible)
        {
            perfil.Refrescar();
            string anterior = quizzes != null && quizzes.Count > 0 ? quizzes[indice].claveArchivo : "";
            quizzes = new CatalogoKahoots().ObtenerDisponibles();
            indice = Mathf.Max(0,quizzes.FindIndex(q => q.claveArchivo == anterior));
            capa.gameObject.SetActive(false);
        }
        visible = abierto;
    }

    void OnDisable() { visible = false; }

    public void AbrirSesionDesdeSelector()
    {
        if (!selector.interactable || ModalAbierto) return;
        abiertoDesdeSelector = true;
        capa.SetParent(selector.transform, false);
        selector.interactable = false;
        Mostrar(acceso);
    }

    public void AbrirSesion() => Mostrar(acceso);
    public void AbrirCreditos() => Mostrar(creditos);
    public void AbrirClasificacion() => Abrir(Seccion.Clasificacion);
    public void AbrirInformes() => Abrir(Seccion.Informes);
    public void Anterior() => Cambiar(-1);
    public void Siguiente() => Cambiar(1);
    public void Cerrar() => CerrarVentana();

    void Mostrar(RectTransform ventana)
    {
        capa.SetAsLastSibling();
        capa.gameObject.SetActive(true);
        acceso.gameObject.SetActive(ventana == acceso);
        creditos.gameObject.SetActive(ventana == creditos);
        tabla.gameObject.SetActive(ventana == tabla);
        if (ventana == acceso) perfil.AbrirFormulario();
    }

    void Cambiar(int paso)
    {
        if (quizzes == null || quizzes.Count == 0) return;
        indice = (indice + paso + quizzes.Count) % quizzes.Count;
        Abrir(Seccion.Clasificacion);
    }

    void Limpiar()
    {
        foreach (Transform hijo in contenido) { hijo.gameObject.SetActive(false); Destroy(hijo.gameObject); }
        listado.StopMovement(); listado.verticalNormalizedPosition = 1;
    }

    void Abrir(Seccion destino)
    {
        if (quizzes == null) quizzes = new CatalogoKahoots().ObtenerDisponibles();
        Mostrar(tabla);
        Limpiar();
        bool mostrarTabla = destino == Seccion.Clasificacion;
        navegacion.gameObject.SetActive(mostrarTabla);
        cabecera.gameObject.SetActive(mostrarTabla);
        botonTabla.interactable = !mostrarTabla;
        botonInformes.interactable = mostrarTabla;
        ((RectTransform)listado.transform).anchorMax = new Vector2(.96f,mostrarTabla ? .59f : .80f);
        if (!mostrarTabla) CargarInformes();
        else
        {
            titulo.text = quizzes.Count == 0 ? "Sin cuestionarios" : quizzes[indice].titulo;
            if (quizzes.Count == 0) return;
            var marcas = ClasificacionKahoot.LeerTabla(quizzes[indice],out string estado);
            if (marcas.Count == 0) Parrafo(estado ?? "Todavía no hay partidas.",22);
            for (int i = 0; i < marcas.Count; i++)
            {
                var marca = marcas[i];
                Columnas(Fila(43),(i+1).ToString(),marca.Jugador,marca.Puntos.ToString(),marca.Segundos.ToString("0.0")+" s");
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(contenido);
        listado.verticalNormalizedPosition = 1;
    }

    RectTransform Fila(float altura)
    {
        var r = Rect(contenido,"Fila",0,0,1,1);
        r.gameObject.AddComponent<LayoutElement>().preferredHeight = altura;
        return r;
    }

    static void Columnas(RectTransform r,string puesto,string jugador,string puntos,string tiempo)
    {
        Texto(r,puesto,0,0,.09f,1,21);
        Texto(r,jugador,.10f,0,.53f,1,21);
        Texto(r,puntos,.54f,0,.76f,1,21);
        Texto(r,tiempo,.77f,0,1,1,21);
    }

    void Parrafo(string valor,int tam)
    {
        Canvas.ForceUpdateCanvases();
        var r = Fila(48);
        var t = Texto(r,valor,.03f,0,.97f,1,tam);
        t.enableAutoSizing = false; t.fontSize = tam; t.alignment = TextAlignmentOptions.TopLeft;
        t.textWrappingMode = TextWrappingModes.Normal;
        float ancho = Mathf.Max(80,listado.viewport.rect.width * .94f);
        r.GetComponent<LayoutElement>().preferredHeight = t.GetPreferredValues(valor,ancho,Mathf.Infinity).y + 14;
    }

    void CargarInformes()
    {
        try
        {
            string carpeta = AlmacenKahoots.CarpetaInformes;
            var rutas = Directory.Exists(carpeta) ? Directory.GetFiles(carpeta,"*.txt") : Array.Empty<string>();
            Array.Sort(rutas,StringComparer.Ordinal); Array.Reverse(rutas);
            if (rutas.Length == 0) Parrafo("Todavía no hay informes.",22);
            foreach (string ruta in rutas)
                Boton(Fila(48),Path.GetFileName(ruta),0,0,1,1,() => LeerInforme(ruta));
        }
        catch (Exception ex) { Parrafo(ex.Message,20); }
    }

    void LeerInforme(string ruta)
    {
        Limpiar();
        Boton(Fila(44),"Volver a informes",0,0,1,1,() => Abrir(Seccion.Informes));
        try { Parrafo(File.ReadAllText(ruta),20); }
        catch (Exception ex) { Parrafo(ex.Message,20); }
    }
}
