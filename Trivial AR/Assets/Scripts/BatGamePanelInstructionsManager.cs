using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatGamePanelInstructionsManager : MonoBehaviour {

    public GameObject panelInstructions; //instructions panel


    private void Start()
    {
        gameObject.SetActive(true);
    }

    /* Método que se ejecuta al pulsar el botón START*/
    public void OnClickOkButton()
    {
        Messenger.Broadcast("stopMusic"); //debo parar la música principal del trivial (escena MainScene), para reproducir la del juego de bats
        Messenger.Broadcast("StartGame");
        gameObject.SetActive(false);
      
        
    }
}
