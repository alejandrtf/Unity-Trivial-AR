using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using UnityEngine.SceneManagement;

public class BatsGameInstantiator : MonoBehaviour
{


    public static int numKilledBats; //contador de bats matados

    public GameObject bat; // GameObject to instanciate
    public GameObject butterfly; //GameObject to instanciate

    public float minTimeBetweenBats = 1f, maxTimeBetweenBats = 10f;
    public float minTimeBetweenButterfly = 5f, maxTimeBetweenButterfly = 10f;
    public float minX = -1.0f, maxX = 1.0f;
    public float minZ = -2f, maxZ = 2f;
    public float minY = 0f, maxY = 1.0f;

    private GameObject panelInstructionsGame; //es el  panel con las instruccions del juego de los bats
    private GameObject panelSuccessfulGame; // es el  panel de que has ganado el juego de los bats
    private GameObject panelUnsuccessfulGame; //es el panel de que has perdido el juego de los bats

    private int _maxTimeGame = 30;

    [SerializeField]
    private AudioSource[] _audioBackground;

    private bool enableBats = true; //controla cuando parar de mostrar bats
    private int _goalKilledBats = 10;  //número de bats a matar para superar el juego

    private Text _countDownText;  // texto que muestra en el panel el contador de tìempo
    private float _countDownTimer; //cuenta atrás del juego
    private Text _numBatsText; //texto que muestra el número 



    public bool _startGame = false; //usada para controlar destrucción objetos y demás al hacer trackingLost;
    public bool _executeUpdate = false; //inicialmente no arranca el Update. Usado para controlarlo

    private string _categoryName = "VIDEOJUEGOS"; //lo usaré para mostrar la categoría de las preguntas en el texto del panel successful;

    private SuccessfulGamePanelManager _successfulManagerScript; //el script del panel de juego ganado. Para acceder a su texto



    // Use this for initialization
    void Start()
    {

        InitCounters();

    }



    private void OnEnable()
    {
        //Registro eventos principales
        Messenger.AddListener("StopMusic", StopMusic);
        Messenger.AddListener("StartGame", StartGame);


    }

    private void OnDisable()
    {
        //desactivo eventos
        Messenger.RemoveListener("StopMusic", StopMusic);
        Messenger.RemoveListener("StartGame", StartGame);

    }





    // Update is called once per frame
    void Update()
    {
        if (_executeUpdate)
        {

            if (_countDownText)
                //actualizo el texto de la cuenta atrás
                _countDownText.text = "Time: " + _countDownTimer.ToString("f0");
            if (_numBatsText)
                //actualizo el texto de los murciélagos matados
                _numBatsText.text = numKilledBats + " / " + _goalKilledBats.ToString();

            //chequeo en cada frame si se gana o pierde pues va por tiempo
            checkWinLoseGame();
        }
    }






    /* Método que inicializa los contadores del juego*/
    private void InitCounters()
    {
        numKilledBats = 0; //inicializo a 0 el contador de murciélagos matados
        _countDownTimer = _maxTimeGame; //inicializo contador de tiempo este minijuego a 20 seg


    }



    /*Método que reproduce la música de fondo*/
    private void PlayMusic()
    {
        //obtengo la música. Son dos músicas para hacer una mezcla entre la música en sí y los sonidos de bats
        _audioBackground = GetComponents<AudioSource>();

        for (int i = 0; i < _audioBackground.Length; i++)
        {
            GameManager.instance.playMusic(GameManager.instance.WithMusic, _audioBackground[i], 0);

        }
    }


    /*Método que para la música de fondo*/
    private void StopMusic()
    {
        foreach (AudioSource audio in _audioBackground)
        {

            GameManager.instance.playMusic(GameManager.instance.WithMusic, audio, 0);

        }
    }





