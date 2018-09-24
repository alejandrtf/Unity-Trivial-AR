using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Butterfly : MonoBehaviour
{

    public float minX = -1.0f, maxX = 2.0f;
    public float minZ = -1.5f, maxZ = 2.5f;
    public float minY = 0f, maxY = 1.0f;

    private bool _isAlive = true;
    private float _minAliveTime = 10f, _maxAliveTime = 20f; //controla si está viva la mariposa
    public static int numButterfly = 0;

    private AudioSource _audioTouched;

    private float _timeToSubstract = 5f; //cada mariposa resta 5 segundos de tiempo

    // Use this for initialization
    void Start()
    {
        //anoto cuántas mariposas creé, lo usaré para darle el nombre cuando lo instancie  "ButterflyX", donde X es el numButterfly
        numButterfly++;
        _audioTouched = GetComponent<AudioSource>();
        StartCoroutine(ButterflyMovement());
        StartCoroutine(DestroyButterfly());



    }


    private void OnMouseDown()
    {
        _isAlive = false;
        //butterfly tocado
        if (GameManager.instance.WithSound)
        {
            //sonido de tocado
            _audioTouched.Play();

        }

        //aviso para restar el tiempo de penalización
        Messenger<float>.Broadcast("substractTime", _timeToSubstract);
        //desactivo el collider del objeto. No tiene renderer
        gameObject.GetComponent<Collider>().enabled = false;
        //lo destruyo en 1 segundos
        Destroy(gameObject, 0.5f);
    }





    //Corrutina para mover las mariposas
    IEnumerator ButterflyMovement()
    {
        while (_isAlive)
        {
            //lo muevo a nueva posición aleatoria cada 2 segundos
            gameObject.transform.Translate(new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ)));
            yield return new WaitForSeconds(3f);
        }
    }

    IEnumerator DestroyButterfly()
    {
        yield return new WaitForSeconds(Random.Range(_minAliveTime, _maxAliveTime));
        Destroy(gameObject);
    }


}
