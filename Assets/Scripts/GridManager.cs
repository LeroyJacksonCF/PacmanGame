using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GridManager : MonoBehaviour
{
    [Header("Board Setup")]
    [SerializeField] private int _width; 
    [SerializeField] private int _height;
    [SerializeField] private int PlayerStartingPosition;
    [SerializeField] private int maxNumOfPUs;
    public int currentNumOfPUs;
    [SerializeField] private int turnsTillPUNumIncrease;
    private int currentPUIncreaseCountdown;
    [SerializeField] private int turnsTillPUSpawn;
    private int currentTurnTillPUSpawn;
    public int roundsTillNextEnemy;
    private int currentRoundTillNextEnemy;

    [Header("ListOfAssets")]
    public List<GameObject> enemyList;
    public List<GameObject> fastEnemyList;
    private int numOfEnemies = 0;
    public int numOfFastEnemies = 0;
    public List<Tile> listOfTiles;
    public List<Tile> listOfTilesToChange;
    private GameObject player;
    public int bombCount;
    [SerializeField] private int mapSizeIncreaseCounter;
    [SerializeField] private int mapSizeIncreaseThreshold;

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    [SerializeField] private Tile _tilePrefab;
    
    public GameObject cameraObject;
    public ScoreManager scoreManager;
    public VFXController vfxHolder;

    [Header("Camera/Audio")]    
    [SerializeField] Vector3 oldCameraPosition;
    [SerializeField] float oldCameraOrthSize;
    [SerializeField] Vector3 newCameraPosition;
    [SerializeField] float newCameraOrthSize;
    [SerializeField] AudioSource bgmAudio;
    [SerializeField] float cameraLerp;
    [SerializeField] AudioSource mapExpandAudio;
    [SerializeField] AudioSource lossAudio;

    //Compass vars
    private bool notTop = false;
    private bool notBot = false;
    private bool notLeft = false;
    private bool notRight = false;

    private void Start()
    {
        scoreManager = cameraObject.GetComponent<ScoreManager>();
        GenerateGrid();
        SpawnPlayer();
        SpawnBomb();

        ResetCamera();
    }

    void GenerateGrid()
    { 
        float randFloat;
        //Set tiles
        for (int x = 0; x < _height; x++) {
            for (int z = 0; z < _width; z++) {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x, 0, -z), Quaternion.Euler(new Vector3(0,90,0)), transform); //Swap this for 2 lines above
                spawnedTile.gridManagerObject = gameObject;
                randFloat = Random.Range(0f, 1f);

                if (randFloat > 0.8f) // chance of ice tile
                {
                    spawnedTile.GetComponent<Tile>().TurnIce();
                }  
                
                //Leaving space for more starting stuff

                spawnedTile.name = $"Tile_{x}_{z}";
                listOfTiles.Add(spawnedTile);
            }
        }
        //tile type calculation second pass
        foreach (Tile tile in listOfTiles)
        {
            tile.ChangeTileSprite();
            tile.ChangeUnderTileSprite();
        }


    }

    void ResetCamera()
    {
        Debug.Log("Camera Moves");

        //old camera stats
        oldCameraPosition = cameraObject.transform.position;
        oldCameraOrthSize = cameraObject.GetComponent<Camera>().orthographicSize;

        //New camera stat calculation
        newCameraPosition = cameraObject.gameObject.transform.position = new Vector3 ((_height/2f) -0.75f, 10, -1 * ((_width/2f) - 0.5f));

        newCameraOrthSize = cameraObject.GetComponent<Camera>().orthographicSize = Mathf.Max(_width, _height) / 2f;

        if (cameraObject.GetComponent<Camera>().aspect < 1.33)
        {
            Debug.Log("Aspect logic. Aspect is: " + cameraObject.GetComponent<Camera>().aspect);
            newCameraOrthSize = cameraObject.GetComponent<Camera>().orthographicSize * 1.33f/(cameraObject.GetComponent<Camera>().aspect);

        }



        //lerp once to stop first frame being buggy
        cameraLerp = 0f;
        cameraObject.GetComponent<Camera>().orthographicSize = Mathf.Lerp(oldCameraOrthSize, newCameraOrthSize, cameraLerp);
        cameraObject.gameObject.transform.position = Vector3.Lerp(oldCameraPosition, newCameraPosition, cameraLerp);
    }

        void Update()
    {
        //Camera smoothly lerping update code
        if (cameraLerp < 1){
            cameraLerp += Time.deltaTime;
            if (Time.deltaTime > 1) { cameraLerp = 1;}
            cameraObject.GetComponent<Camera>().orthographicSize = Mathf.Lerp(oldCameraOrthSize, newCameraOrthSize, cameraLerp);
            cameraObject.gameObject.transform.position = Vector3.Lerp(oldCameraPosition, newCameraPosition, cameraLerp);
        }
    }

    void SpawnPlayer()
    {
        player = Instantiate(playerPrefab, transform);
        player.name = "Player";
        listOfTiles[PlayerStartingPosition].GetComponent<Tile>().TurnDefault();
        player.GetComponent<PlayerControls>().SpawnPlayer(listOfTiles[PlayerStartingPosition]);
        player.GetComponent<PlayerControls>().gridManagerObject = gameObject;
    }

    public void SpawnEnemy(string enemyType)
    {
        GameObject enemy = Instantiate(enemyPrefab, transform);
        enemy.name = "Enemy_" + numOfEnemies;

        //Fast enemy setup logic
        if (enemyType == "fast")
        {
            enemy.GetComponent<EnemyControls>().SetFast();
            numOfFastEnemies++;
            fastEnemyList.Add(enemy);
        }

        //regular enemy setup logic
        else{
            numOfEnemies++;
            enemyList.Add(enemy);
        }

        bool validEnemySpawn = false;
        while (validEnemySpawn == false)
        {
            Tile enemyTileSpawn = listOfTiles[Random.Range(0, (listOfTiles.Count -1))];

            if (enemyTileSpawn != ReturnPlayerTile()
            && (ReturnQuarter(enemyTileSpawn) != ReturnQuarter(ReturnPlayerTile()))
            && enemyTileSpawn.GetComponent<Tile>().IsOccupied == false
            && ((listOfTiles.IndexOf(enemyTileSpawn) < _width) 
            || listOfTiles.IndexOf(enemyTileSpawn) >= listOfTiles.Count - _width
            || listOfTiles.IndexOf(enemyTileSpawn) % _width == 0
            || listOfTiles.IndexOf(enemyTileSpawn) % _width == (_width - 1)))
            {
                enemy.GetComponent<EnemyControls>().SpawnEnemy(enemyTileSpawn);
                //enemyTileSpawn.GetComponent<Tile>().TurnDefault();
                enemy.GetComponent<EnemyControls>().gridManagerObject = gameObject;

                validEnemySpawn = true;
            }
        }
    }


    // Player or Enemy Up/Dow/Left/Right movement returns
    public Tile ReturnTileUp(Tile givenTile, bool returnMissing = false) {
        int positionInListOfTiles = listOfTiles.IndexOf(givenTile);
        if (positionInListOfTiles + _width >= listOfTiles.Count)
        {
            return givenTile;
        }
        else if (returnMissing || listOfTiles[positionInListOfTiles + _width].GetComponent<Tile>().IsOccupied == false)
        {
            return listOfTiles[positionInListOfTiles + _width];
        }
        else
        {
            return givenTile;
        }
    }

    public Tile ReturnTileDown(Tile givenTile, bool returnMissing = false)
    {
        int positionInListOfTiles = listOfTiles.IndexOf(givenTile);
        if (positionInListOfTiles - _width < 0)
        {
            return givenTile;
        }
        else if (returnMissing || listOfTiles[positionInListOfTiles - _width].GetComponent<Tile>().IsOccupied == false)
        {
            return listOfTiles[positionInListOfTiles - _width];
        }
        else
        {
            return givenTile;
        }
    }


    public Tile ReturnTileLeft(Tile givenTile, bool returnMissing = false)
    {
        int positionInListOfTiles = listOfTiles.IndexOf(givenTile);
        if (positionInListOfTiles % _width == 0)
        {
            return givenTile;
        }
        else if (returnMissing || listOfTiles[positionInListOfTiles - 1].GetComponent<Tile>().IsOccupied == false)
        {
            return listOfTiles[positionInListOfTiles - 1];
        }
        else
        {
            return givenTile;
        }
    }


    public Tile ReturnTileRight(Tile givenTile, bool returnMissing = false)
    {
        int positionInListOfTiles = listOfTiles.IndexOf(givenTile);
        if (positionInListOfTiles % _width == _width - 1)
        {
            return givenTile;
        }
        else if (returnMissing || listOfTiles[positionInListOfTiles + 1].GetComponent<Tile>().IsOccupied == false)
        {
            return listOfTiles[positionInListOfTiles + 1];
        }
        else
        {
            return givenTile;
        }
    }

    public Tile ReturnPlayerTile()
    {
        return player.GetComponent<PlayerControls>().currentTile;
    }

    public int ReturnQuarter(Tile givenTile){ //Returns if the player is in the TL, TR, BL, BL corner
        if (listOfTiles.IndexOf(givenTile) -1 < listOfTiles.Count() / 2)
        {
            if (listOfTiles.IndexOf(givenTile) % _width < _width /2 ){return 1;}
            else {return 2;}
        }
        else{
            if (listOfTiles.IndexOf(givenTile) % _width < _width /2 ){return 3;}
            else {return 4;}
        }
    }

    public List<Tile> ReturnCompassTiles(Tile givenTile, bool returnMissing=false)
    {
        List<Tile> compassTiles = new List<Tile>();
        int positionInListOfTiles = listOfTiles.IndexOf(givenTile);
        //Return to default
        notTop = false;
        notBot = false;
        notLeft = false;
        notRight = false;

        if (positionInListOfTiles + _width < listOfTiles.Count){notTop = true;}
        if (positionInListOfTiles - _width >= 0){notBot = true;}
        if (positionInListOfTiles % _width != 0){notLeft = true;}
        if (positionInListOfTiles % _width != _width - 1){notRight = true;}

        if (returnMissing == false) //If not wanting Missing Tiles back
        {
            if (notTop && notLeft) { compassTiles.Add(listOfTiles[positionInListOfTiles + _width - 1]); } //Top L
            if (notTop) { compassTiles.Add(listOfTiles[positionInListOfTiles + _width]); } //Top
            if (notTop && notRight) { compassTiles.Add(listOfTiles[positionInListOfTiles + _width + 1]); } //Top R
            if (notLeft) { compassTiles.Add(listOfTiles[positionInListOfTiles - 1]); } // L
            if (notRight) { compassTiles.Add(listOfTiles[positionInListOfTiles + 1]); } // R
            if (notBot && notLeft) { compassTiles.Add(listOfTiles[positionInListOfTiles - _width - 1]); } // Bot L
            if (notBot) { compassTiles.Add(listOfTiles[positionInListOfTiles - _width]); } // Bot
            if (notBot && notRight) { compassTiles.Add(listOfTiles[positionInListOfTiles - _width + 1]); } //Bot R
        }

        else //If wanting Missing Tiles back, it will put itself in those spots
        {
            if (notTop && notLeft) { compassTiles.Add(listOfTiles[positionInListOfTiles + _width - 1]); }
            else { compassTiles.Add(givenTile); }  //Top L
            if (notTop) { compassTiles.Add(listOfTiles[positionInListOfTiles + _width]); }
            else { compassTiles.Add(givenTile); } //Top
            if (notTop && notRight) { compassTiles.Add(listOfTiles[positionInListOfTiles + _width + 1]); }
            else { compassTiles.Add(givenTile); } //Top R
            if (notLeft) { compassTiles.Add(listOfTiles[positionInListOfTiles - 1]); }
            else { compassTiles.Add(givenTile); } // L
            if (notRight) { compassTiles.Add(listOfTiles[positionInListOfTiles + 1]); }
            else { compassTiles.Add(givenTile); } // R
            if (notBot && notLeft) { compassTiles.Add(listOfTiles[positionInListOfTiles - _width - 1]); }
            else { compassTiles.Add(givenTile); } // Bot L
            if (notBot) { compassTiles.Add(listOfTiles[positionInListOfTiles - _width]); }
            else { compassTiles.Add(givenTile); } // Bot
            if (notBot && notRight) { compassTiles.Add(listOfTiles[positionInListOfTiles - _width + 1]); }
            else { compassTiles.Add(givenTile); } //Bot R
        }

        return compassTiles;
    }

    public void TurnOver()
    {
        foreach (GameObject enemyUnit in enemyList) //Enemies Go
        {
            enemyUnit.GetComponent<EnemyControls>().TakeTurn();

        }
        currentTurnTillPUSpawn += 1;
        if (currentTurnTillPUSpawn >= turnsTillPUSpawn)
        { 
            SpawnRandomPU();
            currentTurnTillPUSpawn = 0;
        }

        foreach (Tile boardTile in listOfTiles)
        {
            if (boardTile.hasBoost == true)
            {boardTile.ReduceBoostTime();}
        }

        if (bombCount >= 9){
            IncreaseMapSize();
            SpawnBomb();
            }
    }

      public void FastTurnOver()
    {
        foreach (GameObject enemyUnit in fastEnemyList) //Enemies Go
        {

                enemyUnit.GetComponent<EnemyControls>().TakeTurn();
        } 
        //currentTurnTillPUSpawn += 1;
    }

    public void AddScore(int extraPoints)
    {
        scoreManager.UpdateScore(extraPoints);
    }

    public void SpawnScoreCube()
    {
        int randomTile = 0;
        while (randomTile != -1)
        {
            randomTile = Random.Range(0, listOfTiles.Count - 1);
            if (listOfTiles[randomTile].GetComponent<Tile>().CanTurnPowerup() 
            && listOfTiles[randomTile].GetComponent<Tile>() != player.GetComponent<PlayerControls>().currentTile)
            {
                listOfTiles[randomTile].GetComponent<Tile>().TurnScoreCube();
                randomTile = -1;
            }
        }
    }

    public void SpawnRandomPU()
    {
        int randInt;
        int randomTile = 0;
        if (currentNumOfPUs < maxNumOfPUs)
        {
            currentNumOfPUs += 1;

            while (randomTile != -1)
            {
                randomTile = Random.Range(0, listOfTiles.Count - 1);
                if (listOfTiles[randomTile].GetComponent<Tile>().CanTurnPowerup())
                {
                    randInt = Random.Range(1, 5);
                    //Choose which PU to give
                    if (randInt == 1){listOfTiles[randomTile].GetComponent<Tile>().TurnBoost();}
                    else if (randInt == 2){listOfTiles[randomTile].GetComponent<Tile>().TurnTempTilePU();}
                    else if (randInt == 3) { SpawnScoreCube(); }
                    else if (randInt == 4) { listOfTiles[randomTile].GetComponent<Tile>().TurnIceStormPU(); }
                    randomTile = -1;
                }
            }
        }
    }

    public void SpawnBomb(){
        bool spawnedBomb = false;
        int spawnedBombTries = 0;

        while (!spawnedBomb)
        {
            Tile bombTile = listOfTiles[Random.Range(0, listOfTiles.Count -1)];
            if (bombTile.GetComponent<Tile>().IsOccupied == false
                && bombTile.GetComponent<Tile>().CanTurnPowerup() == true
                && bombTile != ReturnPlayerTile()
                && bombTile.bombState != 2
                )
            {
                if (spawnedBombTries >= _height || bombTile.turnedGrass)
                { //try not to go on grass a few times, before giving up

                    List<Tile> bomblist = ReturnCompassTiles(bombTile);
                    bombCount = 8 - bomblist.Count;
                    foreach (Tile bombListTile in bomblist)
                    {
                        if (!bombListTile.IsOccupied && !bombListTile.isTempTile)
                        { bombListTile.TurnBomb(); }
                        else { bombCount += 1; }
                    }

                    bombTile.GetComponent<Tile>().TurnBomb();
                    spawnedBomb = true;
                }
            }
            else
            {
                spawnedBombTries += 1;
            }
        }
    }

    public void IncrementBombCount()
    {
        bombCount += 1;
    }

    public void ResetTimer(){
        scoreManager.ResetTime();
    }

    public void GameOver(){
        scoreManager.GameOver();
        lossAudio.Play();
        bgmAudio.Stop();
    }

    public void IncreaseMapSize()
    {
        //check if player has done bomb enough times to increase map
        mapSizeIncreaseCounter += 1;
        if (mapSizeIncreaseCounter >= mapSizeIncreaseThreshold)
        {
            mapSizeIncreaseCounter = 0;

            listOfTilesToChange = new List<Tile>();

            //Audio
            mapExpandAudio.Play();

            //Expand island Particles
            vfxHolder.IncreaseIslandHeight();

            //Makes map one wider
            for (int z = 0; z < _height - 1; z++) {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(z, 0, _width * -1), Quaternion.Euler(new Vector3(0, 90, 0)), transform);
                spawnedTile.gridManagerObject = gameObject;
                if (Random.Range(0.01f, 1f) >= 0.8f) // chance of mountain tile
                {
                    spawnedTile.GetComponent<Tile>().TurnMountain();
                }
                if (Random.Range(0.01f, 1f) >= 0.6f) // chance of ice tile
                {
                    spawnedTile.GetComponent<Tile>().TurnIce();
                }
                spawnedTile.name = $"Tile_{z}_{_width}";
                listOfTiles.Insert(_width + z + (_width * z), spawnedTile);

                //sprite changing code
                listOfTilesToChange.Add(spawnedTile);
                if ((ReturnTileLeft(spawnedTile, true)) != spawnedTile)
                {
                    listOfTilesToChange.Add(ReturnTileLeft(spawnedTile, true));
                }
            }

            //Final makes map one wider, but instead of inserting z, 'adding' instead to avoid error
            var extraSpawnedTile = Instantiate(_tilePrefab, new Vector3(_height - 1, 0, _width * -1), Quaternion.Euler(new Vector3(0, 90, 0)), transform);
            extraSpawnedTile.gridManagerObject = gameObject;
            if (Random.Range(0.01f, 1f) > 0.9f) // chance of mountain tile
            {
                extraSpawnedTile.GetComponent<Tile>().TurnMountain();
            }
            if (Random.Range(0.01f, 1f) > 0.9f) // chance of ice tile
            {
                extraSpawnedTile.GetComponent<Tile>().TurnIce();
            }
            extraSpawnedTile.name = $"Tile_{_height - 1}_{_width}";
            listOfTiles.Add(extraSpawnedTile);


            //sprite changing code
            listOfTilesToChange.Add(extraSpawnedTile);
            listOfTilesToChange.Add(listOfTiles[_width - 1]);


            _width += 1;
            _height += 1;

            // Maes map one taller
            for (int x = 0; x < _width; x++) {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(_height - 1, 0, -x), Quaternion.Euler(new Vector3(0, 90, 0)), transform);
                spawnedTile.gridManagerObject = gameObject;
                if (Random.Range(0.01f, 1f) > 0.9f) // chance of mountain tile
                {
                    spawnedTile.GetComponent<Tile>().TurnMountain();
                }
                if (Random.Range(0.01f, 1f) > 0.9f) // chance of ice tile
                {
                    spawnedTile.GetComponent<Tile>().TurnIce();
                }
                spawnedTile.name = $"Tile_{_height}_{x}";
                listOfTiles.Add(spawnedTile);

                //sprite changing code
                listOfTilesToChange.Add(spawnedTile);
                if ((ReturnTileDown(spawnedTile, true)) != spawnedTile)
                {
                    listOfTilesToChange.Add(ReturnTileDown(spawnedTile, true));
                }

            }
            //tile sprite calculation
            foreach (Tile tile in listOfTilesToChange)
            {
                tile.ChangeTileSprite();
                tile.ChangeUnderTileSprite();
            }


            //Increase number of enemies, after a number of map increases. Also increases map counter threshold;
            if (currentRoundTillNextEnemy == roundsTillNextEnemy)
            { SpawnEnemy("none");
                currentRoundTillNextEnemy = 0;
                mapSizeIncreaseThreshold += 1;
            }
            else
            { currentRoundTillNextEnemy += 1; }


            //increases the max number of boost counts, after a number of map increases
            currentPUIncreaseCountdown += 1;
            if (currentPUIncreaseCountdown == turnsTillPUNumIncrease)
            {
                maxNumOfPUs += 1;
                currentPUIncreaseCountdown = 0;
            }

            ResetCamera();
            scoreManager.UpdateScore(250);
        }
    }
}
