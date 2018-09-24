using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SuccessfulGamePanelManager : MonoBehaviour
{


    public Text message;




    public void OnClickOkButton()
    {
        Messenger.Broadcast("StopMusic"); //paro la música


        //oculto el panel
        gameObject.SetActive(false);
        //anoto la marca a ser bloqueada hasta que se cumpla una condición concreta (ya que cada vez que se usan se bloquean).
        GameManager.instance.DeactivatedMarkQueue.Enqueue(GameManager.instance.trackedGameObject.name);
        SceneManager.LoadScene("TrivialQuestionScene");
    }




}
