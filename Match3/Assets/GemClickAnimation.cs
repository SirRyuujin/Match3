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
    public float[] SelectColor = new float[3];

    public void AnimateSelect()
    {
        if (OnSelectGemEvent.RecentCaller == null)
            return;

        GameObject obj = OnSelectGemEvent.RecentCaller;
        Color color = new Color(SelectColor[0], SelectColor[1], SelectColor[2]);
        obj.GetComponent<Image>().DOColor(color, SelectedGemAnimationDuration.Value/2); 
        obj.transform.DOScale(new Vector3(SelectedGemScale.Value, SelectedGemScale.Value, 1), SelectedGemAnimationDuration.Value);
    }

    public void AnimateDeselect()
    {
        if (OnDeselectGemEvent.RecentCaller == null)
            return;

        GameObject obj = OnDeselectGemEvent.RecentCaller;
        obj.GetComponent<Image>().DOColor(new Color(1, 1, 1), SelectedGemAnimationDuration.Value / 2);
        obj.transform.DOScale(new Vector3(1, 1, 1), SelectedGemAnimationDuration.Value);
    }
}