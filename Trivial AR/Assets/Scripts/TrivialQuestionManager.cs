using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class TrivialQuestionManager : MonoBehaviour
{
    public string category;


    public GameObject questionImage;
    public GameObject Stretcher; //camilla que se muestra cuando la question no tiene image
    public GameObject playAudioButton; //botón que almacenará el sonido de las preguntas que incluyan audio y que permitirá volver a reproducirlo
    public Text questionText;
    public GameObject answerA;
    public GameObject answerB;
    public GameObject answerC;
    public GameObject answerD;
    //boton volver
    public GameObject buttonBackScene;
    //markers
    public Text scoreText;
    public Text countDownText;
    //answer texts
    private Text _answerA_Text;
    private Text _answerB_Text;
    private Text _answerC_Text;
    private Text _answerD_Text;
    //sounds
    private AudioSource _correctAnswerSound;
    private AudioSource _failedAnswerSound;
    private AudioSource[] _sounds;
    private AudioSource _musicBackground;
    private AudioSource _audioQuestionSource;
    //countDownTimer
    private Coroutine _countDownCoroutine;


    private bool wait; //para controlar el update
    private bool _isInTime; //para controlar el chequeo del tiempo en el update y pararlo
    private Question question;







    // Use this for initialization
    void Start()
    {


        //lo uso para controlar que se ejecute o no mi código en update
        wait = true;
        //para controlar si queda tiempo de responder la pregunta o no
        _isInTime = true;

        category = GameManager.instance.currentCategory.IdCategory;


        if (!String.IsNullOrEmpty(GameManager.instance.chosenVacune))
        {
            //el usuario ha elegido una categoría para conseguir una vacuna
            category = GameManager.instance.chosenVacune; //el usuario debe encontrar una pregunta para ganar una VACUNA. La categoría la ha elegido él de un panel de categorias

        }
        if (!string.IsNullOrEmpty(category))
            //obtengo una pregunta aleatoria de la categoría "category". Se guarda en el GameManager.Coincide con el idcategory de FirebaseDatabase
            FirebaseDatabaseManager.instance.GetRandomQuestionFromCategory(category);

        //obtengo los sonidos y la música
        _sounds = GetComponents<AudioSource>();
        //como unity recupera de top a down, sé que el primero es correct y el segundo incorrect
        _correctAnswerSound = _sounds[0]; //aparece el primero en el editor
        _failedAnswerSound = _sounds[1]; //aparece el segundo en el editor
        _musicBackground = _sounds[2]; //aparece el tercero en el editor
        //obtengo los textos de los botones (su referencia)
        _answerA_Text = answerA.GetComponentInChildren<Text>();
        _answerB_Text = answerB.GetComponentInChildren<Text>();
        _answerC_Text = answerC.GetComponentInChildren<Text>();
        _answerD_Text = answerD.GetComponentInChildren<Text>();

        //arranco la música de fondo, si se eligió con música en las settings
        GameManager.instance.playMusic(GameManager.instance.WithMusic, _musicBackground, 0);


    }





    // Update is called once per frame
    void Update()
    {
        //actualizo marcadores de tiempo y puntos
        //actualizar el score en pantalla
        scoreText.text = "SCORE: " + GameManager.instance.Score.ToString();
        //actualizar el contador de tiempo
        countDownText.text = "TIME: " + GameManager.instance.CountDownTimer.ToString("f0");
        //chequeo si se acabó el tiempo de responder
        if (_isInTime)
            CheckEndedTime();

        if (wait)  //por si tarda en cargar la pregunta de Firebase
            if (GameManager.instance.CurrentQuestion != null)
            {
                //recojo la pregunta obtenida aleatoriamente
                question = GameManager.instance.CurrentQuestion;
                if (question.IsImageQuestion)
                {
                    //la pregunta lleva una imagen

                    //desactivo el botón de play audio para cuestiones con audio
                    playAudioButton.SetActive(false);
                    //activo el objeto que contendrá la imagen
                    questionImage.SetActive(true);
                    //desactivo la imagen que se ve cuando no hay imagen en la pregunta
                    Stretcher.SetActive(false);
                    //start coroutine to download image de Firebase Storage a partir de la url
                    StartCoroutine(AccessURLImage(question.QuestionImageUrl));
                }
                else
                {
                    //la pregunta no lleva una imagen

                    //desactivo el objeto que contendrá la imagen
                    questionImage.SetActive(false);
                    //activo la imagen que se ve como adorno
                    Stretcher.SetActive(true);

                    if (question.IsAudioQuestion)
                    {
                        //la pregunta lleva audio

                        //recupero el AudioSource del botón play para almacenarle el audio Clip en él
                        _audioQuestionSource = playAudioButton.GetComponent<AudioSource>();

                        //activo el play button
                        playAudioButton.SetActive(true);

                        //start coroutine to download audio de Firebase Storage a partir de la url
                        StartCoroutine(AccessUrlAudio(question.QuestionAudioUrl));
                    }
                }
                //relleno campos
                questionText.text = question.QuestionText;
                _answerA_Text.text = question.ChoiceA;
                _answerB_Text.text = question.ChoiceB;
                _answerC_Text.text = question.ChoiceC;
                _answerD_Text.text = question.ChoiceD;

                //arranco el contador de tiempo
                _countDownCoroutine = StartCoroutine(CountDownTimer());
                //paro el update
                wait = false;
                //reseteo la pregutna guardada en el GameManager
                GameManager.instance.CurrentQuestion = null;



            }

    }





    /*Método que comprueba si se acabó el tiempo sin responder la pregunta*/
    private void CheckEndedTime()
    {
        if (Mathf.Round(GameManager.instance.CountDownTimer) <= 0)
        {
            //se acabó el tiempo; respuesta fallada
            _isInTime = false;
            //reinicio el tiempo
            GameManager.instance.RestartCountDownTimer();
            NotAnswered();
        }
    }


    // Method is called when BackButton is clicked
    public void OnClickBackButton()
    {

        SceneManager.LoadScene("MainScene");
    }



    /*Corrutina que descarga de Firebase Storage una imagen partiendo de su url y la almacena en un UI Image*/
    IEnumerator AccessURLImage(string url)
    {
        //acceso a internet a esa url
        using (WWW www = new WWW(url))
        {
            //wait for download to complete
            yield return www;

            //creo una textura basada en la textura descargada y guardada en www.texture
            Texture2D texture = new Texture2D(www.texture.width, www.texture.height, TextureFormat.DXT1, false);
            //cargo la imagen descargada en la textura que he creado
            www.LoadImageIntoTexture(texture);
            //obtengo mi objeto UI Image (donde quiero guardar la imagen)
            Image image = questionImage.GetComponent<Image>();
            //asigno al sprite del UI Image un sprite que creo basado en la textura de antes.
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);

            Debug.Log("Texture URL: " + www.url);
        }
    }



    /*Corrutina que descarga de Firebase Storage un audio partiendo de su url y la almacena en un UI Image*/
    IEnumerator AccessUrlAudio(string url)
    {
        //acceso a internet a esa url
        using (WWW www = new WWW(url))
        {
            //wait for download to complete
            yield return www;

            if (_audioQuestionSource != null)
            {
                _audioQuestionSource.clip = www.GetAudioClip(false, false, AudioType.MPEG); //recupero el clip que he descargado de Firebase

                //reproduzco el sonido
                if (GameManager.instance.WithSound)

                    PlayDownloadedSound(_audioQuestionSource);

            }
        }
    }




    /* Método que compara el idAnswer pasado con el almacenado como correcta en la question.
     * Ej: ChoiceA  con ChoiceB
     * */
    public bool isCorrectAnswer(string idAnswer)
    {
        if (idAnswer.Equals(question.CorrectAnswer))
        {
            return true;
        }
        return false;
    }



    /*Método que se ejecuta cada vez que se pulsa un botón de respuesta: A,B,C ó D*/
    public void onClickAnswer()
    {
        //paro el contador de tiempo
        StopCoroutine(_countDownCoroutine);


        bool isCorrect; //indica si la respuesta elegida es la correcta

        string buttonClickedName = EventSystem.current.currentSelectedGameObject.name; //name del botón clickado



        isCorrect = isCorrectAnswer(buttonClickedName); //averiguo si acerté o no

        if (isCorrect) //acertaste
        {
            //reproducir sonido ok
            if (_correctAnswerSound)
                if (GameManager.instance.WithSound)

                    _correctAnswerSound.Play();

            //cambio color del fondo del botón a verde para indicar OK
            EventSystem.current.currentSelectedGameObject.GetComponent<Image>().color = Color.green;

            //incrementar nº de preguntas respondidas (bien o mal)
            GameManager.instance.TotalQuestions++;
            print("TotalQuestions=" + GameManager.instance.TotalQuestions);
            //Desactivo los botones, para que no funcionen y se puedan volver a tocar
            DeactivatedButtons();
            //actualizo todas las puntuaciones del juego
            UpdateAllScores("OK");
            //compruebo si era una pregunta para conseguir vacuna
            if (!String.IsNullOrEmpty(GameManager.instance.chosenVacune))
            {
                //actualizo la vacunas conseguida en GameManager
                UpdateVacune(GameManager.instance.chosenVacune);

                //actualizo la vacuna conseguida en Firebase
                FirebaseDatabaseManager.instance.UpdateVacunaToFirebase(GameManager.instance.chosenVacune);
            }

        }
        else  //fallaste
        {
            FailedAnswer();
        }

        //Actualizo en la Bd de Firebase las puntuaciones
        FirebaseDatabaseManager.instance.UpdateScoreToFirebase(GameManager.instance.currentCategory, GameManager.instance.CurrentUser);


        //activo el botón de volver a la escena anterior
        if (buttonBackScene != null)
            buttonBackScene.SetActive(true);
    }






    /*Método que se ejecuta cuando se responde mal una pregunta*/
    private void FailedAnswer()
    {
        //respuesta incorrecta

        //reproduzco sonido error
        if (_failedAnswerSound)
            if (GameManager.instance.WithSound)

                _failedAnswerSound.Play();
        //pongo fondo rojo
        EventSystem.current.currentSelectedGameObject.GetComponent<Image>().color = Color.red;
        //pongo en verde la correcta
        Button correctButton = GameObject.Find(question.CorrectAnswer).GetComponent<Button>();
        correctButton.GetComponent<Image>().color = Color.green;
        //Desactivo los botones, para que no funcionen y se puedan volver a tocar
        DeactivatedButtons();
        //incrementar nº de preguntas respondidas (bien o mal)
        GameManager.instance.TotalQuestions++;
        print("TotalQuestions=" + GameManager.instance.TotalQuestions);
        //actualizo puntuaciones
        //actualizo todas las puntuaciones del juego
        UpdateAllScores("FAIL");

    }



    /*Método que se ejecuta cuando NO se responde a una pregunta (se acaba el tiempo)*/
    private void NotAnswered()
    {
        //no has respondido

        //reproduzco sonido error
        if (_failedAnswerSound)
            if (GameManager.instance.WithSound)

                _failedAnswerSound.Play();

        //pongo en verde la correcta
        Button correctButton = GameObject.Find(question.CorrectAnswer).GetComponent<Button>();
        correctButton.GetComponent<Image>().color = Color.green;
        //Desactivo los botones, para que no funcionen y se puedan volver a tocar
        DeactivatedButtons();
        //incrementar nº de preguntas respondidas (bien o mal)
        GameManager.instance.TotalQuestions++;
        print("TotalQuestions=" + GameManager.instance.TotalQuestions);
        //actualizo puntuaciones
        //actualizo todas las puntuaciones del juego
        UpdateAllScores("FAIL");

        //Actualizo en la Bd de Firebase las puntuaciones
        FirebaseDatabaseManager.instance.UpdateScoreToFirebase(GameManager.instance.currentCategory, GameManager.instance.CurrentUser);



        //reinicio isInTime
        _isInTime = true;

        //activo el botón de volver a la escena anterior
        if (buttonBackScene != null)
            buttonBackScene.SetActive(true);

    }






    private void DeactivatedButtons()
    {
        answerA.GetComponent<Button>().interactable = false;
        answerB.GetComponent<Button>().interactable = false;
        answerC.GetComponent<Button>().interactable = false;
        answerD.GetComponent<Button>().interactable = false;
    }


    /*Método que actualiza todas las puntuaciones. Tanto si se acierta como si se falla*/
    public void UpdateAllScores(string answerResult)
    {
        if (answerResult == "OK")
        {   //ACERTADA LA RESPUESTA

            //sumo puntos por acertar
            GameManager.instance.Score += GameManager.instance.ScoreOkQuestion;
            print("Score=" + GameManager.instance.Score);
            //incrementar nº de preguntas correctas
            GameManager.instance.TotalOkQuestions++;
            print("TotalOkQuestions=" + GameManager.instance.TotalOkQuestions);
            //incrementar nº de preguntas correctas seguidas si es que lo es
            GameManager.instance.UpdateTotalOkConsecutiveQuestions();
            print("TotalOkConsecutiveQuestions=" + GameManager.instance.TotalOkConsecutiveQuestions);
            //incrementar total preguntas de esta categoria
            GameManager.instance.IncTotalCategoryQuestions(category, 1);
            //incrementar nº de preguntas de esta categoria correctas
            GameManager.instance.IncTotalOkCategoryQuestions(category, 1);
            ///////////////////////////////////////////////////////////////////////////////////
            GameManager.instance.CurrentOkAnswersToVacune++; //incremento el contador de respuestas para conseguir la vacuna



        }
        else
        {
            //respuesta fallada

            //resto puntos por fallar
            GameManager.instance.Score += GameManager.instance.ScoreFailedQuestion;
            print("Score=" + GameManager.instance.Score);
            //reinicio a 0 el contador temporal de respuestas ok seguidas, pues ha habido un fallo y ya no serán seguidas
            GameManager.instance.TempOkConsecutiveQuestions = 0;
            print("TotalOkConsecutiveQuestions=" + GameManager.instance.TotalOkConsecutiveQuestions);
            //incrementar total preguntas de esta categoria
            GameManager.instance.IncTotalCategoryQuestions(category, 1);
        }
    }




    /*Método que actualiza la vacuna conseguida en memoria (GameManager)*/
    public void UpdateVacune(string vacune)
    {
        if (!String.IsNullOrEmpty(vacune))
            GameManager.instance.UserVacunes.Add(vacune);
    }




    /* Método que se ejecuta cuando se pulsa el botón de reproducir el sonido de la pregunta de trivial que lleva audio*/
    public void OnPlayAudioButtonClick()
    {
        PlayDownloadedSound(_audioQuestionSource);
    }


    /* Método que reproduce el audioSource pasado (QUE VIENE DE DESCARGARLO DE INTERNET), comprobando previamente si está listo para ello*/
    private void PlayDownloadedSound(AudioSource audio)
    {
        if (audio != null)
            if (!audio.isPlaying)
                if (audio.clip.loadState == AudioDataLoadState.Loaded)
                    audio.Play();
    }



    /* corrutina que ejecuta una cuenta atrás desde máximoTiempoGame hasta 0 de seg en seg */
    IEnumerator CountDownTimer()
    {
        while (GameManager.instance.CountDownTimer > 0.0f)
        {

            //cuenta atrás del tiempo
            GameManager.instance.CountDownTimer -= Time.deltaTime;

            yield return new WaitForSecondsRealtime(1 * Time.deltaTime);
        }
    }






}
