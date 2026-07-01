using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float destroyDelay = 0f;
    
    [Header("Efectos de Daño")]
    [SerializeField] private GameObject targetGameObject;
    [SerializeField] private AudioSource damageAudio;
    
    private float currentHealth;
    private float previousMaxHealth;
    private Coroutine damageEffectCoroutine;

    private void Awake()
    {
        currentHealth = maxHealth;
        previousMaxHealth = maxHealth;
    }

    private void Update()
    {
        // Detectar cambios en maxHealth
        if (maxHealth != previousMaxHealth)
        {
            previousMaxHealth = maxHealth;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        TriggerDamageEffect();
        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    
    private void TriggerDamageEffect()
    {
        // Detener corrutina anterior si existe
        if (damageEffectCoroutine != null)
        {
            StopCoroutine(damageEffectCoroutine);
        }
        
        damageEffectCoroutine = StartCoroutine(DamageEffectRoutine());
    }
    
    private IEnumerator DamageEffectRoutine()
    {
        if (targetGameObject == null)
        {
            Debug.LogWarning("Health: No se asignó targetGameObject. El efecto visual no funcionará.");
            yield break;
        }
        
        Renderer renderer = targetGameObject.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning("Health: El GameObject asignado no tiene Renderer. El efecto visual no funcionará.");
            yield break;
        }
        
        Material material = renderer.material;
        
        // Habilitar modo transparente
        material.SetFloat("_Mode", 3f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        
        // Cambiar color a rojo y hacerlo translúcido (50%)
        Color redTranslucent = new Color(1f, 0f, 0f, 0.5f);
        material.color = redTranslucent;
        
        // Reproducir audio si existe
        if (damageAudio != null)
        {
            damageAudio.volume = 1f;
            damageAudio.Play();
            Debug.Log("Audio reproduciendo");
        }
        else
        {
            Debug.LogWarning("Health: No se asignó damageAudio. El sonido no se reproducirá.");
        }
        
        // Esperar 1.8 segundos
        yield return new WaitForSeconds(1.8f);
        
        // Restaurar color a blanco opaco
        Color white = Color.white;
        material.color = white;
        
        // Deshabilitar modo transparente
        material.SetFloat("_Mode", 0f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHABLEND_ON");
        material.renderQueue = -1;
    }
    
    private void Die()
    {
        // Aquí puedes agregar animaciones de muerte, efectos, etc.
        Debug.Log($"{gameObject.name} ha muerto.");
        Destroy(gameObject, destroyDelay);
    }
}
