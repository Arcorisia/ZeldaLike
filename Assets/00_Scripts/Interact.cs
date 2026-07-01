using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class Interact : MonoBehaviour
{
    public enum TurnoInicialTeclaI
    {
        PuntoA,
        Turno1ParaA,
        Turno2ParaA
    }

    [System.Serializable]
    public class TrasladoConTeclaI
    {
        public string nombre;
        public GameObject objeto;
        public TurnoInicialTeclaI turnoInicial;
        public float velocidad = 3f;
    }

    public Collider interactCollider;
    public bool Muerte = false;
    public int vida = 30;
    public float contadorTiempoDeVida = 0f;
    public bool Grito = false;
    public TMP_Text textoVida;
    public int mana = 30;
    public TMP_Text textoMana;

    [Header("Estados de Vida")]
    public GameObject Silencio;
    public GameObject objetoGrito;
    public AudioSource sonidoGrito;
    public Color colorGritoAlPerderVida = Color.red;
    public float duracionTiriteGrito = 1f;
    public float intervaloTiriteGrito = 0.03f;
    public float intensidadTiriteGrito = 0.05f;

    [Header("Teclas Numericas")]
    public AudioSource audioSourceNumeros;
    public AudioClip[] sonidosNumeros = new AudioClip[10];
    public GameObject[] spritesNumeros = new GameObject[10];

    [Header("Bonos de Mana")]
    public TopDownCharacterController topDownCharacterController;
    public Attack attack;
    public float segundosParaActivarBono = 9f;
    public float duracionBono = 5f;
    public float bonoMoveSpeed = 4f;
    public float bonoDamage = 5f;

    [Header("Traslado con tecla I")]
    public Transform puntoACompartido;
    public Transform puntoBCompartido;
    public Transform puntoB1Compartido;
    public Transform puntoB2Compartido;
    public List<TrasladoConTeclaI> trasladosTeclaI = new List<TrasladoConTeclaI>
    {
        new TrasladoConTeclaI { nombre = "Eje vacio", turnoInicial = TurnoInicialTeclaI.PuntoA },
        new TrasladoConTeclaI { nombre = "Libro", turnoInicial = TurnoInicialTeclaI.Turno1ParaA },
        new TrasladoConTeclaI { nombre = "Dialogo", turnoInicial = TurnoInicialTeclaI.Turno2ParaA }
    };

    private readonly KeyCode[] teclasNumericas =
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
        KeyCode.Alpha0
    };

    private bool tocoEnemyEsteSegundo = false;
    private float temporizadorSegundo = 0f;
    private float temporizadorMana = 0f;
    private float temporizadorRegeneracionMana = 0f;
    private Coroutine rutinaGrito;
    private Coroutine rutinaTrasladoTeclaI;
    private bool trasladoTeclaIEnMovimiento = false;
    private List<TrasladoConTeclaI> ordenTurnosTeclaI = new List<TrasladoConTeclaI>();
    private bool posicionOriginalGritoGuardada = false;
    private Vector3 posicionOriginalGrito;
    private int indiceNumeroPresionado = -1;
    private AudioClip clipSonidoGritoOriginal;
    private bool muerteProcesada = false;
    private float tiempoNumeroPresionado = 0f;
    private bool bonoMoveSpeedActivo = false;
    private bool bonoDamageActivo = false;
    private bool bonoMoveSpeedUsadoEnPresion = false;
    private bool bonoDamageUsadoEnPresion = false;
    private Coroutine rutinaBonoMoveSpeed;
    private Coroutine rutinaBonoDamage;
    private const int manaMaximo = 30;

    private void Start()
    {
        mana = Mathf.Clamp(mana, 0, manaMaximo);

        if (sonidoGrito != null)
        {
            clipSonidoGritoOriginal = sonidoGrito.clip;
        }

        if (topDownCharacterController == null && InputHandler.instance != null)
        {
            topDownCharacterController = InputHandler.instance.characterController;
        }

        if (attack == null && InputHandler.instance != null)
        {
            attack = InputHandler.instance.attack;
        }

        InicializarTurnosTeclaI();
        GuardarPosicionOriginalGrito();
        ActualizarObjetosDeGrito();
        ActualizarTextoVida();
        ActualizarTextoMana();
    }

    private void Update()
    {
        if (Muerte)
        {
            ProcesarMuerte();
            return;
        }

        contadorTiempoDeVida += Time.deltaTime;
        temporizadorSegundo += Time.deltaTime;

        if (temporizadorSegundo >= 1f)
        {
            temporizadorSegundo -= 1f;

            if (tocoEnemyEsteSegundo)
            {
                PerderVida(Random.Range(1, 8));
                tocoEnemyEsteSegundo = false;
            }
        }

        RevisarTeclasNumericas();
        ActualizarMana();

        if (Input.GetKeyDown(KeyCode.I))
        {
            IniciarTrasladosTeclaI();
        }
    }

    public void PerformInteract()
    {
        Collider[] hits = Physics.OverlapBox(interactCollider.bounds.center, 
        interactCollider.bounds.extents, interactCollider.transform.rotation);
        foreach (Collider hit in hits)        {
            if (hit.gameObject != gameObject) // Evitar interactuar consigo mismo
            {
                Interactuable interactuable = hit.GetComponent<Interactuable>();
                if (interactuable != null)
                {
                    interactuable.Interact();
                    Debug.Log($"Has interactuado con {hit.gameObject.name}.");
                }
            }
        }        
    }

    private void OnCollisionEnter(Collision collision)
    {
        RevisarContacto(collision.gameObject);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("enemy"))
        {
            tocoEnemyEsteSegundo = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        RevisarContacto(other.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("enemy"))
        {
            tocoEnemyEsteSegundo = true;
        }
    }

    private void RevisarContacto(GameObject otroObjeto)
    {
        if (otroObjeto.CompareTag("enemy"))
        {
            tocoEnemyEsteSegundo = true;
        }

        if (otroObjeto.CompareTag("Medicina"))
        {
            vida += 10;
            RevisarMuerte();
        }
    }

    private void PerderVida(int cantidad)
    {
        vida -= cantidad;
        Grito = true;
        RevisarMuerte();

        if (rutinaGrito != null)
        {
            StopCoroutine(rutinaGrito);
        }

        rutinaGrito = StartCoroutine(RutinaGrito());
    }

    private IEnumerator RutinaGrito()
    {
        ActualizarObjetosDeGrito();
        GuardarPosicionOriginalGrito();

        if (objetoGrito != null)
        {
            Renderer rendererGrito = objetoGrito.GetComponent<Renderer>();
            if (rendererGrito != null)
            {
                rendererGrito.material.color = colorGritoAlPerderVida;
            }
        }

        if (sonidoGrito != null)
        {
            ReproducirSonidoGrito();
        }

        float tiempoTirite = 0f;
        float intervalo = Mathf.Max(0.001f, intervaloTiriteGrito);
        float duracion = Mathf.Max(0f, duracionTiriteGrito);

        while (tiempoTirite < duracion)
        {
            if (objetoGrito != null)
            {
                Vector2 movimiento = Random.insideUnitCircle * intensidadTiriteGrito;
                objetoGrito.transform.localPosition = posicionOriginalGrito + new Vector3(movimiento.x, movimiento.y, 0f);
            }

            yield return new WaitForSeconds(intervalo);
            tiempoTirite += intervalo;
        }

        if (objetoGrito != null)
        {
            objetoGrito.transform.localPosition = posicionOriginalGrito;

            Renderer rendererGrito = objetoGrito.GetComponent<Renderer>();
            if (rendererGrito != null)
            {
                rendererGrito.material.color = Color.white;
            }

            objetoGrito.SetActive(false);
        }

        Grito = false;
        ActualizarObjetosDeGrito();
        rutinaGrito = null;
    }

    private void ActualizarObjetosDeGrito()
    {
        if (Silencio != null)
        {
            Silencio.SetActive(!Grito);
        }

        if (objetoGrito != null)
        {
            objetoGrito.SetActive(Grito);
        }
    }

    private void RevisarMuerte()
    {
        ActualizarTextoVida();

        if (vida <= 0)
        {
            Muerte = true;
            ProcesarMuerte();
        }
    }

    private void ActualizarTextoVida()
    {
        if (textoVida != null)
        {
            textoVida.text = vida.ToString();
        }
    }

    private void ActualizarTextoMana()
    {
        if (textoMana != null)
        {
            textoMana.text = mana.ToString();
        }
    }

    private void RevisarTeclasNumericas()
    {
        if (indiceNumeroPresionado >= 0)
        {
            if (Input.GetKey(teclasNumericas[indiceNumeroPresionado]))
            {
                if (mana <= 0)
                {
                    FinalizarNumeroPresionado();
                    return;
                }

                if (rutinaGrito == null)
                {
                    MantenerSonidoNumeroPresionado();
                }

                if (rutinaGrito == null)
                {
                    ActivarGritoBlancoPorTeclaNumerica();
                }

                RevisarBonosPorNumeroPresionado();
                return;
            }

            FinalizarNumeroPresionado();
            return;
        }

        if (mana <= 0)
        {
            return;
        }

        for (int i = 0; i < teclasNumericas.Length; i++)
        {
            if (Input.GetKeyDown(teclasNumericas[i]))
            {
                ActivarNumero(i);
                break;
            }
        }
    }

    private void ActualizarMana()
    {
        if (indiceNumeroPresionado >= 0)
        {
            temporizadorRegeneracionMana = 0f;
            temporizadorMana += Time.deltaTime;

            while (temporizadorMana >= 1f && mana > 0)
            {
                temporizadorMana -= 1f;
                mana--;
                ActualizarTextoMana();
            }

            if (mana <= 0)
            {
                mana = 0;
                ActualizarTextoMana();
                FinalizarNumeroPresionado();
            }

            return;
        }

        temporizadorMana = 0f;

        if (mana >= manaMaximo)
        {
            mana = manaMaximo;
            return;
        }

        temporizadorRegeneracionMana += Time.deltaTime;
        while (temporizadorRegeneracionMana >= 2f && mana < manaMaximo)
        {
            temporizadorRegeneracionMana -= 2f;
            mana++;
            ActualizarTextoMana();
        }
    }

    private void ActivarNumero(int indice)
    {
        indiceNumeroPresionado = indice;
        tiempoNumeroPresionado = 0f;
        bonoMoveSpeedUsadoEnPresion = false;
        bonoDamageUsadoEnPresion = false;

        if (rutinaGrito == null && audioSourceNumeros != null && indice < sonidosNumeros.Length && sonidosNumeros[indice] != null)
        {
            audioSourceNumeros.Stop();
            audioSourceNumeros.clip = sonidosNumeros[indice];
            audioSourceNumeros.loop = true;
            audioSourceNumeros.Play();
        }

        if (indice < spritesNumeros.Length && spritesNumeros[indice] != null)
        {
            spritesNumeros[indice].SetActive(true);
        }

        if (rutinaGrito == null)
        {
            ActivarGritoBlancoPorTeclaNumerica();
        }
    }

    private void MantenerSonidoNumeroPresionado()
    {
        if (audioSourceNumeros == null || indiceNumeroPresionado < 0 || indiceNumeroPresionado >= sonidosNumeros.Length)
        {
            return;
        }

        if (sonidosNumeros[indiceNumeroPresionado] == null)
        {
            return;
        }

        if (audioSourceNumeros.clip != sonidosNumeros[indiceNumeroPresionado])
        {
            audioSourceNumeros.clip = sonidosNumeros[indiceNumeroPresionado];
        }

        audioSourceNumeros.loop = true;

        if (!audioSourceNumeros.isPlaying)
        {
            audioSourceNumeros.Play();
        }
    }

    private void RevisarBonosPorNumeroPresionado()
    {
        if (indiceNumeroPresionado < 0 || mana <= 0)
        {
            return;
        }

        tiempoNumeroPresionado += Time.deltaTime;

        if (tiempoNumeroPresionado < segundosParaActivarBono)
        {
            return;
        }

        if (indiceNumeroPresionado == 0 && !bonoMoveSpeedActivo && !bonoMoveSpeedUsadoEnPresion)
        {
            bonoMoveSpeedUsadoEnPresion = true;
            rutinaBonoMoveSpeed = StartCoroutine(RutinaBonoMoveSpeed());
        }

        if (indiceNumeroPresionado == 2 && !bonoDamageActivo && !bonoDamageUsadoEnPresion)
        {
            bonoDamageUsadoEnPresion = true;
            rutinaBonoDamage = StartCoroutine(RutinaBonoDamage());
        }
    }

    private IEnumerator RutinaBonoMoveSpeed()
    {
        bonoMoveSpeedActivo = true;

        if (topDownCharacterController != null)
        {
            topDownCharacterController.AddMoveSpeed(bonoMoveSpeed);
        }

        yield return new WaitForSeconds(duracionBono);

        if (topDownCharacterController != null)
        {
            topDownCharacterController.AddMoveSpeed(-bonoMoveSpeed);
        }

        bonoMoveSpeedActivo = false;
        rutinaBonoMoveSpeed = null;
    }

    private IEnumerator RutinaBonoDamage()
    {
        bonoDamageActivo = true;

        if (attack != null)
        {
            attack.damage += bonoDamage;
        }

        yield return new WaitForSeconds(duracionBono);

        if (attack != null)
        {
            attack.damage -= bonoDamage;
        }

        bonoDamageActivo = false;
        rutinaBonoDamage = null;
    }

    private void ActivarGritoBlancoPorTeclaNumerica()
    {
        Grito = true;

        if (Silencio != null)
        {
            Silencio.SetActive(false);
        }

        if (objetoGrito != null)
        {
            GuardarPosicionOriginalGrito();
            objetoGrito.transform.localPosition = posicionOriginalGrito;
            objetoGrito.SetActive(true);
            Renderer rendererGrito = objetoGrito.GetComponent<Renderer>();
            if (rendererGrito != null)
            {
                rendererGrito.material.color = Color.white;
            }
        }
    }

    private void FinalizarNumeroPresionado()
    {
        if (audioSourceNumeros != null && !(rutinaGrito != null && audioSourceNumeros == sonidoGrito))
        {
            audioSourceNumeros.Stop();
            audioSourceNumeros.loop = false;
            audioSourceNumeros.clip = null;
        }

        if (indiceNumeroPresionado >= 0 && indiceNumeroPresionado < spritesNumeros.Length && spritesNumeros[indiceNumeroPresionado] != null)
        {
            spritesNumeros[indiceNumeroPresionado].SetActive(false);
        }

        indiceNumeroPresionado = -1;
        tiempoNumeroPresionado = 0f;
        bonoMoveSpeedUsadoEnPresion = false;
        bonoDamageUsadoEnPresion = false;

        if (rutinaGrito != null)
        {
            return;
        }

        Grito = false;

        if (objetoGrito != null)
        {
            objetoGrito.SetActive(false);
        }

        if (Silencio != null)
        {
            Silencio.SetActive(true);
        }
    }

    private void ReproducirSonidoGrito()
    {
        if (sonidoGrito == null)
        {
            return;
        }

        if (audioSourceNumeros == sonidoGrito && clipSonidoGritoOriginal != null)
        {
            sonidoGrito.Stop();
            sonidoGrito.loop = false;
            sonidoGrito.clip = clipSonidoGritoOriginal;
            sonidoGrito.Play();
            return;
        }

        sonidoGrito.Play();
    }

    private void GuardarPosicionOriginalGrito()
    {
        if (!posicionOriginalGritoGuardada && objetoGrito != null)
        {
            posicionOriginalGrito = objetoGrito.transform.localPosition;
            posicionOriginalGritoGuardada = true;
        }
    }

    private void IniciarTrasladosTeclaI()
    {
        if (trasladoTeclaIEnMovimiento)
        {
            return;
        }

        if (ordenTurnosTeclaI.Count == 0)
        {
            InicializarTurnosTeclaI();
        }

        if (ordenTurnosTeclaI.Count < 2)
        {
            return;
        }

        List<TrasladoConTeclaI> nuevoOrden = new List<TrasladoConTeclaI>();
        for (int i = 1; i < ordenTurnosTeclaI.Count; i++)
        {
            nuevoOrden.Add(ordenTurnosTeclaI[i]);
        }

        nuevoOrden.Add(ordenTurnosTeclaI[0]);
        rutinaTrasladoTeclaI = StartCoroutine(RutinaAplicarTurnoTeclaI(nuevoOrden));
    }

    private void InicializarTurnosTeclaI()
    {
        ordenTurnosTeclaI.Clear();
        AgregarPrimerTurnoInicial(TurnoInicialTeclaI.PuntoA);
        AgregarPrimerTurnoInicial(TurnoInicialTeclaI.Turno1ParaA);
        AgregarPrimerTurnoInicial(TurnoInicialTeclaI.Turno2ParaA);

        for (int i = 0; i < trasladosTeclaI.Count; i++)
        {
            TrasladoConTeclaI traslado = trasladosTeclaI[i];
            if (traslado != null && !ordenTurnosTeclaI.Contains(traslado))
            {
                ordenTurnosTeclaI.Add(traslado);
            }
        }

        PosicionarTurnosTeclaI();
    }

    private void AgregarPrimerTurnoInicial(TurnoInicialTeclaI turnoInicial)
    {
        for (int i = 0; i < trasladosTeclaI.Count; i++)
        {
            TrasladoConTeclaI traslado = trasladosTeclaI[i];
            if (traslado != null && traslado.turnoInicial == turnoInicial && !ordenTurnosTeclaI.Contains(traslado))
            {
                ordenTurnosTeclaI.Add(traslado);
                return;
            }
        }
    }

    private void PosicionarTurnosTeclaI()
    {
        if (puntoACompartido == null)
        {
            return;
        }

        for (int i = 0; i < ordenTurnosTeclaI.Count; i++)
        {
            TrasladoConTeclaI traslado = ordenTurnosTeclaI[i];
            if (traslado == null || traslado.objeto == null)
            {
                continue;
            }

            Transform destino = ObtenerPuntoTurnoTeclaI(i);
            if (destino != null)
            {
                traslado.objeto.transform.position = destino.position;
            }
        }
    }

    private IEnumerator RutinaAplicarTurnoTeclaI(List<TrasladoConTeclaI> nuevoOrden)
    {
        trasladoTeclaIEnMovimiento = true;

        while (HayTrasladosTeclaIEnMovimiento(nuevoOrden))
        {
            MoverTurnosTeclaI(nuevoOrden);
            yield return null;
        }

        ordenTurnosTeclaI = nuevoOrden;
        PosicionarTurnosTeclaI();
        trasladoTeclaIEnMovimiento = false;
        rutinaTrasladoTeclaI = null;
    }

    private bool HayTrasladosTeclaIEnMovimiento(List<TrasladoConTeclaI> nuevoOrden)
    {
        if (puntoACompartido == null)
        {
            return false;
        }

        for (int i = 0; i < nuevoOrden.Count; i++)
        {
            TrasladoConTeclaI traslado = nuevoOrden[i];
            if (traslado == null || traslado.objeto == null)
            {
                continue;
            }

            Transform destino = ObtenerPuntoTurnoTeclaI(i);
            if (destino != null && Vector3.Distance(traslado.objeto.transform.position, destino.position) > 0.01f)
            {
                return true;
            }
        }

        return false;
    }

    private void MoverTurnosTeclaI(List<TrasladoConTeclaI> nuevoOrden)
    {
        for (int i = 0; i < nuevoOrden.Count; i++)
        {
            TrasladoConTeclaI traslado = nuevoOrden[i];
            if (traslado == null || traslado.objeto == null || puntoACompartido == null)
            {
                continue;
            }

            Transform destino = ObtenerPuntoTurnoTeclaI(i);
            if (destino == null)
            {
                continue;
            }

            float velocidad = Mathf.Max(0.01f, traslado.velocidad);
            traslado.objeto.transform.position = Vector3.MoveTowards(
                traslado.objeto.transform.position,
                destino.position,
                velocidad * Time.deltaTime);
        }
    }

    private Transform ObtenerPuntoTurnoTeclaI(int indiceTurno)
    {
        if (indiceTurno == 0)
        {
            return puntoACompartido;
        }

        if (indiceTurno == 1)
        {
            return puntoB1Compartido != null ? puntoB1Compartido : puntoBCompartido;
        }

        return puntoB2Compartido != null ? puntoB2Compartido : puntoBCompartido;
    }

    private void ProcesarMuerte()
    {
        if (muerteProcesada)
        {
            return;
        }

        muerteProcesada = true;

        if (rutinaTrasladoTeclaI != null)
        {
            StopCoroutine(rutinaTrasladoTeclaI);
            rutinaTrasladoTeclaI = null;
        }

        trasladoTeclaIEnMovimiento = false;
        OrdenarTurnosConLibroEnPuntoA();
        ActivarGameOverDelLibro();
        DesactivarInputsDelPlayer();
        CrearReiniciadorDeEscena();
    }

    private void OrdenarTurnosConLibroEnPuntoA()
    {
        TrasladoConTeclaI libro = BuscarTrasladoPorNombre("Libro");
        if (libro == null)
        {
            PosicionarTurnosTeclaI();
            return;
        }

        List<TrasladoConTeclaI> nuevoOrden = new List<TrasladoConTeclaI>();
        nuevoOrden.Add(libro);

        for (int i = 0; i < ordenTurnosTeclaI.Count; i++)
        {
            TrasladoConTeclaI traslado = ordenTurnosTeclaI[i];
            if (traslado != null && traslado != libro && !nuevoOrden.Contains(traslado))
            {
                nuevoOrden.Add(traslado);
            }
        }

        for (int i = 0; i < trasladosTeclaI.Count; i++)
        {
            TrasladoConTeclaI traslado = trasladosTeclaI[i];
            if (traslado != null && traslado != libro && !nuevoOrden.Contains(traslado))
            {
                nuevoOrden.Add(traslado);
            }
        }

        ordenTurnosTeclaI = nuevoOrden;
        PosicionarTurnosTeclaI();
    }

    private TrasladoConTeclaI BuscarTrasladoPorNombre(string nombre)
    {
        for (int i = 0; i < trasladosTeclaI.Count; i++)
        {
            TrasladoConTeclaI traslado = trasladosTeclaI[i];
            if (traslado != null && traslado.nombre == nombre)
            {
                return traslado;
            }
        }

        return null;
    }

    private void ActivarGameOverDelLibro()
    {
        ActivarHijoDelLibro("Game Over");
    }

    public void ActivarLibroEnPuntoAConHijo(string nombreHijo)
    {
        OrdenarTurnosConLibroEnPuntoA();
        ActivarHijoDelLibro(nombreHijo);
    }

    private void ActivarHijoDelLibro(string nombreHijo)
    {
        TrasladoConTeclaI libro = BuscarTrasladoPorNombre("Libro");
        if (libro == null || libro.objeto == null)
        {
            return;
        }

        Transform hijo = BuscarHijoPorNombre(libro.objeto.transform, nombreHijo);
        if (hijo != null)
        {
            hijo.gameObject.SetActive(true);
        }
    }

    private Transform BuscarHijoPorNombre(Transform padre, string nombre)
    {
        for (int i = 0; i < padre.childCount; i++)
        {
            Transform hijo = padre.GetChild(i);
            if (hijo.name == nombre)
            {
                return hijo;
            }

            Transform encontrado = BuscarHijoPorNombre(hijo, nombre);
            if (encontrado != null)
            {
                return encontrado;
            }
        }

        return null;
    }

    private void DesactivarInputsDelPlayer()
    {
        InputHandler inputHandler = InputHandler.instance;
        if (inputHandler == null)
        {
            inputHandler = FindObjectOfType<InputHandler>();
        }

        if (inputHandler != null)
        {
            inputHandler.canUseInput = false;

            if (topDownCharacterController == null)
            {
                topDownCharacterController = inputHandler.characterController;
            }
        }

        if (topDownCharacterController == null)
        {
            topDownCharacterController = FindObjectOfType<TopDownCharacterController>();
        }

        if (topDownCharacterController != null)
        {
            topDownCharacterController.Move(Vector2.zero);
        }
    }

    private void CrearReiniciadorDeEscena()
    {
        GameObject reiniciador = new GameObject("Reiniciador Escena Muerte");
        reiniciador.AddComponent<ReiniciadorEscenaMuerte>().Reiniciar(3f);
    }
    
}

class ReiniciadorEscenaMuerte : MonoBehaviour
{
    public void Reiniciar(float demora)
    {
        StartCoroutine(ReiniciarEscena(demora));
    }

    private IEnumerator ReiniciarEscena(float demora)
    {
        yield return new WaitForSeconds(demora);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
