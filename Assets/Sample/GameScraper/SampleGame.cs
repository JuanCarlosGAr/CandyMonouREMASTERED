using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SampleGame : MonoBehaviour
{
    public int chances = 3;
    public TMP_Text resultText; 
    private int currentChances = 0;
    private int points = 0;

    // Start is called before the first frame update
    void Start()
    {
        // prevent game start without retet
        currentChances = chances;

        //MONOU: not use Monou.GameScraper.Starts() on start
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ResetGame(){
        // reset game data
        currentChances = 0;
        points = 0;
        resultText.text = "Lets Play!";
        //MONOU: notify Monou game starts
        Monou.GameScraper.Starts();
    }

    public async void PlayDice(){
        if(currentChances == chances) return;
        // work game
        currentChances++;
        int n = Random.Range(0,7)+1;
        bool iswinner = n == 7;

        string status = "";
        if(iswinner){
            // if number is 7, then win points
            status = "Winner";
            points++;
            //MONOU: notify Monou game advance
            Monou.GameScraper.Advance(1);
        }else{
            // if number is not 7, then nothing happen
            status = "Looser";
            //MONOU: notify Monou game advance
            Monou.GameScraper.Advance(0);
        }
        // draw result
        resultText.text = $"${status} ({n}). {points} Points. Attempt {currentChances} of {chances}";
        // if game is finish
        if(currentChances == chances){
            //MONOU: notify Monou game finish
            Monou.GameScraper.Finish(points);

            //MONOU: show ads
            Debug.Log("Advertice Start! " + Time.time);
            await Monou.GameScraper.Advertise();

            // reset game on finish
            Debug.Log("Advertice Finish! " + Time.time);
            ResetGame();
        }
    }
}
