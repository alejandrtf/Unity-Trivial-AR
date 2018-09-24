using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; //para ordenar Dictionary

/* Script que controla todo el entorno gráfico UI del panel RANKING*/
public class RankingUI : MonoBehaviour
{
    public Transform userPanelParent; //es el objeto padre que contendrá los usuarios del ranking (el scrollable)
    public GameObject userPanelPrefab; //el prefab a instanciar

    private GameObject _progressbarRanking; //barra que se puestra mientras carga el ranking


    private void Awake()
    {
        //obtengo referencia a la barra de progreso que indica cargando
        _progressbarRanking = gameObject.transform.Find("ProgressBar").gameObject;
    }




    private void OnEnable()
    {
        LoadRanking();
    }




    private void OnDisable()
    {
        //destruyo todos los usuarios del ranking, para que cuando recargue, no los acumule
        foreach (Transform child in userPanelParent.transform)
            Destroy(child.gameObject);
    }




    /*Método que carga el ranking*/
    private void LoadRanking()
    {
        //obtengo el ranking de firebase de forma asíncrona. Cuando lo tenga, se llama el callback ShowRanking
        FirebaseDatabaseManager.instance.GetRankingFromFirebase(ShowRanking); //ShowRanking es un callback que será llamado cuando se tenga el ranking entero (estará en un dictionary<email,score>)
    }


    /* Método callback que es llamado desde GetRankingFromFirebase cuando se recibe el ranking de Firebase.
     Recibe el ranking en forma pares <userEmail,score> 
     */
    private void ShowRanking(Dictionary<string, int> ranking)
    { //en ranking tengo pares <userEmail,score>
        int _contUsers = 1;

        //oculto la barra de progreso de cargando
        _progressbarRanking.SetActive(false);

        //creo tantos objetos UserPanelRanking como usuarios haya y los hago hijos del scrollListContent para que
        //aparezcan en la lista scrollable.

        //recorro el dictionary ordenándolo de mayor a menor, pues cuando llega está ordenado de menos valor a más
        foreach (KeyValuePair<string, int> entryRanking in ranking.OrderByDescending(entry => entry.Value))
        {
            //instancio un UserPanel
            GameObject _userPanel = (GameObject)Instantiate(userPanelPrefab, userPanelParent);
            //le pongo nombre con el formato: User+nº de user
            _userPanel.transform.name = _userPanel.transform.name.Replace("(Clone)", _contUsers.ToString());
            _contUsers++;
            //seteo sus valores
            SetUserEmail(_userPanel, entryRanking.Key);
            SetUserScore(_userPanel, entryRanking.Value);
        }


    }


    /*Método que busca dentro de userPanel el objeto Text UserEmail y le da valor userEmailValue*/
    private void SetUserEmail(GameObject userPanel, string userEmailValue)
    {
        userPanel.transform.Find("EmailUser").gameObject.GetComponent<Text>().text = userEmailValue;
    }



    /*Método que busca dentro de userPanel el objeto Text Score y le da valor score*/
    private void SetUserScore(GameObject userPanel, int score)
    {
        userPanel.transform.Find("Score").gameObject.GetComponent<Text>().text = score.ToString();
    }




    // Method is called when CloseProfileButton is clicked
    public void OnClickCloseRankingButton()
    {
        //hacer invisible el panel Ranking 
        gameObject.SetActive(false);
    }

}
