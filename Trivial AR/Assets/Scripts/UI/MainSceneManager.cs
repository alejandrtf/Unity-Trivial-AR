using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSceneManager : MonoBehaviour
{
    public GameObject profilePanel;
    public GameObject settingsPanel;
    public GameObject rankingPanel;
    public GameObject chooseVacunePanel; //permite escoger la vacuna (quesito a conseguir)
    public GameObject[] vacunes; // las vacunas conseguidas (quesitos). si no están conseguidas aparecen desvanecidas

    public Text userEmail;
    public Text currentLevel;
    public GameObject progressbarToObtainVacune; //barra de progreso que indica cuántas preguntas llevas acumuladas y cuántas de faltan para conseguir una vacuna (quesito)




    private AudioSource _audioMainBackground;
    private GameObject _progressbarRankingPanel; //barra de progreso mientras carga el ranking
    private ProgressBar _progressBarScript; //almacenaré el script que controla la barra toObtainVacune





    private void Awake()
    {
        //cargo todos las puntuaciones y vacunas almacenadas para ese usuario en firebase
        //cargo las puntuaciones de este usuario de la BD FirebaseDatabase
        FirebaseDatabaseManager.instance.GetScoreFromFirebase(GameManager.instance.CurrentUser);

        //cargo las vacunas del usuario de la BD Firebase (son los quesitos)
        FirebaseDatabaseManager.instance.GetVacuneFromFirebase(GameManager.instance.CurrentUser);


    }




    // Use this for initialization
    void Start()
    {
        //obtengo la música
        _audioMainBackground = GetComponent<AudioSource>();
        //reproduzco la música
        if (_audioMainBackground)
        {

            //arranco la música, si se eligió con música en las settings
            GameManager.instance.playMusic(GameManager.instance.WithMusic, _audioMainBackground, 0);
        }

        if (profilePanel.activeInHierarchy)
        {
            userEmail.text = GameManager.instance.CurrentUser.Email;
            currentLevel.text = "Nivel " + GameManager.instance.CurrentLevel;
        }

        //obtengo referencia a la barra de progreso del panel ranking
        _progressbarRankingPanel = rankingPanel.transform.Find("ProgressBar").gameObject;
        //obtengo acceso al script que controla la barra de progreso toObtainVacune
        _progressBarScript = progressbarToObtainVacune.GetComponent<ProgressBar>();


    }

    private void OnEnable()
    {
        //registro listeners
        Messenger.AddListener("stopMusic", StopMusic);
        //reseteo vacuna elegida
        if (!String.IsNullOrEmpty(GameManager.instance.chosenVacune))
        {
            //vine de escoger una vacuna en el panel ChooseVAcune.Por tanto, lo cierro

            GameManager.instance.chosenVacune = null;
        }

    }




    // Update is called once per frame
    void Update()
    {
        //actualizo la barra de progreso que muestra el progreso hacia la vacuna
        UpdateProgressBarToVacune();
        if (GameManager.instance.CurrentOkAnswersToVacune == GameManager.instance.NumberOkAnswersNecessaryToVacune)
        {
            //puedes optar a conseguir una vacuna(quesito)
            if (!chooseVacunePanel.activeInHierarchy)
                chooseVacunePanel.SetActive(true);
            GameManager.instance.CurrentOkAnswersToVacune = 0; //reinicio el contador
        }


        //actualizar el dibujo de las vacunas conseguidas. mirar como las dibujo. pero es activar y desactivar segun la lista de lsa que tenga
        DrawVacunesUI();

        //comprobar si gana un juego que será cuando GAmemanagre.instance.listadevacunas=16 (que es el nº de categorias).
        //seria mejor  leerlo de una variable de Gamemanager, pero no me dio tiempo
        CheckWinGame();
    }




    // Method is called when HomeButton is clicked
    public void OnClickHomeButton()
    {
        SceneManager.LoadScene("InitialScene");
    }


    // Method is called when ProfileButton is clicked
    public void OnClickProfileButton()
    {
        if (!profilePanel.activeInHierarchy)
        {
            //hacer visible un panel Profile con las opciones adecuadas y un botón de x de cerrar
            profilePanel.SetActive(true);
        }
    }




    // Method is called when SettingButton is clicked
    public void OnClickSettingButton()
    {
        if (!settingsPanel.activeInHierarchy)
            settingsPanel.SetActive(true);
    }


    // Method is called when CloseSettingsButton is clicked
    public void OnClickCloseSettingsButton()
    {
        if (settingsPanel.activeInHierarchy)
        {
            //hacer invisible el panel settings 
            settingsPanel.SetActive(false);
        }
    }


    // Method is called when RankingButton is clicked
    public void OnClickRankingButton()
    {
        if (!rankingPanel.activeInHierarchy)
        {
            //hacer visible un panel Ranking con las opciones adecuadas y un botón de x de cerrar
            rankingPanel.SetActive(true);
            _progressbarRankingPanel.SetActive(true); //hago visible la barra de progreso de carga del ranking
                                                      // Messenger.Broadcast("LoadRanking");
        }
    }



    /*Method is called when logout button is clicked*/
    public void OnClickLogoutButton()
    {
        //Actualizo en la Bd de Firebase las puntuaciones del currentUser
        FirebaseDatabaseManager.instance.UpdateScoreToFirebase(GameManager.instance.currentCategory, GameManager.instance.CurrentUser);


        //Borro las puntuaciones del usuario actual del GameManager
        GameManager.instance.RestartAllScores();

        if (FirebaseLoginManager.instance != null)
            FirebaseLoginManager.instance.SignOut();

        //borro el usuario del GameManager
        GameManager.instance.CurrentUser = null;
        //actualizo prefs
        PlayerPrefs.DeleteKey("emailCurrentUser");
        PlayerPrefs.DeleteKey("userIdCurrentUser");
        PlayerPrefs.Save();
        AuthUI.LoadLoginScene();

    }




    //para la música principal, porque se va a reproducir otra
    private void StopMusic()
    {
        if (_audioMainBackground)
        {

            //arranco la música, si se eligió con música en las settings
            GameManager.instance.playMusic(GameManager.instance.WithMusic, _audioMainBackground, 0);
        }
    }



    /*Método que actualiza la barra de progreso que muestra el progreso hacia la vacuna
     * */
    private void UpdateProgressBarToVacune()
    {
        //99 porque la barra va de 0 a 100 sin llegar a 100
        _progressBarScript.currentPercent = GameManager.instance.CurrentOkAnswersToVacune * (99 / GameManager.instance.NumberOkAnswersNecessaryToVacune);

    }



    /* Método que muestra en el interfaz UI las vacunas conseguidas*/
    private void DrawVacunesUI()
    {
        if (GameManager.instance.UserVacunes.Count > 0)
        {
            foreach (string idCategory in GameManager.instance.UserVacunes)
            {
                int index = System.Array.FindIndex<GameObject>(vacunes, vacune => vacune.tag == idCategory);
                if (index != -1)
                {
                    //encontrado, por tanto, esa categoría ya la tiene el usuario, la activo en el UI (quitarle alpha)
                    Image image = vacunes[index].GetComponent<Image>();
                    image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);//1f totalmente visible (alpha 255)
                }
            }
        }
        else
        {
            //no hay vacunas, debo ocultar todo
            foreach (GameObject vacune in vacunes)
            {
                Image image = vacune.GetComponent<Image>();
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0.45f);
            }
        }
    }




    /*Método que comprueba si se ha ganado el juego (se tienen todas las vacunas) o no
     */
    private void CheckWinGame()
    {
        if (GameManager.instance.UserVacunes.Count == GameManager.instance.NumCategories)

        {
            //juego ganado; tiene todas las vacunas (quesitos)

            //incremento contador de juegos ganados
            GameManager.instance.TotalWonGames++;

            //cierro el panel chooseVacune
            chooseVacunePanel.SetActive(false);

            //actualizo Firebase
            FirebaseDatabaseManager.instance.UpdateWonGamesToFirebase(GameManager.instance.CurrentUser);

            //borro las vacunas, para empezar de nuevo de GameManager
            GameManager.instance.UserVacunes.Clear();
            //borro las vacunas del FirebaseDatabase
            FirebaseDatabaseManager.instance.DeleteVacuneFromFirebase(GameManager.instance.CurrentUser);

        }
    }
}
