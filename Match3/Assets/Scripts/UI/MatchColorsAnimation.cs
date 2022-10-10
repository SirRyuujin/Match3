using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MatchColorsAnimation : MonoBehaviour
{
    public GameEventSO OnColorMatchEvent;
    public float[] RGB;
    private Color _color;

    private void Awake()
    {
        _color = new Color(RGB[0], RGB[1], RGB[2]);
    }

    public void AnimateColorMatch()
    {
        if (OnColorMatchEvent.RecentCaller == null)
            return;

        BoardController board = OnColorMatchEvent.RecentCaller.GetComponent<BoardController>();
        List<TileController> tiles = board.ComboMatches;

        for (int i = 0; i < tiles.Count; i++)
        {
            ColorMatchSequence(tiles[i]);
        }
    }

    private void ColorMatchSequence(TileController tile)
    {
        Image img = tile.GetComponent<Image>();
        img.DOColor(_color, 0);
        img.DOColor(new Color(1, 1, 1), 1f);
    }
}