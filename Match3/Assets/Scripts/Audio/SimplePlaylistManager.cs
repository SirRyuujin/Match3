using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlaylistManager : MonoBehaviour
{
    public AudioSource AudioSource;
    public AudioClip[] Clips;
    public bool Next = false;

    private void Update()
    {
        if (!AudioSource.isPlaying)
            PickClipAndPlay();

        if (Next)
            Skip();
    }

    public void ToogleMute()
    {
        AudioSource.mute = !AudioSource.mute;
    }

    private void PickClipAndPlay()
    {
        AudioSource.clip = GetRandomClip();
        AudioSource.Play();
    }

    private void Skip()
    {
        PickClipAndPlay();
        Next = false;
    }

    private AudioClip GetRandomClip()
    {
        AudioClip clip;

        do
            clip = Clips[Random.Range(0, Clips.Length)];
        while (clip == AudioSource.clip);

        return clip;
    }
}