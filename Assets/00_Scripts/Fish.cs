using UnityEngine;

public class Fish : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform meshTransform;
    [SerializeField] private Transform tailTransform;
    
    [Header("Movimiento Ondulante")]
    [SerializeField] private float bodyWaveAmplitude = 0.1f;
    [SerializeField] private float bodyWaveFrequency = 3f;
    [SerializeField] private float lateralAmplitude = 0.08f;
    [SerializeField] private float lateralFrequency = 2.5f;
    
    [Header("Movimiento de Cola")]
    [SerializeField] private float tailWaveAmplitude = 25f;
    [SerializeField] private float tailWaveFrequency = 4f;
    
    private Vector3 meshLocalPositionBase;
    private Quaternion tailLocalRotationBase;

    private void Start()
    {
        // Guardar posiciones base
        if (meshTransform != null)
        {
            meshLocalPositionBase = meshTransform.localPosition;
        }
        
        if (tailTransform != null)
        {
            tailLocalRotationBase = tailTransform.localRotation;
        }
        else
        {
            Debug.LogWarning("Fish: No se asignó tailTransform. La cola no se animará.");
        }
    }

    private void Update()
    {
        if (meshTransform != null)
        {
            AnimateMesh();
        }
        
        if (tailTransform != null)
        {
            AnimateTail();
        }
    }

    private void AnimateMesh()
    {
        // Movimiento ondulante vertical (arriba/abajo)
        float waveOffset = Mathf.Sin(Time.time * bodyWaveFrequency) * bodyWaveAmplitude;
        
        // Movimiento lateral (izquierda/derecha)
        float lateralOffset = Mathf.Sin(Time.time * lateralFrequency) * lateralAmplitude;
        
        // Aplicar movimiento local
        Vector3 newLocalPosition = meshLocalPositionBase;
        newLocalPosition.y += waveOffset;
        newLocalPosition.x += lateralOffset;
        
        meshTransform.localPosition = newLocalPosition;
    }

    private void AnimateTail()
    {
        // Rotación de cola en Z (el eje hacia donde apunta la cola)
        float tailRotation = Mathf.Sin(Time.time * tailWaveFrequency) * tailWaveAmplitude;
        
        Vector3 eulerAngles = tailLocalRotationBase.eulerAngles;
        eulerAngles.z += tailRotation;
        
        tailTransform.localRotation = Quaternion.Euler(eulerAngles);
    }
}