    //Muestra los bats
    IEnumerator ShowRandomBat()
    {

        while (enableBats)
        {

            //instancio un bat
            GameObject _bat = (GameObject)Instantiate(bat);
            //le pongo nombre con el formato: Bat+nº de murciélago
            _bat.transform.name = _bat.transform.name.Replace("(Clone)", Bat.numBat.ToString());

            //asigno a su padre que será el imageTarget detectado
            _bat.transform.SetParent(GameManager.instance.trackedGameObject.transform);
            //asigno rotación para que el murciélago mire a la cámara
            _bat.transform.localEulerAngles = new Vector3(-90, 180, 0);
            //asigno posición aleatoria
            _bat.transform.localPosition = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));

            yield return new WaitForSeconds(Random.Range(minTimeBetweenBats, maxTimeBetweenBats));

        }
    }



    //Muestra las mariposas
    IEnumerator ShowRandomButterfly()
    {

        while (enableBats)
        {

            //instancio una mariposa
            GameObject _butterfly = (GameObject)Instantiate(butterfly);
            //le pongo nombre con el formato: Bat+nº de murciélago
            _butterfly.transform.name = _butterfly.transform.name.Replace("(Clone)", Butterfly.numButterfly.ToString());
            //asigno a su padre que es el imagetarget detectado
            _butterfly.transform.SetParent(GameManager.instance.trackedGameObject.transform);
            //asigno rotación para que la mariposa mire a la cámara
            _butterfly.transform.localEulerAngles = new Vector3(-90, 180, 0);
            //asigno posición aleatoria
            _butterfly.transform.localPosition = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));
            yield return new WaitForSeconds(Random.Range(minTimeBetweenButterfly, maxTimeBetweenButterfly));

        }

    }



    /* corrutina que ejecuta una cuenta atrás desde máximoTiempoGame hasta 0 de seg en seg */
    IEnumerator CountDownTimer()
    {

        while (_countDownTimer > 0.0f)
        {

            //cuenta atrás del tiempo
            _countDownTimer -= Time.deltaTime;

            yield return new WaitForSecondsRealtime(1 * Time.deltaTime);

        }
    }



    /*Método que chequea si se gana o pierde el juego de los murciélagos*/
    private void checkWinLoseGame()
    {
        //compruebo si he matado todos los murciélagos necesarios (juego ganado)

        if (numKilledBats >= _goalKilledBats)

        {

            _executeUpdate = false;  //paro el update
            //juego ganado
            //parar todas las corrutinas
            StopAllCoroutines();
            //DEstruyo los bats y butterfly que queden creadas por ahí, como hijos del imageTarget
            DestroyChildsImageTarget();
            //mostrar un panel felicitando por ello 
            ShowSuccessfulPanel();


        }

        //compruebo si el contador de tiempo acabó y no maté todos los murciélagos
        if (Mathf.Round(_countDownTimer) <= 0 && numKilledBats < _goalKilledBats)
        {
            _executeUpdate = false;
            //juego perdido
            //parar todas las corrutinas
            StopAllCoroutines();
            //Destruyo los bats y butterfly que queden creadas como hijos del imageTarget
            DestroyChildsImageTarget();
            //para la musica de fondo
            StopMusic();
            //inicializo contadors
            InitCounters();
            //paro el update
            _startGame = false;
            //mostrar un panel  juego perdido
            ShowUnsuccessfulPanel();

        }
    }



    /* Método que destruye los bats y/o butterfly que hayan quedado creados bajo el imageTarget cuando se para el juego
     * 
     */
    private void DestroyChildsImageTarget()
    {

        for (int i = 0; i < GameManager.instance.trackedGameObject.transform.childCount; i++)
        {
            Transform child = GameManager.instance.trackedGameObject.transform.GetChild(i);

            if (child.tag == "Bat" || child.tag == "Butterfly")
                Destroy(child.gameObject);
        }
    }






    /* Método que se ejecuta cuando se detecta la marca*/
    public void TargetDetected()
    {
        //obtengo el panel de instrucciones
        panelInstructionsGame = GameManager.instance.trackedGameObject.transform.Find("CanvasBatGame/BatGamePanelInstructions").gameObject;
        //obtengo el panel de que has conseguido ganar el juego para tenerlo cargado ya
        panelSuccessfulGame = GameManager.instance.trackedGameObject.transform.Find("CanvasBatGame/SuccessfulGamePanel").gameObject;
        //   panelSuccessfulGame = gameObject.transform.parent.transform.Find("CanvasBatGame/SuccessfulGamePanel").gameObject;

        //obtengo el panel de que has perdido el juego para que esté cargado
        panelUnsuccessfulGame = GameManager.instance.trackedGameObject.transform.Find("CanvasBatGame/UnsuccessfulGamePanel").gameObject;


        //hago visible el panel de instrucciones, por si se perdió el enfoque de la marca y se volvió, se tendrá que ejecutar
        if (panelInstructionsGame)
            panelInstructionsGame.SetActive(true);

        //obtengo los marcadores a mostrar en el UI
        GameObject parentMarks = gameObject.transform.parent.transform.Find("CanvasBatGame/ElementsUIBatGame").gameObject;
        Text[] marcadores = parentMarks.GetComponentsInChildren<Text>();
        if (marcadores[0].name == "CountDownTimer")
        {
            _countDownText = marcadores[0];
            _numBatsText = marcadores[1];
        }
        else
        {
            _countDownText = marcadores[1];
            _numBatsText = marcadores[0];
        }

        //inicializo los contadores del juego
        InitCounters();

        //anoto la categoría detectada. Está guardada en un tag del imageTarget
        GameManager.instance.currentCategory = new Category(gameObject.transform.parent.tag);

        //registro listeners
        //resta tiempo si mato mariposa
        Messenger<float>.AddListener("substractTime", SubstractTime);

    }




    /*Método que se ejecuta cuando se pierde la marca*/
    public void TargetLost()
    {
        InitCounters();
        //oculto el panel de éxito del juego por si estuviese activo en ese momento
        panelSuccessfulGame.SetActive(false);
        //oculto el panel de fracaso del juego por si estuviese activo en ese momento
        panelUnsuccessfulGame.SetActive(false);


        //desregistro los listeners
        Messenger<float>.RemoveListener("substractTime", SubstractTime);


        if (_startGame)
        {

            //paro todas las corutinas para parar el juego
            StopAllCoroutines();

            //destruyo bats y butterfly que hayan quedado creados bajo imageTarget
            DestroyChildsImageTarget();
            //para la musica de fondo
            StopMusic();
            //inicializo contadors
            InitCounters();
            //paro el update
            _startGame = false;
        }
    }



    /* Método que muestra un panel felicitando por el éxito del juego*/
    private void ShowSuccessfulPanel()
    {
        //activo el panel de que has ganado el juego de los bats
        if (panelSuccessfulGame == null)
            panelSuccessfulGame = GameManager.instance.trackedGameObject.transform.Find("CanvasBatGame/SuccessfulGamePanel").gameObject;
        panelSuccessfulGame.SetActive(true);
        //accedo a su script
        _successfulManagerScript = panelSuccessfulGame.GetComponent<SuccessfulGamePanelManager>();
        //cambio su texto
        _successfulManagerScript.message.text = "¡ERES UN CRACK! \n\n ¡ Lo has conseguido ! \n\n ¿Cuánto sabes de " + _categoryName + " ? \n\n Uhm...ahora lo sabré. \n\n ¡Ahí va la pregunta!";


    }


    /* Método que muestra un panel informando del fracaso del juego*/
    private void ShowUnsuccessfulPanel()
    {
        //activo el panel de que has perdido el juego de los bats
        panelUnsuccessfulGame.SetActive(true);

    }


    /*Método que se ejecuta cuando se lanza el evento startGame*/
    private void StartGame()
    {

        PlayMusic();
        _startGame = true;
        //   GameManager.instance.IsAlreadyStartedGame = true;
        _executeUpdate = true; //empieza el juego
        StartCoroutine(ShowRandomBat());
        StartCoroutine(ShowRandomButterfly());
        StartCoroutine(CountDownTimer());//cuenta atrás

    }


    /*Método que se ejecuta cuando se lanza el evento substractitme*/
    private void SubstractTime(float timeToSubstract)
    {
        _countDownTimer -= timeToSubstract;
    }
}
