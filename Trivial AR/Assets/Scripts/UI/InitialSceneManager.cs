using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InitialSceneManager : MonoBehaviour
{
    public GameObject progressBarLoadingScene;

    private AudioSource _audioBackground;
    private bool loadScene = false;  //controlo que no se cargue la misma escena varias veces
    private ProgressBarLoadingScene progressBarLoadingSceneScript;


    private void Awake()
    {
        //obtengo el audio
        _audioBackground = GetComponent<AudioSource>();

    }


    // Use this for initialization
    void Start()
    {
        //arranco la música si se eligió en las settings
        if (_audioBackground)
        {
            //arranco la música, si se eligió con música en las settings
            GameManager.instance.playMusic(GameManager.instance.WithMusic, _audioBackground, 0);
        }
        //obtengo acceso al script que controla la barra de progreso
        progressBarLoadingSceneScript = progressBarLoadingScene.GetComponent<ProgressBarLoadingScene>();
        //oculto progressBarLoadingScene
        progressBarLoadingScene.SetActive(false);


    }



    // Method is called when StartButton is clicked
    public void OnClickStartButton()
    {
        //  if (GameManager.instance.IsSignIn)
        if (GameManager.instance.CurrentUser != null)
        {
            // SceneManager.LoadScene("MainScene");
            if (!loadScene)   //la escena  no se ha cargado ya
            {
                loadScene = true; //lo uso para evitar que se cargue varias veces la misma escena

                //hago visible progress bar
                progressBarLoadingScene.SetActive(true);

                //cambio texto barra progreso
                progressBarLoadingSceneScript.textPercent.GetComponent<Text>().text = "Loading...";

                StartCoroutine(LoadScene("MainScene"));
            }
        }
        else
            //debo controlar si ya está logueado cargo la pantalla principal del juego. Sino, cargo la pantalla de login
            //ahora solo cargo la de login directamente.
            SceneManager.LoadScene("LoginScene");
    }





    /* Coroutine which load scene with sceneName in asynchronous way*/
    IEnumerator LoadScene(string sceneName)
    {

        //Begin to load the Scene you specify
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = true;

        //When the load is still in progress, output the Text and progress bar
        while (!asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f); //me da un nº entre 0 y 1. Lo necesito porque si no para en 0.9
            //Output the current progress
            progressBarLoadingSceneScript.currentPercent = progress * 100;
            // m_Text.text = "Loading progress: " + (asyncOperation.progress * 100) + "%";



            yield return null;
        }
    }
}
