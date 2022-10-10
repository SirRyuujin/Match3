using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicButtonController : MonoBehaviour
{
    public Sprite[] Sprites; // 0 - on | 1 - off
    public Image NoteIcon;
    private bool _isOn = true;

    public void SwapSprite()
    {
        if (_isOn)
            NoteIcon.sprite = Sprites[1];
        else
            NoteIcon.sprite = Sprites[0];

        _isOn = !_isOn;
    }
}