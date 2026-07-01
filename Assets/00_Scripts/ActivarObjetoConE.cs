using UnityEngine;
using System.Collections;

public class ActivarObjetoConE : MonoBehaviour
{
    public Collider colliderObjetivo;
    public GameObject objetoAActivar;
    public KeyCode teclaActivar = KeyCode.E;
    public Interact interact;
    public float esperaParaActivarLibro = 4f;
    public string hijoLibroAActivar = "Victory";

    private bool estaColisionando = false;
    private Coroutine rutinaActivarLibro;

    private void Update()
    {
        if (estaColisionando && Input.GetKeyDown(teclaActivar))
        {
            ActivarObjeto();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        RevisarEntrada(other);
    }

    private void OnTriggerExit(Collider other)
    {
        RevisarSalida(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        RevisarEntrada(collision.collider);
    }

    private void OnCollisionExit(Collision collision)
    {
        RevisarSalida(collision.collider);
    }

    private void RevisarEntrada(Collider other)
    {
        if (other == colliderObjetivo)
        {
            estaColisionando = true;
        }
    }

    private void RevisarSalida(Collider other)
    {
        if (other == colliderObjetivo)
        {
            estaColisionando = false;
        }
    }

    private void ActivarObjeto()
    {
        if (objetoAActivar != null)
        {
            objetoAActivar.SetActive(true);
        }

        if (rutinaActivarLibro != null)
        {
            StopCoroutine(rutinaActivarLibro);
        }

        rutinaActivarLibro = StartCoroutine(ActivarLibroDespuesDeEspera());
    }

    private IEnumerator ActivarLibroDespuesDeEspera()
    {
        yield return new WaitForSeconds(esperaParaActivarLibro);

        if (interact == null)
        {
            interact = FindObjectOfType<Interact>();
        }

        if (interact != null)
        {
            interact.ActivarLibroEnPuntoAConHijo(hijoLibroAActivar);
        }

        rutinaActivarLibro = null;
    }
}
