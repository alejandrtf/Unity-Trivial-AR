using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour {

    private Toggle _withMusic, _withSound;
    //música y sonidos control
    private AudioSource _audioBackground;
    //música de fondo



    // Use this for initialization
    void Awake()
    {
        _withMusic = GameObject.Find("MusicToggle").GetComponent<Toggle>();
        _withSound = GameObject.Find("SoundsToggle").GetComponent<Toggle>();
        _audioBackground = GameObject.Find("MainSceneManager").GetComponent<AudioSource>();

    }


    void Start()
    {

        //actualizo las casillas checkbox con los valores de GameManager
        _withMusic.isOn = GameManager.instance.WithMusic;
        _withSound.isOn = GameManager.instance.WithSound;
    }



    /* método llamado al cambiar el valor del toggle con música (SI/NO) */
    public void OnValueChangeMusic()
    {
        //cambio valor a parámetro withMusic del GameManager
        GameManager.instance.WithMusic = _withMusic.isOn;
        //grabo en preferencias
        PlayerPrefs.SetInt("withMusic", (_withMusic.isOn == true) ? 1 : 0);
        //actualizo comportamiento música de esta escena
        GameManager.instance.playMusic(GameManager.instance.WithMusic, _audioBackground, GameManager.instance.TimeIntroductionMusic);

    }


    /* método llamado al cambiar el valor del toggle con sonido(SI/NO) */
    public void OnValueChangeSound()
    {
        //cambio el valor al parámetro withSound del GameManager
        GameManager.instance.WithSound = _withSound.isOn;
        //grabo en preferencias
        PlayerPrefs.SetInt("withMusic", (_withMusic.isOn == true) ? 1 : 0);
    }



}
