using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //Declaración singletone MUY básica.
    public static AudioManager Instance;
    //Fin de la declaración

    [Header("Audio Source Reference")]
    [SerializeField] AudioSource musicSource; //Variable de referencia al "altavoz de música"
    [SerializeField] AudioSource sfxSource; //Variable de ref al "altavoz de los efectos de sonido"
    [SerializeField] AudioSource sfxSource2;
    [SerializeField] AudioSource sfxSource3;

    [Header("Audio Clip Arrays")]
    [SerializeField] AudioClip[] musicList; //Array que almacena TODAS las canciones del juego.
    [SerializeField] AudioClip[] sfxList; //Array que almacena TODOS los sonidos del juego.

    [Header("Audio Stats")]
    public float musicVolume;
    public float sfxVolume;

    private void Awake()
    {
        //Singleton : Referencia a si mismo y que no se destruya entre escenas.
        if (Instance == null)
        {
            Instance = this; //El singleton se autoreferencia
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject); //Si hay otro audio manager, se destruye el "nuevo/duplicado"
        }
    }

    public void Start()
    {
        PlayMusic(0); 
    }

    public void PlayMusic(int musicToPlay)
    {
        musicSource.clip = musicList[musicToPlay];
        musicSource.Play();
    }

    public void PlaySFX(int soundToPlay)
    {
        if (soundToPlay == 2 || soundToPlay == 5 || soundToPlay == 7 || soundToPlay == 9) { sfxSource2.PlayOneShot(sfxList[soundToPlay]); }
        else if (soundToPlay == 11) { sfxSource3.PlayOneShot(sfxList[soundToPlay]); }
        else { sfxSource.PlayOneShot(sfxList[soundToPlay]); }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

}


