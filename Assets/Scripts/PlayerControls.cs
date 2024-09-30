using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerControls : MonoBehaviour
{
    public Tile currentTile;
    public GameObject gridManagerObject;
    public PowerupController powerupControllerScript;
    public Animator playerAnimator;
    public AudioSource playerAudioSource1;
    public AudioSource playerAudioSource2;
    private bool playerstep1 = false;
    [SerializeField] private List<AudioClip> walkingSounds;
    [SerializeField] private List<AudioClip> walkingIceSounds;

    public string exInput; 
    public string inputCommand = "none";
    public string secondInputCommand = "none";
    private float holdingUpTimer = -1f;
    private float holdingDownTimer = -1f;
    private float holdingLeftTimer = -1f;
    private float holdingRightTimer = -1f;

    //Boost stuff
    private Tile furthestBoostTile;
    private bool foundFurthestBoostTile;
    private List<Tile> boostListOfTilesCrossed;

    private Vector3 currentPos;
    private Vector3 nextPos;
    private float movementLerpAmount;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //Delayed-Movement Controls
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if  (inputCommand == "none"){
                inputCommand = "up";}
            else{
                secondInputCommand = inputCommand;
                inputCommand = "up";}
            MovePlayerUp();
            PlayerTurn();
            holdingUpTimer = 0f;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if  (inputCommand == "none"){
                inputCommand = "down";}
            else{
                secondInputCommand = inputCommand;
                inputCommand = "down";}
            MovePlayerDown();
            PlayerTurn();
            holdingDownTimer = 0f;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if  (inputCommand == "none"){
                inputCommand = "left";}
            else{
                secondInputCommand = inputCommand;
                inputCommand = "left";}
            MovePlayerLeft();
            PlayerTurn();
            holdingLeftTimer = 0f;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if  (inputCommand == "none"){
                inputCommand = "right";}
            else{
                secondInputCommand = inputCommand;
                inputCommand = "right";}
            MovePlayerRight();
            PlayerTurn();
            holdingRightTimer = 0f;
        }

                //release to stop auto-move, set input or second input to none
        if (Input.GetKeyUp(KeyCode.UpArrow)){
            holdingUpTimer = -1f;
            if  (inputCommand == "up"){
                inputCommand = "none";
                exInput = "up";}
            else if (secondInputCommand == "up"){
                secondInputCommand = "none";}
        }
        if (Input.GetKeyUp(KeyCode.DownArrow)){
            holdingDownTimer = -1f;
            if  (inputCommand == "down"){
                inputCommand = "none";
                exInput = "down";}
            else if (secondInputCommand == "down"){
                secondInputCommand = "none";}
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow)){
            holdingLeftTimer = -1f;
            if  (inputCommand == "left"){
                inputCommand = "none";
                exInput = "left";}
            else if (secondInputCommand == "left"){
                secondInputCommand = "none";}
        }
        if (Input.GetKeyUp(KeyCode.RightArrow)){
            holdingRightTimer = -1f;
            if  (inputCommand == "right"){
                inputCommand = "none";
                exInput = "right";}
            else if (secondInputCommand == "right"){
                secondInputCommand = "none";}
        }

        //Set second input to first
        if (secondInputCommand != "none" && inputCommand == "none"){
            inputCommand = secondInputCommand;
            secondInputCommand = "none";
        }

        // Add time to auto-move
        if (inputCommand == "up"){holdingUpTimer += Time.deltaTime *5;}
        if (inputCommand == "down"){holdingDownTimer += Time.deltaTime *5;}
        if (inputCommand == "left"){holdingLeftTimer += Time.deltaTime *5;}
        if (inputCommand == "right"){holdingRightTimer += Time.deltaTime *5;}
        
        // Auto-move if timer high enough
        if (holdingUpTimer >= 1f)
        {
            MovePlayerUp();
            PlayerTurn();
            holdingUpTimer = 0f;
        }
        if (holdingDownTimer >= 1f)
        {
            MovePlayerDown();
            PlayerTurn();
            holdingDownTimer = 0f;
        }
        if (holdingLeftTimer >= 1f)
        {
            MovePlayerLeft();
            PlayerTurn();
            holdingLeftTimer = 0f;
        }
        if (holdingRightTimer >= 1f)
        {
            MovePlayerRight();
            PlayerTurn();
            holdingRightTimer = 0f;
        }

        // Lerping player between tiles
        movementLerpAmount += (Time.deltaTime * 10);
        transform.position = Vector3.Lerp(currentPos, nextPos, movementLerpAmount);

        //Anim code
        if (inputCommand == "none"){playerAnimator.SetInteger("latestDir", 0);}
        if (inputCommand == "up"){playerAnimator.SetInteger("latestDir", 1);}
        if (inputCommand == "down"){playerAnimator.SetInteger("latestDir", 2);}
        if (inputCommand == "left"){playerAnimator.SetInteger("latestDir", 3);}
        if (inputCommand == "right"){playerAnimator.SetInteger("latestDir", 4);}
    }

    public void UpdatePosition()
    {
        currentPos = transform.position;
        nextPos = currentTile.transform.position + new Vector3(0, 1, 0);
        movementLerpAmount = 0f;
    }
    public void SpawnPlayer(Tile startingTile)
    {
        currentTile = startingTile;
        UpdatePosition();
    }

    public void MovePlayerUp()
    {
        if (gridManagerObject.GetComponent<GridManager>().ReturnTileUp(currentTile) != currentTile){
            MoveOffTempTile();
            gridManagerObject.GetComponent<GridManager>().AddScore(1);} //Add 1 point per step
        currentTile = gridManagerObject.GetComponent<GridManager>().ReturnTileUp(currentTile); //Get new tile
        UpdatePosition();
        PlayRandomWalkSound();
    }

    public void MovePlayerDown()
    {
        if (gridManagerObject.GetComponent<GridManager>().ReturnTileDown(currentTile) != currentTile){
            MoveOffTempTile();
            gridManagerObject.GetComponent<GridManager>().AddScore(1);} //Add 1 point per step
        currentTile = gridManagerObject.GetComponent<GridManager>().ReturnTileDown(currentTile); //Get new tile
        UpdatePosition();
        PlayRandomWalkSound();
    }

    public void MovePlayerLeft()
    {
        if (gridManagerObject.GetComponent<GridManager>().ReturnTileLeft(currentTile) != currentTile){
            MoveOffTempTile();
            gridManagerObject.GetComponent<GridManager>().AddScore(1);} //Add 1 point per step
        currentTile = gridManagerObject.GetComponent<GridManager>().ReturnTileLeft(currentTile); //Get new tile
        UpdatePosition();
        PlayRandomWalkSound();
    }

    public void MovePlayerRight()
    {
        if (gridManagerObject.GetComponent<GridManager>().ReturnTileRight(currentTile) != currentTile){
            MoveOffTempTile();
            gridManagerObject.GetComponent<GridManager>().AddScore(1);} //Add 1 point per step
        currentTile = gridManagerObject.GetComponent<GridManager>().ReturnTileRight(currentTile); //Get new tile
        UpdatePosition();
        PlayRandomWalkSound();
    }

    public void PlayerTurn()
    {
        //Ice Tiles
        if (currentTile.isIce)
        {
            powerupControllerScript.useBoost(true);
        }

        //Regular bomb logic
        if (currentTile.GetComponent<Tile>().bombState == 1)
        {
            currentTile.GetComponent<Tile>().TouchBomb();
            gridManagerObject.GetComponent<GridManager>().IncrementBombCount();
        }

        //boost powerup
        if (currentTile.GetComponent<Tile>().hasBoost == true)
        {
            //reset tile underneath me
            currentTile.GetComponent<Tile>().TurnDefault();
            gridManagerObject.GetComponent<GridManager>().currentNumOfPUs -= 1;
            powerupControllerScript.gainBoost();
        }

        if (currentTile.GetComponent<Tile>().hasTempTilePU == true)
        {
            //reset tile underneath me
            currentTile.GetComponent<Tile>().TurnDefault();
            gridManagerObject.GetComponent<GridManager>().currentNumOfPUs -= 1;
            powerupControllerScript.gainTempTile();
        }
       
        //Regular ScoreCube logic
        if (currentTile.GetComponent<Tile>().hasScoreCube == true)
        {
            gridManagerObject.GetComponent<GridManager>().AddScore(50); //Add 100 points
            currentTile.GetComponent<Tile>().ClaimScoreCube();
        }
    }
    private void MoveOffTempTile(){
        //If walking off a temptile, then trigger it
        if (currentTile.isTempTile == true){
            currentTile.TempTileDecrease();
        }
    }

    public void PlayRandomWalkSound()
    {
        if (playerstep1)
        {
            if (currentTile.isIce)
            {
                Debug.Log("Slip");
                playerAudioSource1.clip = walkingSounds[Random.Range(0, walkingIceSounds.Count - 1)];
            }
            else
            {
                playerAudioSource1.clip = walkingSounds[Random.Range(0, walkingSounds.Count - 1)];
            }
            playerAudioSource1.Play();
            playerstep1 = false;
        }
        else
        {
            if (currentTile.isIce)
            {
                playerAudioSource1.clip = walkingSounds[Random.Range(0, walkingIceSounds.Count - 1)];
            }
            else
            {
                playerAudioSource1.clip = walkingSounds[Random.Range(0, walkingSounds.Count - 1)];
            }
            playerAudioSource2.Play();
            playerstep1 = true;
        }
    }
}