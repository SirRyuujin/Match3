using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class PenaltyPointsAnimation : MonoBehaviour
{
    public GameEventSO OnFailedSwapEvent;
    public GameObject Parent;
    public Vector3[] Path;
    public TextMeshProUGUI PenaltyPointsText;

    public void PenaltyPointsSequence()
    {
        Sequence penalty = DOTween.Sequence();
        penalty.Append(Parent.transform.DOPath(Path, 1f)).OnComplete(() => EndSequence());
        
        Parent.SetActive(true);
        Parent.transform.localPosition = Path[0];

        Parent.transform.DOPath(Path, 1.5f, (PathType)PathMode.TopDown2D);
        Parent.transform.DOScale(2, 0.75f);
        penalty.Play();
    }

    private void EndSequence()
    {
        Parent.transform.DOScale(1, 0);
        Parent.SetActive(false);
    }
}