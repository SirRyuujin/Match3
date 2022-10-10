using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BombDetonationAnimation : MonoBehaviour
{
    public GameEventSO OnBombDetonationEvent;
    public float[] SelectColor;

    public void AnimateDetonation()
    {
        if (OnBombDetonationEvent.RecentCaller == null)
            return;

        GameObject obj = OnBombDetonationEvent.RecentCaller;
        List<TileController> AdjacentTiles = obj.GetComponent<BoardController>().ComboMatches;
        Color color = new Color(SelectColor[0], SelectColor[1], SelectColor[2]);
        for (int i = 0; i < AdjacentTiles.Count; i++)
        {
            Image img = AdjacentTiles[i].GetComponent<Image>();
            BlueSquareSequence(img, color);
        }
    }

    private void BlueSquareSequence(Image img, Color color)
    {
        Sequence blueSquare = DOTween.Sequence();
        blueSquare.Append(img.DOColor(color, 0));
        blueSquare.AppendInterval(0.5f);
        blueSquare.Append(img.DOColor(new Color(1, 1, 1), 0.1f));
    }

    private void RevertBack(Image img)
    {
        img.DOColor(new Color(1, 1, 1), 0);
        img.DOFade(1, 0);
    }
}