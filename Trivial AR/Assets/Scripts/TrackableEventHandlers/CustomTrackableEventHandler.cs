using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vuforia;

public class CustomTrackableEventHandler : DefaultTrackableEventHandler
{

    private TrackableBehaviour miTrackableBehaviour;
    private BatsGameInstantiator batsGameInstantiator;


    protected override void Start()
    {
        base.Start();
        miTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (miTrackableBehaviour)
            print("algo detectado");
        else
            print("nada detectado");



    }



    /*Método que se ejecuta al detectar un imageTarjet*/
    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();
        miTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (miTrackableBehaviour)
            print("objeto detectado=" + miTrackableBehaviour.gameObject.name);
        if (!IsActiveMark(miTrackableBehaviour.gameObject.name))
        {

            gameObject.SetActive(false);
            print("bloqueada");

        }
        else
        {
            //marca totalmente operativa
            GameManager.instance.trackedGameObject = miTrackableBehaviour.gameObject; //anoto el target detectado (será el padre del gameObject que yo quiera luego)
                                                                                      //obtengo una referencia al script que controla el juego de los murciélagos
                                                                                      //  batsGameInstantiator = FindObjectOfType<BatsGameInstantiator>();
            batsGameInstantiator = gameObject.transform.Find("BatsGameInstantiatorObject").gameObject.GetComponent<BatsGameInstantiator>();

            if (batsGameInstantiator)
            {

                batsGameInstantiator.TargetDetected();


            }
        }

        //comprueba si hay alguna marca para desbloquear porque hayan pasado ya por otras 2 marcas
        if (GameManager.instance.IsAnyTargetReadyToUnlock())
        {
            //la marca a desbloquear será la más antigua. Están en una cola
            GameManager.instance.DeactivatedMarkQueue.Dequeue();
        }




    }





    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();
        string name = SceneManager.GetActiveScene().name;

        if (batsGameInstantiator)
        {
            if (IsActiveMark(miTrackableBehaviour.gameObject.name))  //marca está operativa
            {


                batsGameInstantiator.TargetLost();
            }
            else
                gameObject.SetActive(false);
        }

    }





    /*Método que devuelve true si la marca imageTarget es activa. False en otro caso*/
    private bool IsActiveMark(string nameTarget)
    {
        if (GameManager.instance.DeactivatedMarkQueue.Count == 0) return true;
        if (GameManager.instance.DeactivatedMarkQueue.Contains(nameTarget))
            return false;
        return true;

    }
}
