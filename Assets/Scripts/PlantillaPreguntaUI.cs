using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Se coloca en la raiz del prefab Question (ABCD) y del prefab TF.
// La raiz, enunciado y botones deben tener las Tags documentadas.
public class PlantillaPreguntaUI : MonoBehaviour
{
    [SerializeField] private Color colorCorrecta = new Color(0.1f, 0.68f, 0.22f);
    [SerializeField] private Color colorIncorrecta = new Color(0.85f, 0.13f, 0.18f);

    private readonly List<Button> botones = new List<Button>();

    public bool Preparar(PreguntaKahoot pregunta, Action<int> alResponder)
    {
        LimpiarListeners();
        botones.Clear();
        if (pregunta == null) return false;

        string tagRaizEsperada = pregunta.tipo == "TF" ? "TF" : "Question";
        if (gameObject.tag != tagRaizEsperada)
        {
            Debug.LogError("La plantilla " + name + " debe tener Tag " + tagRaizEsperada);
            return false;
        }

        TMP_Text enunciado = BuscarPorTag<TMP_Text>("Enunciado");
        if (enunciado == null)
        {
            Debug.LogError("No se encontro TMP_Text con Tag Enunciado dentro de " + name);
            return false;
        }
        enunciado.text = pregunta.enunciado;

        string[] etiquetas = pregunta.tipo == "TF"
            ? new[] { "True", "False" }
            : new[] { "A", "B", "C", "D" };

        for (int i = 0; i < etiquetas.Length; i++)
        {
            Button boton = BuscarPorTag<Button>(etiquetas[i]);
            if (boton == null)
            {
                Debug.LogError("No se encontro Button con Tag " + etiquetas[i] + " en " + name);
                LimpiarListeners();
                botones.Clear();
                return false;
            }
            TMP_Text texto = boton.GetComponentInChildren<TMP_Text>(true);
            if (texto == null)
            {
                Debug.LogError("El boton " + etiquetas[i] + " no contiene TMP_Text.");
                LimpiarListeners();
                botones.Clear();
                return false;
            }
            texto.text = pregunta.respuestas[i];
            botones.Add(boton);
            int indice = i;
            UnityEngine.Events.UnityAction accion = () => alResponder?.Invoke(indice);
            boton.onClick.AddListener(accion);
            accionesUnity.Add(accion);
        }
        return true;
    }

    private readonly List<UnityEngine.Events.UnityAction> accionesUnity =
        new List<UnityEngine.Events.UnityAction>();

    private T BuscarPorTag<T>(string tagBuscada) where T : Component
    {
        foreach (Transform hijo in GetComponentsInChildren<Transform>(true))
        {
            if (hijo.gameObject.tag == tagBuscada)
                return hijo.GetComponent<T>();
        }
        return null;
    }

    public void MostrarSolucion(int indiceCorrecto, int indiceSeleccionado)
    {
        for (int i = 0; i < botones.Count; i++)
        {
            Button boton = botones[i];
            boton.interactable = false;
            // Evitar que el tinte de "Disabled" tape la indicacion de correcto/incorrecto.
            boton.transition = Selectable.Transition.None;
            Graphic grafico = boton.targetGraphic;
            if (grafico == null) continue;
            if (i == indiceCorrecto) grafico.color = colorCorrecta;
            else if (i == indiceSeleccionado) grafico.color = colorIncorrecta;
        }
    }

    private void LimpiarListeners()
    {
        for (int i = 0; i < accionesUnity.Count && i < botones.Count; i++)
            if (botones[i] != null)
                botones[i].onClick.RemoveListener(accionesUnity[i]);
        accionesUnity.Clear();
    }

    private void OnDestroy() { LimpiarListeners(); }
}
