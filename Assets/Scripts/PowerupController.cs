using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupController : MonoBehaviour
{

    [SerializeField] private bool hasBoost;
    [SerializeField] private bool hasTempTile;
    public PlayerControls playerControlsScript;
    public GridManager gridManagerScript;
    public GameObject cameraObject;

    //Boost stuff
    private Tile furthestBoostTile;
    private bool foundFurthestBoostTile;
    private List<Tile> boostListOfTilesCrossed;

    public AudioSource playerBoostSource;
    public AudioSource playerPlaceTempTileSource;

    void Start()
    {
        gridManagerScript = GameObject.Find("GridManager").GetComponent<GridManager>();
        cameraObject = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void Update()
    {
    if (Input.GetKeyDown(KeyCode.Space))
        {
            if (hasBoost){useBoost();}
            if (hasTempTile){useTempTile();}
        }
    }

    public void ClearAllBoosts(){
        hasBoost = false;
        hasTempTile = false;
        cameraObject.GetComponent<ScoreManager>().setInventoryIcon("");
    }
    public void gainBoost(){
        ClearAllBoosts();
        cameraObject.GetComponent<ScoreManager>().setInventoryIcon("boost");
        hasBoost = true;
    }
    private void useBoost()
    {
        //move
        foundFurthestBoostTile = false;
        furthestBoostTile = playerControlsScript.currentTile;
        boostListOfTilesCrossed = new List<Tile>();
        string currentorExInput = "";

        playerBoostSource.Play();

        //Sets current input command, or if that is "none", then the exInput
        if (playerControlsScript.inputCommand != "none"){
            currentorExInput = playerControlsScript.inputCommand;}
        else{
            currentorExInput = playerControlsScript.exInput;}

        while(!foundFurthestBoostTile)
        {
            //If up not blocked,
            if (currentorExInput == "up"){
                if (gridManagerScript.GetComponent<GridManager>().ReturnTileUp(furthestBoostTile) != furthestBoostTile)
                    {
                        boostListOfTilesCrossed.Add(furthestBoostTile);
                        furthestBoostTile = gridManagerScript.GetComponent<GridManager>().ReturnTileUp(furthestBoostTile);
                    }
                else{
                    boostListOfTilesCrossed.Add(furthestBoostTile);
                    foundFurthestBoostTile = true;
                    }
                }
            //If down not blocked,
            else if (currentorExInput == "down"){
                if (gridManagerScript.GetComponent<GridManager>().ReturnTileDown(furthestBoostTile) != furthestBoostTile)
                    {
                        boostListOfTilesCrossed.Add(furthestBoostTile);
                        furthestBoostTile = gridManagerScript.GetComponent<GridManager>().ReturnTileDown(furthestBoostTile);
                    }
                else{
                    boostListOfTilesCrossed.Add(furthestBoostTile);
                    foundFurthestBoostTile = true;
                    }
                }
            //If left not blocked,
            else if (currentorExInput == "left"){
                if (gridManagerScript.GetComponent<GridManager>().ReturnTileLeft(furthestBoostTile) != furthestBoostTile)
                    {
                        boostListOfTilesCrossed.Add(furthestBoostTile);
                        furthestBoostTile = gridManagerScript.GetComponent<GridManager>().ReturnTileLeft(furthestBoostTile);
                    }
                else{
                    boostListOfTilesCrossed.Add(furthestBoostTile);
                    foundFurthestBoostTile = true;
                    }
                }
            //If right not blocked,
            else if (currentorExInput == "right"){
                if (gridManagerScript.GetComponent<GridManager>().ReturnTileRight(furthestBoostTile) != furthestBoostTile)
                    {
                        boostListOfTilesCrossed.Add(furthestBoostTile);
                        furthestBoostTile = gridManagerScript.GetComponent<GridManager>().ReturnTileRight(furthestBoostTile);
                    }
                else{
                    boostListOfTilesCrossed.Add(furthestBoostTile);
                    foundFurthestBoostTile = true;
                    }
                }
            else{boostListOfTilesCrossed.Add(furthestBoostTile);}
        }

        //Check boosted tiles for stuff
        foreach (Tile tileCrossed in boostListOfTilesCrossed)
        {
            //Check boosted tiles for scorecube
            if (tileCrossed.GetComponent<Tile>().hasScoreCube == true)
            {
                gridManagerScript.GetComponent<GridManager>().AddScore(50); //Add 50 points
                tileCrossed.GetComponent<Tile>().TurnDefault();
                gridManagerScript.GetComponent<GridManager>().SpawnScoreCube();
            }

            //Check boosted tiles for bomb tiles
            if (tileCrossed.GetComponent<Tile>().bombState == 1)
            {
                tileCrossed.TouchBomb();
                gridManagerScript.GetComponent<GridManager>().IncrementBombCount();
            }

            //Check for other boost tiles
            if (tileCrossed.GetComponent<Tile>().hasBoost)
            {
                tileCrossed.GetComponent<Tile>().TurnDefault();
            }

            //If walking onto a temptile, then trigger it
            if (tileCrossed.isTempTile == true){
                tileCrossed.TempTileDecrease();
            }

        playerControlsScript.currentTile = furthestBoostTile;
        playerControlsScript.UpdatePosition();

        hasBoost = false;

        //Update Score Manager
        cameraObject.GetComponent<ScoreManager>().setInventoryIcon("off");
        } 
    }

    public void gainTempTile(){
        ClearAllBoosts();
        cameraObject.GetComponent<ScoreManager>().setInventoryIcon("temptile");
        hasTempTile = true;
    }

    private void useTempTile(){
        playerPlaceTempTileSource.Play();

        List<Tile> compassTiles = gridManagerScript.ReturnCompassTiles(gridManagerScript.ReturnPlayerTile());
        foreach (Tile tile in compassTiles){
            if (tile.IsBlank){tile.TurnTempTile();}
        }
        hasTempTile = false;
        cameraObject.GetComponent<ScoreManager>().setInventoryIcon("");
    }
}
