using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 

    private GameObject OverTxt;
    public GameObject scorePanel;

    public GameObject backgroundPanel; 
    public GameObject victoryPanel;
    public GameObject losePanel;
    public TMP_Text victoryText;

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
        OverTxt = GameObject.Find("GameOver_txt");
                if (OverTxt != null)
        {
            // Desactiva el objeto al inicio
            OverTxt.SetActive(false);
        }
        else
        {
            Debug.LogError("No se encontró el objeto GameOver_txt en la jerarquía.");
        }
    }

    public void Initialize(int _moves, int _goal)
    {
        moves = _moves;
        goal = _goal;
    }

    void Update()
    {
        if (scorePanel.activeSelf)
        {
            pointsTxt.text = "Puntos: " + points.ToString();
            movesTxt.text = "Movidas: " + moves.ToString();
            goalTxt.text = "Goal: " + goal.ToString();
            victoryText.text = points.ToString();
        }
        else return;
    }
        private IEnumerator ActivateAndDeactivateOverCoroutine(float delay)
    {
        OverTxt.SetActive(true);
        yield return new WaitForSeconds(delay);
        OverTxt.SetActive(false);
    }
    
    public void ProcessTurn(int _pointsToGain, bool _subtractMoves, bool _addMoves, bool isPowerUpActivation = false)
    {
        Debug.Log($"isPowerUpActivation: {isPowerUpActivation}");
        points += _pointsToGain;
       // try{ Monou.MonouArcadeManager.inst.Advance(_pointsToGain); } catch {}
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
            victoryText.text = points.ToString();
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
            StartCoroutine(ActivateAndDeactivateOverCoroutine(1.0f));
            StartCoroutine(ShowGameOverAfterDelay());
        }
    }

    private IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSeconds(1.0f);

        isGameEnded = true;
        print("perdiste");
        backgroundPanel.SetActive(true);
        losePanel.SetActive(true);

        string loseMessage = $"Ya no tienes más movimientos. lograste {points} puntos! sigue partisipando.";
        //try { 
      //  Monou.MonouArcadeManager.inst.Success(points); 
        //} catch { }

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

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
