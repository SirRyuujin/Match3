using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GemClickAnimation : MonoBehaviour
{
    [Header("References")]
    public GameEventSO OnSelectGemEvent;
    public GameEventSO OnDeselectGemEvent;
    public FloatVariable SelectedGemScale;
    public FloatVariable SelectedGemAnimationDuration;

    public void AnimateSelect()
    {
        if (OnSelectGemEvent.RecentCaller == null)
            return;

        GameObject obj = OnSelectGemEvent.RecentCaller;
        obj.transform.DOScale(new Vector3(SelectedGemScale.Value, SelectedGemScale.Value, 1), SelectedGemAnimationDuration.Value);
    }

    public void AnimateDeselect()
    {
        if (OnDeselectGemEvent.RecentCaller == null)
            return;

        GameObject obj = OnDeselectGemEvent.RecentCaller;
        obj.transform.DOScale(new Vector3(1, 1, 1), SelectedGemAnimationDuration.Value);
    }
}