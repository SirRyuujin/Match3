using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro; 

public class ScoreController : MonoBehaviour
{
    public TextMeshProUGUI ScoreCounterText;
    public GameEventSO OnAddPoints;

    public void UpdateScore()
    {
        if (OnAddPoints.RecentCaller == null)
            return;

        GameObject go = OnAddPoints.RecentCaller;
        BoardController boardController = go.GetComponent<BoardController>(); ;

        ScoreCounterText.text = boardController.Score.ToString();
    }
}