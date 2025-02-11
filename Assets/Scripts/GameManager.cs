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
    
    public void ProcessTurn(int _pointsToGain, bool _subtractMoves, bool _addMoves, bool isPowerUpActivation = false)
    {
        Debug.Log($"isPowerUpActivation: {isPowerUpActivation}");
        points += _pointsToGain;
        if (_subtractMoves)
            moves--;

        if (_addMoves)
            moves++;

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

            // actualizamos los textos
            var congratulationsTxt = GameObject.FindGameObjectWithTag("congratulationsTxt");
   
            string winMessage = $"fELICIDADES! You won in {moves} moves and scored {points} points!";
           // try{ Monou.MonouArcadeManager.inst.Success(points); } catch {}

            if (congratulationsTxt != null)
            {
                TMP_Text textComp = congratulationsTxt.GetComponent<TMP_Text>();
                if (textComp != null)
                {
                    textComp.text = winMessage;
                }
            }

            PotionBoard.Instance.potionParent.SetActive(false);
        }
        
        // Verificamos si el jugador pierde porque no hay más movimientos
        if (moves <= 0 && points < goal)
        {
            isGameEnded = true;
            print ("perdiste");
            backgroundPanel.SetActive(true);
            losePanel.SetActive(true);

            string loseMessage = $"Ya no tienes más movimientos. lograste {points} puntos! sigue partisipando.";
            try{ Monou.MonouArcadeManager.inst.Success(points); } catch {}
            
            var loseTxt = GameObject.FindGameObjectWithTag("loseText");
            if (loseTxt != null)
            {
                TMP_Text textComp = loseTxt.GetComponent<TMP_Text>();
                if (textComp != null)
                {
                    textComp.text = loseMessage;
                }
            }

            PotionBoard.Instance.potionParent.SetActive(false);
        }
    }
    public void RestartGame()
{
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
}
