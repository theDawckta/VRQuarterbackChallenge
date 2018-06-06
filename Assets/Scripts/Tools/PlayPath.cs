using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayPath : MonoBehaviour
{
    public JackalopeMediaPlayer MediaPlayer;

    public void Play(InputField input)
    {
        Debug.Log("Given path" + input.text);

        if (MediaPlayer.IsPlaying)
        {
            MediaPlayer.Stop();
        }

        MediaPlayer.MoviePath = input.text;
        MediaPlayer.Play();
    }
}
