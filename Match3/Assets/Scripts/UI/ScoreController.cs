using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro; 

public class ScoreController : MonoBehaviour
{
    public TextMeshProUGUI ScoreCounterText;
    public GameEventSO OnCollectGamesEvent;

    public void UpdateScore()
    {
        if (OnCollectGamesEvent.RecentCaller == null)
            return;

        GameObject go = OnCollectGamesEvent.RecentCaller;
        BoardController boardController = go.GetComponent<BoardController>(); ;

        ScoreCounterText.text = boardController.Score.ToString();
    }
}