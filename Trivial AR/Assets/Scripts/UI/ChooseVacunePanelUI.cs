using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ChooseVacunePanelUI : MonoBehaviour {
    public GameObject[] vacuneCategory;

    //cuando se active el panel
    private void OnEnable()
    {
        

        if (GameManager.instance.UserVacunes.Count > 0)
        {
            //el usuario ya tiene vacunas (quesitos)
           foreach(string idCategory in GameManager.instance.UserVacunes)
            {
                int index = System.Array.FindIndex<GameObject>(vacuneCategory, vacune => vacune.tag == idCategory);
                if (index != -1)
                {
                    //encontrado, por tanto, esa categoría ya la tiene el usuario, la desactivo
                    vacuneCategory[index].SetActive(false);
                }
            }
        }
        else
        {
            foreach(GameObject vacune in vacuneCategory)
            {
                vacune.SetActive(true);
            }
        }
    }




   
    // Update is called once per frame
    void Update () {
		
	}



    /* Método que se ejecuta cuando se pulsa sobre una vacuna (categoría)*/
    public void OnVacuneClick()
    {
       
        Debug.Log("pulsado: "+EventSystem.current.currentSelectedGameObject.tag);
        //anoto la vacuna elegida
        GameManager.instance.chosenVacune = EventSystem.current.currentSelectedGameObject.tag;
      
        //cargo la escena que contiene el panel QuestionPanel
        SceneManager.LoadScene("TrivialQuestionScene");
    }



    /*Método que desactiva el panel*/
    private void ClosePanel()
    {
        if (gameObject.activeInHierarchy)
            gameObject.SetActive(false);
    }
}
