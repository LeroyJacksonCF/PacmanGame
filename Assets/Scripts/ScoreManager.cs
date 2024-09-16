using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ScoreManager : MonoBehaviour
{
    [Header("Ingame UI")]
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI powerupText;
    [SerializeField] Image powerupSprite;
    [SerializeField] Sprite boostSprite;
    [SerializeField] Sprite temptileSprite;

    [Header("Loss UI")]
    [SerializeField] TextMeshProUGUI lossScoreText;
    [SerializeField] TextMeshProUGUI lossHighScoreText;
    [SerializeField] GameObject lossBackground;
    [SerializeField] GameObject restartButton;

    [Header("Other")]
    [SerializeField] private GridManager gridManagerObject;
    private int score = 0;
    private int finalScore = 0;
    private float time;
    private float turnTimeLimit;
    private float fastEnemyTime;
    private float fastEnemyTimeLimit;

    private float highScore;


    // Start is called before the first frame update
    void Start()
    {
        scoreText.SetText("Score:\n0");
        highScore = PlayerPrefs.GetFloat("HighScore");
    }

    // Update is called once per frame
    void Update()
    {
        turnTimeLimit = 2.5f - ((score * 2) / (score + 100f)); // = max starting time - (max amount to remove) / (score to halve) 
        time += Time.deltaTime;

        fastEnemyTimeLimit = turnTimeLimit * 0.8f;
        fastEnemyTime += Time.deltaTime;

        //for fast enemies
        if (fastEnemyTime >= fastEnemyTimeLimit)
        {
            fastEnemyTime = 0f;
            gridManagerObject.FastTurnOver();
        }

        //for all enemies
        if (time >= turnTimeLimit)
        {
            time = 0f;
            gridManagerObject.TurnOver();
        }

        UpdateTimer();
    }

    public void UpdateScore(int addPoints)
    {
        score += addPoints;
        scoreText.SetText("Score:\n" + score);

        //Summon fast enemy
        if (score >= (2000 * (gridManagerObject.numOfFastEnemies + 1))){
            gridManagerObject.SpawnEnemy("fast");
        }
    }

    private void UpdateTimer()
    {

        timerText.SetText(Mathf.Round(time * 10.0f) * 0.1f + " /" + Mathf.Round(turnTimeLimit * 100.0f) * 0.01f);
    }

    public void ResetTime(){
        time = 0f;
        gridManagerObject.TurnOver();
    }

    public void GameOver(){
        if (finalScore == 0){ 
            finalScore = score;
            lossScoreText.SetText("Score: " + finalScore);

            //set highscore
            if (finalScore > PlayerPrefs.GetFloat("HighScore"))
            {
                PlayerPrefs.SetFloat("HighScore", finalScore);
                lossHighScoreText.SetText("New High Score!");
            }
            else
            {
                lossHighScoreText.SetText("Score: " + PlayerPrefs.GetFloat("HighScore"));
            }

        }


        lossBackground.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(false);

        timerText.gameObject.SetActive(false);
        powerupText.gameObject.SetActive(false);
        time = -99999;
        fastEnemyTime = -99999;

        EventSystem.current.SetSelectedGameObject(restartButton);
    }

    public void RestartGame(){
        SceneManager.LoadScene("MainScene");
    }

    public void setInventoryIcon(string iconString)
    {
        if (iconString == "")
        {
            powerupSprite.gameObject.SetActive(false);
        }
        
        else if (iconString == "boost") 
        {
            powerupSprite.sprite = boostSprite;
            powerupSprite.gameObject.SetActive(true);
        }
        else if (iconString == "temptile")
        {
            powerupSprite.sprite = temptileSprite;
            powerupSprite.gameObject.SetActive(true);
        }
        else
        {powerupText.SetText("Error");}
    }
}