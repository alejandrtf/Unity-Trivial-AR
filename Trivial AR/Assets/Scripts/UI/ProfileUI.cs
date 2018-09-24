using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/* Script que controla todo el entorno gráfico UI del panel PROFILE PANEL*/
public class ProfileUI : MonoBehaviour
{

    public Text userEmail;
    public Text currentLevel;

    //ESTADÍSTICAS
    public Text totalScoreText;
    public Text totalWonGamesText;
    public Text totalQuestionsText;
    public Text totalOkQuestionsText;
    public Text totalOkConsecutiveQuestionsText;



    //CATEGORIAS
    public GameObject[] categoryObject;

    private void OnEnable()
    {
        LoadProfile();
    }




    private void LoadProfile()
    {
        userEmail.text = GameManager.instance.CurrentUser.Email;
        currentLevel.text = "Nivel: " + GameManager.instance.CurrentLevel.ToString();
        //estadísticas
        totalScoreText.text = GameManager.instance.Score.ToString();
        totalWonGamesText.text = GameManager.instance.TotalWonGames.ToString();
        totalQuestionsText.text = GameManager.instance.TotalQuestions.ToString();
        totalOkQuestionsText.text = GameManager.instance.TotalOkQuestions.ToString();
        totalOkConsecutiveQuestionsText.text = GameManager.instance.TotalOkConsecutiveQuestions.ToString();
        //categories
        foreach (GameObject category in categoryObject)
        {
            //category success percent
            GetSuccessPercentCategory(category);
            //category success answers
            GetSuccessAnswerCategory(category);
            //category error answers
            GetErrorAnswerCategory(category);


        }
    }


    // Method is called when CloseProfileButton is clicked
    public void OnClickCloseProfileButton()
    {
        //hacer invisible el panel Profile 
        gameObject.SetActive(false);
    }


    /*Método que rellena el apartado "successPercent" de la categoría que se le pase*/
    private void GetSuccessPercentCategory(GameObject category)
    {
        float percent;
        //category success %
        Text successPercentText = category.transform.Find("SuccessPercent").gameObject.GetComponent<Text>();
        if (GameManager.instance.GetTotalCategoryQuestions(category.tag) != 0)
        {
            //hubo preguntas de esa categoría
            float successNumber = GameManager.instance.GetTotalOkCategoryQuestions(category.tag);
            float totalNumber = GameManager.instance.GetTotalCategoryQuestions(category.tag);

            percent = (successNumber / totalNumber) * 100;
        }
        else
        {
            //no hubo preguntas de esa categoría
            percent = 0;
        }

        successPercentText.text = percent.ToString("F2") + " %"; //lo muestro con 2 decimales sólo.
    }




    /*Método que rellena el apartado "successAnswer" de la categoría que se le pase*/
    private void GetSuccessAnswerCategory(GameObject category)
    {


        Text successAnswerText = category.transform.Find("SuccessAnswer").gameObject.GetComponent<Text>();
        successAnswerText.text = GameManager.instance.GetTotalOkCategoryQuestions(category.tag).ToString();
    }


    /*Método que rellena el apartado "errorAnswer" de la categoría que se le pase*/
    private void GetErrorAnswerCategory(GameObject category)
    {

        float errorAnswer;
        Text errorAnswerText = category.transform.Find("ErrorAnswer").gameObject.GetComponent<Text>();
        if (GameManager.instance.GetTotalCategoryQuestions(category.tag) != 0)
        {
            //hubo preguntas de esa categoría
            errorAnswer = (GameManager.instance.GetTotalCategoryQuestions(category.tag) - GameManager.instance.GetTotalOkCategoryQuestions(category.tag));
        }
        else
        {
            //no hubo preguntas de esa categoría
            errorAnswer = 0;
        }
        errorAnswerText.text = errorAnswer.ToString();
    }

}
