using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GestorCreacionesUI : MonoBehaviour
{
    [Header("Vistas internas de CreacionDeNivel")]
    [SerializeField] private GameObject misCreaciones;
    [SerializeField] private GameObject editorKahoot;

    [Header("Lista de Mis creaciones")]
    [SerializeField] private Transform contenidoLista;
    [SerializeField] private FilaCreacionKahoot prefabCreacion;
    [SerializeField] private ScrollRect scrollLista;

    [Header("Botones")]
    [SerializeField] private Button botonMas;
    [SerializeField] private Button botonVolverLista; // Opcional: ESC tambien funciona

    [Header("Editor")]
    [SerializeField] private CreadorKahoots creador;

    private bool escuchando;

    private void Awake()
    {
        ResolverReferencias();
        ConectarEventos();
    }

    private void ResolverReferencias()
    {
        if (creador == null)
            creador = GetComponentInChildren<CreadorKahoots>(true);
        if (botonMas == null && contenidoLista != null)
        {
            Transform mas = contenidoLista.Find("BotonMas");
            if (mas != null) botonMas = mas.GetComponent<Button>();
        }
    }

    private void ConectarEventos()
    {
        if (escuchando) return;
        if (botonMas != null) botonMas.onClick.AddListener(NuevaCreacion);
        if (botonVolverLista != null)
            botonVolverLista.onClick.AddListener(MostrarLista);
        if (creador != null) creador.KahootGuardado += AlGuardar;
        escuchando = true;
    }

    private void OnEnable()
    {
        ResolverReferencias();
        if (!escuchando) ConectarEventos();
        MostrarLista();
        if (botonMas == null)
            Debug.LogError("GestorCreacionesUI: falta BotonMas. Asignalo en el Inspector.", this);
        if (creador == null)
            Debug.LogError("GestorCreacionesUI: falta CreadorKahoots.", this);
    }

    public void MostrarLista()
    {
        if (misCreaciones == null || editorKahoot == null || misCreaciones == editorKahoot)
        {
            Debug.LogError("GestorCreacionesUI: asigna dos vistas DIFERENTES: Mis Creaciones y Editor Kahoot.", this);
            return;
        }
        editorKahoot.SetActive(false);
        misCreaciones.SetActive(true);
        RefrescarLista();
    }

    public void RefrescarLista()
    {
        if (contenidoLista == null || prefabCreacion == null || botonMas == null)
        {
            Debug.LogError("GestorCreacionesUI: faltan Content, F (plantilla) o BotonMas.", this);
            return;
        }
        if (botonMas.transform.parent != contenidoLista)
        {
            Debug.LogError("BotonMas debe ser hijo DIRECTO de Content.", botonMas);
            return;
        }

        // La F original puede ser prefab de Assets o F de la escena.
        // Si F esta en Content, mantenerla oculta como plantilla.
        if (prefabCreacion.transform.parent == contenidoLista)
            prefabCreacion.gameObject.SetActive(false);

        var eliminar = new List<GameObject>();
        foreach (Transform hijo in contenidoLista)
        {
            if (hijo == prefabCreacion.transform) continue;
            if (hijo.GetComponent<FilaCreacionKahoot>() != null)
                eliminar.Add(hijo.gameObject);
        }
        foreach (GameObject objeto in eliminar)
        {
            objeto.SetActive(false);
            Destroy(objeto);
        }

        List<CreacionGuardada> creaciones = RepositorioCreacionesKahoot.Obtener();
        foreach (CreacionGuardada creacion in creaciones)
        {
            FilaCreacionKahoot fila = Instantiate(prefabCreacion, contenidoLista, false);
            fila.name = "Quiz_" + creacion.Datos.titulo;
            fila.gameObject.SetActive(true);
            fila.Configurar(creacion, EditarCreacion, BorrarCreacion);
            fila.transform.SetSiblingIndex(botonMas.transform.GetSiblingIndex());
        }
        botonMas.transform.SetAsLastSibling();
        if (contenidoLista is RectTransform rect)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
        if (scrollLista != null) scrollLista.verticalNormalizedPosition = 1f;
        Debug.Log("Mis creaciones: " + creaciones.Count + " Kahoot(s) locales. Carpeta: " + AlmacenKahoots.CarpetaKahoots);
    }

    public void NuevaCreacion()
    {
        Debug.Log("Boton + pulsado: abrir editor");
        ResolverReferencias();
        if (creador == null || editorKahoot == null)
        {
            Debug.LogError("No puedo abrir el editor: falta CreadorKahoots o EditorKahoot.", this);
            return;
        }
        MostrarEditor(); // Activarlo ANTES de inicializar las filas
        creador.NuevaCreacion();
    }

    private void EditarCreacion(string ruta)
    {
        if (creador == null || editorKahoot == null) return;
        MostrarEditor();
        if (!creador.EditarCreacion(ruta)) MostrarLista();
    }

    private void BorrarCreacion(string ruta)
    {
        if (!RepositorioCreacionesKahoot.Eliminar(ruta)) return;
        RefrescarLista();
        SelectorKahoots selector = FindFirstObjectByType<SelectorKahoots>(FindObjectsInactive.Include);
        if (selector != null) selector.RefrescarListado();
        Debug.Log("El Kahoot tambien se ha quitado del selector Jugar.");
    }

    private void MostrarEditor()
    {
        if (misCreaciones != null) misCreaciones.SetActive(false);
        if (editorKahoot != null) editorKahoot.SetActive(true);
    }

    private void AlGuardar(KahootDatos datos)
    {
        Debug.Log("Mis creaciones: guardado recibido " + datos.titulo);
        MostrarLista();
    }

    // Lo usa EscKey: ESC en el editor vuelve a Mis creaciones.
    public bool VolverDesdeEditor()
    {
        if (editorKahoot == null || !editorKahoot.activeSelf) return false;
        MostrarLista();
        return true;
    }

    private void OnDestroy()
    {
        if (botonMas != null) botonMas.onClick.RemoveListener(NuevaCreacion);
        if (botonVolverLista != null) botonVolverLista.onClick.RemoveListener(MostrarLista);
        if (creador != null) creador.KahootGuardado -= AlGuardar;
    }
}
