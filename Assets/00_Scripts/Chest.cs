using UnityEngine;
using DG.Tweening;

public class Chest : MonoBehaviour, Interactuable
{
    public Transform top;
    public bool isOpen = false;
    public void Interact()
    {
        if (isOpen) return; // Evitar abrir el cofre si ya está abierto
        isOpen = true;
        Debug.Log("Has abierto el cofre!");
        // Aquí puedes agregar lógica para dar al jugador un objeto, abrir una animación, etc.
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(()=>InputHandler.instance.canUseInput = false);
        sequence.Append(top.DOLocalRotate(new Vector3(-120, 0, 0), 0.5f).SetEase(Ease.OutBack));

        sequence.AppendInterval(1f); // Espera 1 segundo antes de permitir el movimiento nuevamente
        sequence.AppendCallback(()=>InputHandler.instance.canUseInput = true);

        
    }
}
