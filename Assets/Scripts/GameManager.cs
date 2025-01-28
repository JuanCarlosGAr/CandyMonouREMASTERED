using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 

    public GameObject backgroundPanel; 
    public GameObject victoryPanel;
    public GameObject losePanel;

    public int goal; 
    public int moves; 
    public int points; 

    public bool isGameEnded;

    public TMP_Text pointsTxt;
    public TMP_Text movesTxt;
    public TMP_Text goalTxt;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize(int _moves, int _goal)
    {
        moves = _moves;
        goal = _goal;
    }

    void Update()
    {
        pointsTxt.text = "Points: " + points.ToString();
        movesTxt.text = "Moves: " + moves.ToString();
        goalTxt.text = "Goal: " + goal.ToString();
    }

    public void ProcessTurn(int _pointsToGain, bool _subtractMoves, bool isPowerUpActivation = false)
    {
         Debug.Log($"isPowerUpActivation: {isPowerUpActivation}");
        points += _pointsToGain;
        if (_subtractMoves)
            moves--;

        // Sonido de destrucción
        if (_pointsToGain > 0 && !isPowerUpActivation)
        {
            SoundManager soundManager = FindObjectOfType<SoundManager>();
            if (soundManager != null)
            {
                soundManager.PlayDestroySound();
            }
        }

        if (points >= goal)
        {
            isGameEnded = true;
            backgroundPanel.SetActive(true);
            victoryPanel.SetActive(true);
            PotionBoard.Instance.potionParent.SetActive(false);
            return;
        }
    } // <-- Aquí termina ProcessTurn

    // Métodos dentro de la clase
    public void WinGame()
    {
        SceneManager.LoadScene(0);
    }

    public void LoseGame()
    {
        SceneManager.LoadScene(0);
    }
}
