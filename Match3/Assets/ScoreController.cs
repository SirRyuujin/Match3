using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro; 

public class ScoreController : MonoBehaviour
{
    public TextMeshProUGUI ScoreCounterText;
    private int _currentScore;

    public void AddPointsToScore(int amount)
    {
        _currentScore += amount;
        UpdateText(ScoreCounterText, _currentScore.ToString());
    }

    private void UpdateText(TextMeshProUGUI textField, string message)
    {
        textField.text = message;
    }
}
