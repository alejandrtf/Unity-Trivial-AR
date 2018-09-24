using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


/* Script que controla todo el entorno gráfico UI de la pantalla de LOGIN y REGISTRO del usuario*/
public class AuthUI : MonoBehaviour
{

    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject progressBar;

    //login panel elements
    public InputField loginEmailInput;
    public InputField loginPasswordInput;
    public Button ForgotPasswordButton;
    public Text loginLogText;


    //register panel elements
    public InputField registerEmailInput;
    public InputField registerPasswordInput;
    public InputField registerConfirmPasswordInput;
    public Text registerLogText;

    // Use this for initialization
    void Start()
    {
        ShowLoginPanel();
    }


    public void ShowLoginPanel()
    {
        ShowPanel(loginPanel);
    }



    public void ShowRegisterPanel()
    {
        ShowPanel(registerPanel);
    }



    public void LoadLoggedInScene()
    {
        Debug.Log("cargar escena main");
        SceneManager.LoadSceneAsync("MainScene");
    }

    public static void LoadLoginScene()
    {
        SceneManager.LoadScene("LoginScene");
    }

    public void ShowPanel(GameObject panel)
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);

        panel.SetActive(true);
    }

    // Method is called when CancelButton is pressed
    public void OnClickCancelButton()
    {
        ShowLoginPanel();
    }
}
