using UnityEngine;
using UnityEngine.UI;

public sealed class AjustesKahoot : MonoBehaviour
{
    [SerializeField] Slider general, musica, efectos;
    [SerializeField] Toggle pantalla;
    AudioSource pista, sonido;
    AudioClip clic;

    void Awake()
    {
        pista = gameObject.AddComponent<AudioSource>();
        sonido = gameObject.AddComponent<AudioSource>();
        pista.clip = Resources.Load<AudioClip>("AudioKahoot/Menu");
        clic = Resources.Load<AudioClip>("AudioKahoot/Clic");
        pista.loop = true;
        Cargar(general, "General");
        Cargar(musica, "Musica");
        Cargar(efectos, "Efectos");
        AplicarVolumen();
        pista.Play();
        pantalla.SetIsOnWithoutNotify(PlayerPrefs.GetInt("PantallaCompleta", 0) == 1);
        AplicarPantalla(pantalla.isOn);
        pantalla.onValueChanged.AddListener(CambiarPantalla);
        foreach (var boton in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            boton.onClick.AddListener(Sonar);
    }

    void Cargar(Slider control, string clave)
    {
        control.SetValueWithoutNotify(PlayerPrefs.GetFloat(clave, 0.8f));
        control.onValueChanged.AddListener(valor =>
        {
            PlayerPrefs.SetFloat(clave, valor);
            AplicarVolumen();
            if (control == efectos && !sonido.isPlaying) Sonar();
        });
    }

    void AplicarVolumen()
    {
        AudioListener.volume = general.value;
        pista.volume = musica.value;
        sonido.volume = efectos.value;
    }

    void Sonar() => sonido.PlayOneShot(clic);

    static void AplicarPantalla(bool completa)
    {
#if UNITY_STANDALONE && !UNITY_EDITOR
        Screen.SetResolution(1920, 1080, completa ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
#endif
    }

    void CambiarPantalla(bool activa)
    {
        AplicarPantalla(activa);
        PlayerPrefs.SetInt("PantallaCompleta", activa ? 1 : 0);
    }
}
