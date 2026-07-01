using UnityEngine;

public class AlternarMusicaConM : MonoBehaviour
{
    public AudioSource musica;
    public KeyCode teclaMusica = KeyCode.M;

    private void Update()
    {
        if (Input.GetKeyDown(teclaMusica))
        {
            AlternarMusica();
        }
    }

    private void AlternarMusica()
    {
        if (musica == null)
        {
            return;
        }

        if (musica.isPlaying)
        {
            musica.Stop();
        }
        else
        {
            musica.Play();
        }
    }
}
