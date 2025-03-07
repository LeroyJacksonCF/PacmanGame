using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupController : MonoBehaviour
{

    [SerializeField] private bool hasBoost;
    [SerializeField] private bool hasTempTile;
    [SerializeField] private bool hasIceStormPU;
    public PlayerControls playerControlsScript;
    public GridManager gridManagerScript;
    public GameObject cameraObject;

    //Boost stuff
    private Tile furthestBoostTile;
    private bool foundFurthestBoostTile;
    private List<Tile> boostListOfTilesCrossed;

    //Ice Storm stuff
    private List<Tile> iceStormCloseList;
    private List<Tile> iceStormFarList;
    private List<Tile> tempIceStormFarList;


    public AudioSource playerBoostSource;
    public AudioSource playerPlaceTempTileSource;
    public AudioSource playerIceStormSource;

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
            else if (hasTempTile){useTempTile();}
            else if (hasIceStormPU) { useIceStorm(); }
        }
    }

    public void ClearAllBoosts(){
        hasBoost = false;
        hasTempTile = false;
        hasIceStormPU = false;
        cameraObject.GetComponent<ScoreManager>().setInventoryIcon("");
    }
    public void gainBoost(){
        ClearAllBoosts();
        cameraObject.GetComponent<ScoreManager>().setInventoryIcon("boost");
        hasBoost = true;
        gridManagerScript.currentNumOfPUs -= 1;
    }
    public void useBoost(bool iceBoost = false)
    {
        //move
        foundFurthestBoostTile = false;
        furthestBoostTile = playerControlsScript.currentTile;
        boostListOfTilesCrossed = new List<Tile>();
        string currentorExInput = "";

        if (!iceBoost)
        {
            playerBoostSource.Play();

            hasBoost = false;

            //Update Score Manager
            cameraObject.GetComponent<ScoreManager>().setInventoryIcon("");
        }

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

                //Ice Tile logic
            if (!furthestBoostTile.isIce && iceBoost)
            {
                foundFurthestBoostTile = true;
            }
        }

        //Check boosted tiles for stuff
        foreach (Tile tileCrossed in boostListOfTilesCrossed)
        {
            //Check boosted tiles for scorecube
            if (tileCrossed.GetComponent<Tile>().hasScoreCube == true)
            {
                tileCrossed.GetComponent<Tile>().ClaimScoreCube();
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
                gainBoost();
                tileCrossed.GetComponent<Tile>().TurnDefault();
            }

            //Check for temp PU tiles
            if (tileCrossed.GetComponent<Tile>().hasTempTilePU)
            {
                gainTempTile();
                tileCrossed.GetComponent<Tile>().TurnDefault();
            }

            //Check for ice storm tiles
            if (tileCrossed.GetComponent<Tile>().hasIceStormPU)
            {
                gainIceStormPU();
                tileCrossed.GetComponent<Tile>().TurnDefault();
            }

            //If walking onto a temptile, then trigger it
            if (tileCrossed.isTempTile == true){
                tileCrossed.TempTileDecrease();
            }

            playerControlsScript.currentTile = furthestBoostTile;
            playerControlsScript.UpdatePosition();

        }
    }

    public void gainTempTile(){
        ClearAllBoosts();
        cameraObject.GetComponent<ScoreManager>().setInventoryIcon("temptile");
        hasTempTile = true;
        gridManagerScript.currentNumOfPUs -= 1;
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


    public void gainIceStormPU()
    {
        ClearAllBoosts();
        cameraObject.GetComponent<ScoreManager>().setInventoryIcon("iceStorm");
        hasIceStormPU = true;
        gridManagerScript.currentNumOfPUs -= 1;
    }
    private void useIceStorm()
    {
        iceStormCloseList = new List<Tile>();
        iceStormFarList = new List<Tile>();

        playerIceStormSource.Play();
        iceStormCloseList = gridManagerScript.ReturnCompassTiles(gridManagerScript.ReturnPlayerTile(), false);

        foreach (Tile closeTile in iceStormCloseList)
        {
            closeTile.GetComponent<Tile>().IceStormVFXBurst(true);
            tempIceStormFarList = gridManagerScript.ReturnCompassTiles(closeTile, false);

            foreach (Tile farTile in tempIceStormFarList)
            {
                if (iceStormCloseList.Contains(farTile) == false)
                {
                    iceStormFarList.Add(farTile);
                }
            }
        }

        iceStormFarList.Remove(gridManagerScript.ReturnPlayerTile()); //Didn't work, I think 25/11

        foreach (Tile farTile in iceStormFarList)
        {
            farTile.GetComponent<Tile>().IceStormVFXBurst(false);
        }


        //freeze enemies
        foreach (GameObject enemy in gridManagerScript.enemyList)
        {
            if (iceStormCloseList.Contains(enemy.GetComponent<EnemyControls>().currentTile))
            {
                enemy.GetComponent<EnemyControls>().FreezeAnimal(5);
            }
            else if (iceStormFarList.Contains(enemy.GetComponent<EnemyControls>().currentTile))
            {
                enemy.GetComponent<EnemyControls>().FreezeAnimal(3);
            }
        }
        foreach (GameObject enemy in gridManagerScript.fastEnemyList)
        {
            if (iceStormCloseList.Contains(enemy.GetComponent<EnemyControls>().currentTile))
            {
                enemy.GetComponent<EnemyControls>().FreezeAnimal(3);
            }
            else if (iceStormFarList.Contains(enemy.GetComponent<EnemyControls>().currentTile))
            {
                enemy.GetComponent<EnemyControls>().FreezeAnimal(2);;
            }
        }





        hasIceStormPU = false;
        cameraObject.GetComponent<ScoreManager>().setInventoryIcon("");
    }
}
