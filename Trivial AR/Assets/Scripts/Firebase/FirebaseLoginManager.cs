using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Vuforia;


// Handler for UI buttons on the scene.  Also performs some
// necessary setup (initializing the firebase app, etc) on
// startup.
public class FirebaseLoginManager : MonoBehaviour
{

    AuthUI authUI; //clase que gestiona el UI de login y registro de usuarios (es propia)


    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;

    protected Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth =
      new Dictionary<string, Firebase.Auth.FirebaseUser>();


    private string logText = "";
    protected string email = "";
    protected string password = "";
    protected string confirmPassword = "";
    protected string displayName = "";



    // Whether to sign in / link or reauthentication *and* fetch user profile data.
    protected bool signInAndFetchProfile = false;
    // Flag set when a token is being fetched.  This is used to avoid printing the token
    // in IdTokenChanged() when the user presses the get token button.
    private bool fetchingToken = false;
    private bool isCheckDependenciesOK;
    private bool isInitializedFirebase = false;



    bool UIEnabled = true;

    const int kMaxLogSize = 16382;


    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

    public static FirebaseLoginManager instance = null;





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
    public virtual void Start()
    {

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {

                isCheckDependenciesOK = true;

            }
            else
            {
                isCheckDependenciesOK = false;
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }






    // Handle initialization of the necessary firebase modules:
    protected void InitializeFirebase()
    {
        DebugLog("Setting up Firebase Auth");
        authUI = GetComponent<AuthUI>();
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;


        AuthStateChanged(this, null);
    }




    // Exit if escape (or back, on mobile) is pressed.
    protected virtual void Update()
    {
        if (isCheckDependenciesOK && !isInitializedFirebase)
        {
            InitializeFirebase();
            isInitializedFirebase = true;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }


    // Method is called when REGISTER BUTTON is clicked in order to create a new user
    // in Firebase Auth
    //Create a user with the email and password and sign in in app
    public void CreateUserWithEmailAsync()
    {

        email = authUI.registerEmailInput.text;
        password = authUI.registerPasswordInput.text;
        confirmPassword = authUI.registerConfirmPasswordInput.text;

        if (!String.IsNullOrEmpty(password) && !String.IsNullOrEmpty(confirmPassword))

            if (!String.Equals(password, confirmPassword))
            {
                authUI.registerLogText.text = "Las contraseñas no coinciden";
                return;
            }


        if ((String.IsNullOrEmpty(password) && !String.IsNullOrEmpty(confirmPassword)) ||
            (!String.IsNullOrEmpty(password) && String.IsNullOrEmpty(confirmPassword)))
        {
            authUI.registerLogText.text = "Las contraseñas no coinciden";
            return;
        }

        authUI.progressBar.SetActive(true); //hago visible la barra de progreso
        DebugLog(String.Format("Attempting to create user {0}...", email));
        DisableUI();

        // This passes the current displayName through to HandleCreateUserAsync
        // so that it can be passed to UpdateUserProfile().  displayName will be
        // reset by AuthStateChanged() when the new user is created and signed in.
        string newDisplayName = displayName;
        auth.CreateUserWithEmailAndPasswordAsync(email, password)
          .ContinueWith((task) =>
          {
              authUI.progressBar.SetActive(false);
              EnableUI();
              if (LogTaskCompletion(task, "Creación de usuario", authUI.registerLogText))
              {
                  if (auth.CurrentUser != null)
                  {
                      //obtengo los datos del usuario creado.
                      var user = task.Result;
                      DisplayDetailedUserInfo(user, 1);

                      authUI.ShowLoginPanel();
                  }
              }
              //Nothing to update, so just return a completed Task
              return task;

          });
    }





    // Sign-in with an email and password.
    public void SigninWithEmailAsync()
    {

        email = authUI.loginEmailInput.text;
        password = authUI.loginPasswordInput.text;

        DebugLog(String.Format("Attempting to sign in as {0}...", email));
        authUI.progressBar.SetActive(true); //hago visible la barra de progreso
        DisableUI();

        auth.SignInWithEmailAndPasswordAsync(email, password)
                  .ContinueWith(HandleSignInWithUser);
    }




    // Called when a sign-in without fetching profile data completes.
    void HandleSignInWithUser(Task<Firebase.Auth.FirebaseUser> task)
    {
        EnableUI();

        if (LogTaskCompletion(task, "Login ", authUI.loginLogText))
        {
            authUI.progressBar.SetActive(false);//desactivo barra progreso
            Firebase.Auth.FirebaseUser newUser = task.Result;

            DebugLog(String.Format("{0} - {1} signed in", newUser.DisplayName, newUser.UserId));
            //load scene mainscene for database
            Debug.Log("usuario logueado" + auth.CurrentUser.Email);

            //almaceno el usuario
            GameManager.instance.CurrentUser = new User(newUser.Email, "", newUser.UserId);

            //cargo las puntuaciones de este usuario de la BD FirebaseDatabase
            FirebaseDatabaseManager.instance.GetScoreFromFirebase(GameManager.instance.CurrentUser);

            //cargo las vacunas del usuario de la BD Firebase (son los quesitos)
            FirebaseDatabaseManager.instance.GetVacuneFromFirebase(GameManager.instance.CurrentUser);


            authUI.LoadLoggedInScene();
            //guardo en prefs
            PlayerPrefs.SetString("emailCurrentUser", newUser.Email);
            PlayerPrefs.SetString("userIdCurrentUser", newUser.UserId);
            PlayerPrefs.Save();

            Debug.Log("usuario añadido a gamemanager: " + GameManager.instance.CurrentUser.Email);

        }
    }









    void OnDestroy()
    {
        if (auth != null)
            auth.StateChanged -= AuthStateChanged;


    }

    void DisableUI()
    {
        UIEnabled = false;
    }





    void EnableUI()
    {
        UIEnabled = true;
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




    // Display additional user profile information.
    protected void DisplayProfile<T>(IDictionary<T, object> profile, int indentLevel)
    {
        string indent = new String(' ', indentLevel * 2);
        foreach (var kv in profile)
        {
            var valueDictionary = kv.Value as IDictionary<object, object>;
            if (valueDictionary != null)
            {
                DebugLog(String.Format("{0}{1}:", indent, kv.Key));
                DisplayProfile<object>(valueDictionary, indentLevel + 1);
            }
            else
            {
                DebugLog(String.Format("{0}{1}: {2}", indent, kv.Key, kv.Value));
            }
        }
    }

    // Display user information reported
    protected void DisplaySignInResult(Firebase.Auth.SignInResult result, int indentLevel)
    {
        string indent = new String(' ', indentLevel * 2);
        DisplayDetailedUserInfo(result.User, indentLevel);
        var metadata = result.Meta;
        if (metadata != null)
        {
            DebugLog(String.Format("{0}Created: {1}", indent, metadata.CreationTimestamp));
            DebugLog(String.Format("{0}Last Sign-in: {1}", indent, metadata.LastSignInTimestamp));
        }
        var info = result.Info;
        if (info != null)
        {
            DebugLog(String.Format("{0}Additional User Info:", indent));
            DebugLog(String.Format("{0}  User Name: {1}", indent, info.UserName));
            DebugLog(String.Format("{0}  Provider ID: {1}", indent, info.ProviderId));
            DisplayProfile<string>(info.Profile, indentLevel + 1);
        }
    }

    // Display user information.
    protected void DisplayUserInfo(Firebase.Auth.IUserInfo userInfo, int indentLevel)
    {
        string indent = new String(' ', indentLevel * 2);
        var userProperties = new Dictionary<string, string> {
      {"Display Name", userInfo.DisplayName},
      {"Email", userInfo.Email},
      {"Photo URL", userInfo.PhotoUrl != null ? userInfo.PhotoUrl.ToString() : null},
      {"Provider ID", userInfo.ProviderId},
      {"User ID", userInfo.UserId}
    };
        foreach (var property in userProperties)
        {
            if (!String.IsNullOrEmpty(property.Value))
            {
                DebugLog(String.Format("{0}{1}: {2}", indent, property.Key, property.Value));
            }
        }
    }

    // Display a more detailed view of a FirebaseUser.
    protected void DisplayDetailedUserInfo(Firebase.Auth.FirebaseUser user, int indentLevel)
    {
        string indent = new String(' ', indentLevel * 2);
        DisplayUserInfo(user, indentLevel);
        DebugLog(String.Format("{0}Anonymous: {1}", indent, user.IsAnonymous));
        DebugLog(String.Format("{0}Email Verified: {1}", indent, user.IsEmailVerified));
        DebugLog(String.Format("{0}Phone Number: {1}", indent, user.PhoneNumber));
        var providerDataList = new List<Firebase.Auth.IUserInfo>(user.ProviderData);
        var numberOfProviders = providerDataList.Count;
        if (numberOfProviders > 0)
        {
            for (int i = 0; i < numberOfProviders; ++i)
            {
                DebugLog(String.Format("{0}Provider Data: {1}", indent, i));
                DisplayUserInfo(providerDataList[i], indentLevel + 2);
            }
        }
    }

    // Track state changes of the auth object.
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {


        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                DebugLog("Signed out " + user.UserId);

            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                DebugLog("Signed in " + user.UserId);

                if (signedIn)
                {
                    DebugLog("Signed in " + user.UserId);

                }
            }

        }
    }








    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    protected bool LogTaskCompletion(Task task, string operation, Text inputText)
    {
        authUI.progressBar.SetActive(false);//oculto la barra de progreso
        bool complete = false;
        if (task.IsCanceled)
        {

            DebugLog(operation + " canceled.");
            inputText.text = operation + " cancelada";

        }
        else if (task.IsFaulted)
        {
            DebugLog(operation + " encounted an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
            {
                string authErrorCode = "";
                Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                if (firebaseEx != null)
                {
                    authErrorCode = String.Format("AuthError.{0}: ",
                      ((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString());
                }
                DebugLog(authErrorCode + exception.ToString());
                inputText.text = GetErrorMessage((Firebase.Auth.AuthError)firebaseEx.ErrorCode);

            }
        }
        else if (task.IsCompleted)
        {
            DebugLog(operation + " completed");
            inputText.text = operation + " completada";


            complete = true;
        }
        return complete;
    }




    //Method  returns the message for one Firebase error code  
    protected string GetErrorMessage(Firebase.Auth.AuthError errorCode)
    {
        var message = "";

        switch (errorCode)
        {
            case Firebase.Auth.AuthError.AccountExistsWithDifferentCredentials:
                message = "Ya existe esa cuenta con credenciales diferentes";
                break;
            case Firebase.Auth.AuthError.MissingPassword:
                message = "Falta la contraseña";
                break;
            case Firebase.Auth.AuthError.WeakPassword:
                message = "Contraseña débil";
                break;
            case Firebase.Auth.AuthError.WrongPassword:
                message = "Contraseña Incorrecta";
                break;
            case Firebase.Auth.AuthError.EmailAlreadyInUse:
                message = "Ya existe una cuenta con ese correo electrónico";
                break;
            case Firebase.Auth.AuthError.InvalidEmail:
                message = "Email no válido";
                break;
            case Firebase.Auth.AuthError.MissingEmail:
                message = "Email no introducido";
                break;
            case Firebase.Auth.AuthError.NetworkRequestFailed:
                message = "Error de conexión de red";
                break;
            case Firebase.Auth.AuthError.UserNotFound:
                message = "Usuario no encontrado";
                break;
            default:
                message = "Ha ocurrido un error";
                break;

        }
        return message;

    }




    // Display information about the currently logged in user.
    void GetUserInfo()
    {
        if (auth.CurrentUser == null)
        {
            DebugLog("Not signed in, unable to get info.");
        }
        else
        {
            DebugLog("Current user info:");
            DisplayDetailedUserInfo(auth.CurrentUser, 1);
        }
    }





    // Sign out the current user.
    public void SignOut()
    {
        if (auth != null)
        {
            DebugLog("Signing out.");
            auth.SignOut();


        }
        //borro el usuario del GameManager
        GameManager.instance.CurrentUser = null;
        //actualizo prefs
        PlayerPrefs.DeleteKey("emailCurrentUser");
        PlayerPrefs.DeleteKey("userIdCurrentUser");
        PlayerPrefs.Save();
        AuthUI.LoadLoginScene();


    }




    /*Método llamado cuando se pulsa el botón de olvidé mi password*/
    public void OnForgotPasswordClick()
    {
        email = authUI.loginEmailInput.text;
        if (String.IsNullOrEmpty(email))
        {
            authUI.loginLogText.text = "Debes introducir un email válido";
            return;
        }

        auth.SendPasswordResetEmailAsync(email).ContinueWith((authTask) =>
        {
            if (LogTaskCompletion(authTask, "Send Password Reset Email", authUI.loginLogText))
            {
                DebugLog("Password reset email sent to " + email);
            }
        });
    }

}
