using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


public class FirebaseDatabaseManager : MonoBehaviour
{



    const int kMaxLogSize = 16382;  //max linea del log
    private string logText = "";  //texto para mostrar en DebugLog



    private DatabaseReference CategoriesReference;



    public static FirebaseDatabaseManager instance = null;

    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(this);

    }


    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    private void Start()
    {

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //Firebase is ready
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                    "Could not resolve all Firebase dependencies: " + dependencyStatus);
                //Firebase Unity SDK is not safe to use here.
            }
        });
    }




    protected virtual void InitializeFirebase()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;
        // It's necessary in order for the database connection to work correctly in editor.
        // Set up the Editor before calling into the realtime database.
        app.SetEditorDatabaseUrl("https://trivial-ar.firebaseio.com/");
        if (app.Options.DatabaseUrl != null) app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);
        //obtengo referencias a las colecciones de la BD
        //  UsersReference = FirebaseDatabase.DefaultInstance.GetReference("Users");
        CategoriesReference = FirebaseDatabase.DefaultInstance.GetReference("categories");

    }





    


    private void Update()
    {
        //Exit if escape (or back, on mobile) is pressed.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    


    // Output text to the debug log text field, as well as the console.
    public void DebugLog(string s)
    {
        Debug.Log(s);
        logText += s + "\n";

        while (logText.Length > kMaxLogSize)
        {
            int index = logText.IndexOf("\n");
            logText = logText.Substring(index + 1);
        }


    }





    ///////////////////////////////////////////////////////////////////////////////////
    ////            CATEGORIES  AND  QUESTIONS ///////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////


    //Este método recibe un id de Category, lo busca en categoryQuestions en Firebase y guarda la información en un object
    //de tipo Category que se le pasa a la función callbackFunction.
    //Por tanto, para recuperarlo, debo programar ese método cuando llame a GetCategory
    private void GetCategoryFromFirebase(string idCategory, Action<Category> callbackFunction)
    {
        DatabaseReference CategoryQuestions = FirebaseDatabase.DefaultInstance.GetReference("es").Child("categoryQuestions");
        CategoryQuestions.Child(idCategory).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                DebugLog(task.Exception.Message);
            }
            else if (task.IsCompleted)
            {
                Category category = new Category(idCategory);//creo la categoria

                //leo los datos de la categoría
                DataSnapshot questions = task.Result;
                foreach (DataSnapshot question in questions.Children)
                {

                    if (question.Key.Equals("questionsTotalCat"))
                    {
                        int idQuestionNumber;
                        DebugLog("total de preguntas es " + question.Value);
                        if (Int32.TryParse(question.Value.ToString(), out idQuestionNumber))
                            category.QuestionsTotalCat = idQuestionNumber;
                    }
                    else
                    {
                        //es un id de pregunta
                        category.AddQuestionId(question.Key);
                    }
                }
                foreach (string question in category.QuestionsList)
                {
                    DebugLog("Pregunta nº " + question);
                }

                //la almaceno en GameManager esa categoría
                GameManager.instance.currentCategory = category;

                callbackFunction(category);

            }
        });
    }


    //MÉTODO A LLAMAR DESDE UN BOTÓN O SIMILAR
    //Método que obtiene una pregunta aleatoria de una categoría a partir de su id.
    //Hace una llamada al método anterior que se gestiona con un Callback
    public Question GetRandomQuestionFromCategory(string idCategory)
    {
        //obtengo los datos de la categoría y sus preguntas de firebase
        GetCategoryFromFirebase(idCategory, GetQuestionFromCategoryFromFirebase); //GetQuestionFromCategoryFromFirebase es un callback que será llamado cuando se tenga la categoria
        return null;

    }



    /* Método callback que es llamado desde GetCategoryFromFirebase cuando se recibe la categoría de Firebase*/
    private void GetQuestionFromCategoryFromFirebase(Category category)
    { //en category tengo toda las preguntas de la categoría y cuántas hay

        System.Random rnd = new System.Random();
        int posAleatoria = rnd.Next(0, category.QuestionsTotalCat);
        string idQuestion = category.QuestionsList[posAleatoria];
        //obtengo los datos de Firebase de la pregunta aleatoria con id idQuestion
        GetQuestionFromFirebase(idQuestion, IsReadyQuestion);

    }



    /* Método que obtiene asíncronamente los datos de una pregunta de Firebase cuyo idQuestion se le pasa.
     * Cuando los tiene listos, llama a un callback con la información obtenida de esa Question*/
    private void GetQuestionFromFirebase(string idQuestion, Action<Question> callbackFunction)
    {
        DatabaseReference Question = FirebaseDatabase.DefaultInstance.GetReference("es").Child("questions");
        Question.Child(idQuestion).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                DebugLog(task.Exception.Message);
            }
            else if (task.IsCompleted)
            {
                Question question = null;
                //leo los datos de la pregunta
                DataSnapshot questionDetails = task.Result;

                //es la info de la pregunta
                question = new Question(idQuestion, questionDetails.Child("Category").Value.ToString(),
                                                    questionDetails.Child("QuestionText").Value.ToString(),
                                                    questionDetails.Child("ChoiceA").Value.ToString(),
                                                    questionDetails.Child("ChoiceB").Value.ToString(),
                                                    questionDetails.Child("ChoiceC").Value.ToString(),
                                                    questionDetails.Child("ChoiceD").Value.ToString(),
                                                    questionDetails.Child("CorrectAnswer").Value.ToString(),
                                                    (bool)questionDetails.Child("IsAudioQuestion").Value,
                                                    (bool)questionDetails.Child("IsImageQuestion").Value);
                if (question.IsImageQuestion)
                    question.QuestionImageUrl = questionDetails.Child("QuestionImageUrl").Value.ToString();
                if (question.IsAudioQuestion)
                    question.QuestionAudioUrl = questionDetails.Child("QuestionAudioUrl").Value.ToString();



                callbackFunction(question);

            }
        });
    }


    /* Método callback que es llamado desde GetQuestionFromFirebase cuando se recibe la question de Firebase*/
    private void IsReadyQuestion(Question question)
    { //en question tengo toda la información de la pregunta

        GameManager.instance.CurrentQuestion = question;
        DebugLog(question.ToString());

    }





    ///////////////////////////////////////////////////////////////////////////////////
    ////            SCORES                     ///////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////

    /* Método que actualiza o sube a Firebase Database todas las puntuaciones del actual usuario */
    public void UpdateScoreToFirebase(Category category, User user)
    {
        DatabaseReference userScore = FirebaseDatabase.DefaultInstance.GetReference("es").Child("userScore");
        DatabaseReference categoryScore = FirebaseDatabase.DefaultInstance.GetReference("es").Child("categoryScore");

        UserScore userScoreToSave = new UserScore(user.UserIdAuth, user.Email, GameManager.instance.Score, GameManager.instance.CurrentLevel, GameManager.instance.TotalWonGames, GameManager.instance.TotalQuestions,
                                                        GameManager.instance.TotalOkQuestions, GameManager.instance.TotalOkConsecutiveQuestions);
        string jsonUserScoreToSave = JsonUtility.ToJson(userScoreToSave); //lo serializo a json

        userScore.Child(user.UserIdAuth).SetRawJsonValueAsync(jsonUserScoreToSave).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                DebugLog(task.Exception.Message);
            }
            else if (task.IsCompleted)
            {
                DebugLog("userScore almacenado");
            }
        });

        if (category != null)
        {
            CategoryScore categoryScoreToSave = new CategoryScore(user.UserIdAuth, user.Email,
                                String.Format("{0}_{1}", user.UserIdAuth, category.IdCategory), category.Name, category.IdCategory, GameManager.instance.GetTotalCategoryQuestions(category.IdCategory),
                                GameManager.instance.GetTotalOkCategoryQuestions(category.IdCategory));
            string jsonCategoryScoreToSave = JsonUtility.ToJson(categoryScoreToSave); //lo serializo a json
            categoryScore.Child(String.Format("{0}_{1}", user.UserIdAuth, category.IdCategory)).SetRawJsonValueAsync(jsonCategoryScoreToSave).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    DebugLog(task.Exception.Message);
                }
                else if (task.IsCompleted)
                {
                    DebugLog("categoryScore almacenado");
                }
            });
        }
    }



    /* Método que obtiene del FirebaseDatabase todas las puntuaciones de este usuario: por categorías, totales,...*/
    public void GetScoreFromFirebase(User currentUser)
    {
        DatabaseReference userScoreReference = FirebaseDatabase.DefaultInstance.GetReference("es").Child("userScore");
        DatabaseReference categoryScoreReference = FirebaseDatabase.DefaultInstance.GetReference("es").Child("categoryScore");

        //puntuaciones generales
        userScoreReference.Child(currentUser.UserIdAuth).GetValueAsync().ContinueWith(task =>
        {

            if (task.IsFaulted)
            {
                DebugLog(task.Exception.Message);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot data = task.Result;
                UserScore userScore = JsonUtility.FromJson<UserScore>(data.GetRawJsonValue());
                //guardo el usuario en GameManager
                GameManager.instance.CurrentUser = new User(userScore);
                //guardo las puntuaciones del usuario generales en GameManager
                GameManager.instance.SetUserMainScores(userScore);

            }
        });
        //Puntuaciones por categorias
        categoryScoreReference.OrderByChild("UserIdAuth").EqualTo(currentUser.UserIdAuth).GetValueAsync().ContinueWith(task =>
        {

            if (task.IsFaulted)
            {
                DebugLog(task.Exception.Message);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot data = task.Result;
                //recorro cada categoria con puntos de ese usuario
                foreach (DataSnapshot categoryScoreData in data.Children)
                {
                    CategoryScore categoryScore = JsonUtility.FromJson<CategoryScore>(categoryScoreData.GetRawJsonValue());
                    //añado las puntuaciones de esa categoría al GameManager
                    GameManager.instance.SetTotalCategoryQuestions(categoryScore.CategoryId, categoryScore.TotalCategoryQuestions);
                    GameManager.instance.SetTotalOkCategoryQuestions(categoryScore.CategoryId, categoryScore.TotalOkCategoryQuestions);
                }


            }
        });

    }



    /* Método que actualiza o sube a Firebase Database el nº de juegos ganados del actual usuario */
    public void UpdateWonGamesToFirebase(User user)
    {
        DatabaseReference userScore = FirebaseDatabase.DefaultInstance.GetReference("es").Child("userScore").Child(user.UserIdAuth);

        userScore.Child("TotalWonGames").SetValueAsync(GameManager.instance.TotalWonGames).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                DebugLog(task.Exception.Message);
            }
            else if (task.IsCompleted)
            {
                DebugLog("totalWonGames almacenado");
            }
        });
    }



    /* Método que obtiene del FirebaseDatabase el ranking: nombreUsuario y score de todos los usuarios*/
    public void GetRankingFromFirebase(Action<Dictionary<string, int>> callbackFunction)
    {
        //almacenaré el ranking en este dictionary
        Dictionary<string, int> rankingDictionary = new Dictionary<string, int>();

        DatabaseReference userScoreReference = FirebaseDatabase.DefaultInstance.GetReference("es").Child("userScore");
        //obtengo las 100 mejores puntuaciones. Tengo que usar LimitToLast porque ordena de menos a más, con lo cual
        //cojo las 100 últimas para que sean las mayores.
        userScoreReference.OrderByChild("Score").LimitToLast(100).GetValueAsync().ContinueWith(task =>
        {

            if (task.IsFaulted)
            {
                DebugLog(task.Exception.Message);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot data = task.Result;
                //recorro cada usuario para coger sus datos
                foreach (DataSnapshot user in data.Children)
                {
                    rankingDictionary.Add(user.Child("UserEmail").Value.ToString(), int.Parse(user.Child("Score").Value.ToString()));

                }
                //aquí el dictionary contiene el ranking completo pero ordenado de menos valor a más
                callbackFunction(rankingDictionary);

            }
        });

    }






    ///////////////////////////////////////////////////////////////////////////////////
    ////            VACUNES                     ///////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////


    /* Método que obtiene del FirebaseDatabase todas las vacunas de este usuario: los quesitos de trivial*/
    public void GetVacuneFromFirebase(User currentUser)
    {
        DatabaseReference vacuneReference = FirebaseDatabase.DefaultInstance.GetReference("es").Child("vacunes");


        vacuneReference.Child(currentUser.UserIdAuth).GetValueAsync().ContinueWith(task =>
        {

            if (task.IsFaulted)
            {
                DebugLog(task.Exception.Message);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot vacunes = task.Result;

                foreach (DataSnapshot vacune in vacunes.Children)
                {

                    //guardo esa vacuna del usuario al GameManager
                    GameManager.instance.UserVacunes.Add(vacune.Key);

                }

            }
        });
    }






    /* Método que actualiza o sube a Firebase Database una vacuna del actual usuario */
    public void UpdateVacunaToFirebase(string vacune)
    {
        DatabaseReference vacunesReference = FirebaseDatabase.DefaultInstance.GetReference("es").Child("vacunes").Child(GameManager.instance.CurrentUser.UserIdAuth);

        vacunesReference.Child(vacune).SetValueAsync(true).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                DebugLog(task.Exception.Message);
            }
            else if (task.IsCompleted)
            {
                DebugLog("vacuna almacenado");
            }
        });
    }



    /* Método que borra todas las vacunas de este usuario: los quesitos de trivial*/
    public void DeleteVacuneFromFirebase(User currentUser)
    {
        DatabaseReference vacuneReference = FirebaseDatabase.DefaultInstance.GetReference("es").Child("vacunes");


        vacuneReference.Child(currentUser.UserIdAuth).RemoveValueAsync().ContinueWith(task =>
        {

            if (task.IsFaulted)
            {
                DebugLog(task.Exception.Message);
            }
            else if (task.IsCompleted)
            {
                DebugLog("vacunas borradas");

            }
        });

    }







}