using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bat : MonoBehaviour
{

    public float minX = -1.0f, maxX = 1.0f;
    public float minZ = -2f, maxZ = 2f;
    public float minY = 0f, maxY = 1.0f;
    public float offset = 0.9f; //lo uso para desplazar el centro del efecto explosiónBat, porque siempre sale por debajo del gameObject

    public GameObject explosionBat; //efecto visual al matarlo

    private bool _isAlive = true;
    private float _minAliveTime = 10f, _maxAliveTime = 15f; //controla si está vivo el murciélago
    public static int numBat = 0;
    private AudioSource _audioDying;




    // Use this for initialization
    void Start()
    {
        //anoto cuántos bats creé, lo usaré para darle el nombre cuando lo instancie  "BatX", donde X es el numBat
        numBat++;
        _audioDying = GetComponent<AudioSource>();
        StartCoroutine(BatMovement());
        StartCoroutine(DestroyBat());



    }



    private void OnMouseDown()
    {
        //bat tocado
        if (GameManager.instance.WithSound)
        {
            //sonido de muerto
            _audioDying.Play();
        }
        _isAlive = false;
        //incremento bats matados
        print("gameobject TOCADO=" + gameObject.name);
        print("DENTRO DE onmousedown, bat tocado y antes de actualizar numkilledbats=" + BatsGameInstantiator.numKilledBats);
        BatsGameInstantiator.numKilledBats++;
        print("DENTRO DE onmousedown, bat tocado y despues de actualizar numkilledbats=" + BatsGameInstantiator.numKilledBats);
        Vector3 origen = new Vector3(transform.position.x, transform.position.y + offset, transform.position.z);
        Destroy(Instantiate(explosionBat, origen, Quaternion.identity), 1); //destruyo a los 1 seg el efecto explosión
                                                                            //desactivo el collider del objeto. No tiene renderer
        gameObject.GetComponent<Collider>().enabled = false;

        //lo destruyo en 0.8 segundos
        Destroy(gameObject, 0.8f);
    }





    //Corrutina para mover el murciélago
    IEnumerator BatMovement()
    {
        while (_isAlive)
        {
            //lo muevo a nueva posición aleatoria cada 2 segundos
            gameObject.transform.Translate(new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ)));
            yield return new WaitForSeconds(3f);
        }
    }

    IEnumerator DestroyBat()
    {
        yield return new WaitForSeconds(Random.Range(_minAliveTime, _maxAliveTime));
        Destroy(gameObject);
    }




}
