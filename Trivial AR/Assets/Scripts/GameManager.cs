using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    // Unique instance
    public static GameManager instance;
    public GameObject panelContainerMusicScene;



    // USER CONTROL
    public User CurrentUser { get; set; }
    public int CurrentLevel { get; set; }


    //VACUNES -- son los quesitos en un trivial normal
    public int NumberOkAnswersNecessaryToVacune = 3;  //nº de respuestas correctas para conseguir una vacuna (quesito)
    public int CurrentOkAnswersToVacune = 0; //nº de respuestas ok que lleva de las que necesita para conseguir una vacuna (va de 0 a 3)
    public string chosenVacune;//vacuna elegida en el panel de vacunas disponibles (para ganar un quesito).

    public List<string> UserVacunes = new List<string>(); //contendrá las vacunas (quesitos) de este usuario, sus ids

    //CATEGORIES
    public Category currentCategory { get; set; }
    public int NumCategories = 16; //debería leerlo mejor de la BD en el start, pero no me da tiempo

    //QUESTIONS
    public Question CurrentQuestion { get; set; }


    //SCORE FOR QUESTION
    public int ScoreOkQuestion = 10; //puntos vale cada pregunta acertada
    public int ScoreFailedQuestion = -3; //puntos resta cada pregunta fallada o no contestada

    //COUNTERS
    private int score = 0;
    public int Score { get; set; }  //se logran puntos respondiendo preguntas y pasando juegos
    private int totalWonGames = 0;
    public int TotalWonGames { get; set; }      //nº de juegos conseguidos (un quesito de cada categoría)

    public float CountDownTimer = 20;  //contador hacia atrás del tiempo para responder a cada pregunta. Inicialmente son 20 segundos.Almacena el tiempo en cada segundo

    private int totalQuestions = 0;
    public int TotalQuestions { get; set; }     //nº total de preguntas jugadas
    private int totalOkQuestions = 0;
    public int TotalOkQuestions { get; set; }   //nº de preguntas acertadas

    //----------------------------------Consecutive Answers Control--------------------------------
    private int totalOkConsecutiveQuestions = 0;
    public int TotalOkConsecutiveQuestions { get; set; }  //nº de preguntas consecutivas acertadas
    private int tempOkConsecutiveQuestions = 0;
    public int TempOkConsecutiveQuestions { get; set; } //contador temporal de preguntas consecutivas acertadas. Usado para calcular el total


    /*Método que comprueba si hay que incrementar o no las consecutivas respuestas acertadas y si es así, las incrementa*/
    public void UpdateTotalOkConsecutiveQuestions()
    {

        TempOkConsecutiveQuestions++; //incremento contador temporal de consecutivas
        if (TempOkConsecutiveQuestions > 0)
            if (TempOkConsecutiveQuestions > TotalOkConsecutiveQuestions)
                TotalOkConsecutiveQuestions = TempOkConsecutiveQuestions; //sólo lo actualizo si se ha superado, siempre muestra el mayor nº hecho

    }



    /* Método que resetea el contador temporal de respuestas ok consecutivas*/
    public void ResetTempOkConsecutiveQuestions()
    {
        TempOkConsecutiveQuestions = 0;
    }
    //----------------------------------------------------
    private IDictionary<string, int> totalCategoryQuestions = new Dictionary<string, int>(); //contador de preguntas hechas de cada categoría (acertadas y/o falladas)
    public void SetTotalCategoryQuestions(string key, int value)
    {
        if (totalCategoryQuestions.ContainsKey(key))
        {
            totalCategoryQuestions[key] = value;
        }
        else
        {
            totalCategoryQuestions.Add(key, value);
        }
    }



    public void IncTotalCategoryQuestions(string key, int increment)
    {
        if (totalCategoryQuestions.ContainsKey(key))
        {
            totalCategoryQuestions[key] += increment;
        }
        else
        {
            totalCategoryQuestions.Add(key, increment);
        }
    }



    public int GetTotalCategoryQuestions(string key)
    {
        int result = 0;

        if (totalCategoryQuestions.ContainsKey(key))
        {
            result = totalCategoryQuestions[key];
        }

        return result;
    }


    //-------------------------------------------
    private IDictionary<string, int> totalOkCategoryQuestions = new Dictionary<string, int>();  //contador de preguntas acertadas de cada categoría
    public void SetTotalOkCategoryQuestions(string key, int value)
    {
        if (totalOkCategoryQuestions.ContainsKey(key))
        {
            totalOkCategoryQuestions[key] = value;
        }
        else
        {
            totalOkCategoryQuestions.Add(key, value);
        }
    }

    public void IncTotalOkCategoryQuestions(string key, int increment)
    {
        if (totalOkCategoryQuestions.ContainsKey(key))
        {
            totalOkCategoryQuestions[key] += increment;
        }
        else
        {
            totalOkCategoryQuestions.Add(key, increment);
        }
    }


    public int GetTotalOkCategoryQuestions(string key)
    {
        int result = 0;

        if (totalOkCategoryQuestions.ContainsKey(key))
        {
            result = totalOkCategoryQuestions[key];
        }

        return result;
    }


    //------------------------------------------------------------------------------------

    /*Método que reinicia todas las puntuaciones a 0 de memoria. Se usará al cambiar de usuario
    */
    public void RestartAllScores()
    {
        Score = 0;
        CurrentLevel = 1;
        TotalWonGames = 0;
        TotalQuestions = 0;
        TotalOkQuestions = 0;
        TotalOkConsecutiveQuestions = 0;
        totalCategoryQuestions.Clear();
        totalOkCategoryQuestions.Clear();
        CurrentOkAnswersToVacune = 0;
    }


    /*Método que recupera las puntuaciones generales del usuario actual a partir de un objeto UserScore*/
    public void SetUserMainScores(UserScore userScore)
    {
        Score = userScore.Score;
        TotalWonGames = userScore.TotalWonGames;
        TotalQuestions = userScore.TotalQuestions;
        TotalOkQuestions = userScore.TotalOkQuestions;
        TotalOkConsecutiveQuestions = userScore.TotalOkConsecutiveQuestions;
    }



    /*Método que reincio el contador de tiempo*/
    public void RestartCountDownTimer()
    {
        CountDownTimer = 20;
    }





    // DEACTIVATED MARK QUEUE -- contains all  image target which have already been visited previously and therefore, they shouldn't work until
    // a condition is satisfied. They will appear in order of arrival
    public Queue<string> DeactivatedMarkQueue = new Queue<string>();

    public int NumNecessaryMarkToUnlock = 2;

    // VUFORIA Y AR
    public GameObject trackedGameObject; //contiene el ImageTarget detectado en cada momento. Sólo se detecta uno
    public bool IsAlreadyStartedGame = false; //controla si ya se avisó para start, porque al haber más de una marca con el mismo hijo, da problemas, pues notifica tantas veces como marcas haya

    //////////////////////////////////////////////////////////////////////////////


    //MUSIC
    public bool WithMusic { get; set; }
    public bool WithSound { get; set; }
    private AudioSource _audioBackground; //audiosource donde guardaré la música


    private float timeIntroductionMusic = 0.0f; //controla la posición de la música de las pantallas introductorias, para que siga de una a otra
    public float TimeIntroductionMusic
    {
        get { return timeIntroductionMusic; }
        set { TimeIntroductionMusic = value; }
    }





    ////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////   LIFE CICLE  METHODS  //////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////

    void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
            Destroy(gameObject);

        //Call the InitGame function to initialize the first level 
        InitGame();
    }





    void Start()
    {

        getPrefs();

        //arranco la música, si se eligió con música en las settings
        playMusic(WithMusic, _audioBackground, GameManager.instance.TimeIntroductionMusic);
    }




    //Initializes the game for each level.
    void InitGame()
    {
        //obtengo el audio
        _audioBackground = panelContainerMusicScene.GetComponent<AudioSource>();

        // PlayerPrefs.DeleteAll();//solo ahora para probar. Luego quitarlo

    }



    /* lee las preferencias */
    public void getPrefs()
    {
        //comandos read de PlayerPrefs

        WithMusic = (PlayerPrefs.GetInt("withMusic", 1) == 1) ? true : false;
        WithSound = (PlayerPrefs.GetInt("withSounds", 1) == 1) ? true : false;

        CurrentUser = getUserFromPrefs();
        CurrentLevel = PlayerPrefs.GetInt("currentLevel", 1);

    }



    //Lee de las preferencias los datos del currentUser y las devuelve
    public User getUserFromPrefs()
    {
        string email = PlayerPrefs.GetString("emailCurrentUser");
        string idDatabase = PlayerPrefs.GetString("userIdCurrentUser");
        string passwordMd5 = PlayerPrefs.GetString("userPassword");
        string idAuth = PlayerPrefs.GetString("userIdCurrentUser");
        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(idDatabase) && string.IsNullOrEmpty(passwordMd5) &&
            string.IsNullOrEmpty(idAuth)) return null;
        return new User(email, passwordMd5, idAuth, idDatabase);
    }



    /* Método que arranca la música si se le pasa true. La para si estaba sonando, en caso contario.
	  Recuerda posición donde se quedó la reproducción del audio. Devuelve la posición de la música*/
    public float playMusic(bool yesMusic, AudioSource audio, float timeMusic)
    {
        if (yesMusic)
        {
            if (!(audio.isPlaying))
            {
                audio.time = timeMusic;
                audio.Play();
            }
        }
        else
        {
            //paro la música si estaba sonando
            if (audio.isPlaying)
            {
                timeIntroductionMusic = audio.time;
                audio.Stop();
            }
        }
        return (timeIntroductionMusic);
    }






    /*Método que comprueba si hay alguna marca para desbloquear*/
    public bool IsAnyTargetReadyToUnlock()
    {
        if (DeactivatedMarkQueue.Count == NumNecessaryMarkToUnlock + 1)
        {
            return true;
        }
        return false;
    }
}