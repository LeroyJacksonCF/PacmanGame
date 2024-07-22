using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public GameObject gridManagerObject;
    public bool IsBlank;
    public bool IsOccupied;
    public bool hasScoreCube;
    public bool hasBoost;
    public bool hasTempTilePU;
    public bool isTempTile;
    public GameObject dirt;
    public GameObject ExtraScoreCube;
    public GameObject boostG;
    public GameObject boostB;
    public GameObject boostR;
    public int bombState;
    public GameObject bombTileUntouched;
    public GameObject bombTileTouched;
    public int TempTileRemainder;
    public GameObject TempTilePU;
    public GameObject TempTile03;
    public GameObject TempTile02;
    public GameObject TempTile01;

    private int boostTurns;

    public List<Tile> surroundingTiles;
    public int tiletypeInt = 0;
    public List<Sprite> tileSpriteList;
    public SpriteRenderer tileSpriteRenderer;

    [Header("Ingame UI")]
    [Tooltip("Art / VFX stuff")]
    [SerializeField] ParticleSystem chestVFX;
    [SerializeField] ParticleSystem chestDelayedVFX;
    [SerializeField] AudioSource chestOpenSFX;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TurnMountain()
    {
        IsBlank = true;
        IsOccupied = true;
        dirt.SetActive(false);
    }

    public void TurnDefault()
    {
        IsOccupied = false;
        ExtraScoreCube.SetActive(false);
        hasScoreCube = false;
        boostG.SetActive(false);
        boostB.SetActive(false);
        boostR.SetActive(false);
        hasBoost = false;
        if (bombState == 0) { dirt.SetActive(true); }
        hasTempTilePU = false;
        TempTilePU.SetActive(false);
    }

    public bool CanTurnPowerup()
    {
        if (!IsOccupied && !hasScoreCube && !hasBoost && !isTempTile) { return true; }
        else { return false; }
    }
    public void TurnScoreCube()
    {
        ExtraScoreCube.SetActive(true);
        hasScoreCube = true;
    }

    public void ClaimScoreCube()
    {
        TurnDefault();
        chestVFX.Play();
        chestOpenSFX.Play();
        chestDelayedVFX.Play();
    }

    public void TurnBoost()
    {
        boostG.SetActive(true);
        hasBoost = true;
        boostTurns = 20;
    }

    public void ReduceBoostTime()
    {
        boostTurns -= 1;
        switch (boostTurns)
        {

            case 20:
                boostG.SetActive(true);
                break;

            case 10:
                boostG.SetActive(false);
                boostB.SetActive(true);
                break;

            case 1:
                boostB.SetActive(false);
                boostR.SetActive(true);
                break;

            case 0:
                TurnDefault();
                gridManagerObject.GetComponent<GridManager>().currentNumOfPUs -= 1;
                break;
        }
    }

    public void TurnBomb()
    {
        if (bombState == 0 || bombState == 2)
        {
            bombState = 1;
            dirt.SetActive(false);
            bombTileUntouched.SetActive(true);
            bombTileTouched.SetActive(false);
        }
        else
        {
            gridManagerObject.GetComponent<GridManager>().IncrementBombCount();
        }
    }
    public void TouchBomb()
    {
        bombState = 2;
        bombTileUntouched.SetActive(false);
        bombTileTouched.SetActive(true);
    }

    public void TurnTempTilePU()
    {
        hasTempTilePU = true;
        TempTilePU.SetActive(true);
    }

    public void TurnTempTile()
    {
        isTempTile = true;
        TempTileRemainder = 3;
        TempTile03.SetActive(true);
        IsOccupied = false;
    }

    public void TempTileDecrease()
    {
        TempTileRemainder -= 1;
        if (TempTileRemainder == 2)
        {
            TempTile03.SetActive(false);
            TempTile02.SetActive(true);
        }

        else if (TempTileRemainder == 1)
        {
            TempTile02.SetActive(false);
            TempTile01.SetActive(true);
        }

        else if (TempTileRemainder == 0)
        {
            TempTile01.SetActive(false);
            IsOccupied = true;
            isTempTile = false;
        }
    }

    public void ChangeTileSprite()
    { //get a returned list of the compass tile directions
        surroundingTiles = new List<Tile>();
        surroundingTiles = gridManagerObject.GetComponent<GridManager>().ReturnCompassTiles(this, true);
        Debug.Log("Updating: " + gameObject.name);

        /*
        tileTL = (surroundingTiles[0] != this);
        tileT = (surroundingTiles[1] != this);
        tileTR = (surroundingTiles[2] != this);
        tileL = (surroundingTiles[3] != this);
        tileR = (surroundingTiles[4] != this);
        tileBL = (surroundingTiles[5] != this);
        tileB = (surroundingTiles[6] != this);
        tileBR = (surroundingTiles[7] != this);
        */

        tiletypeInt = 0;


        if(surroundingTiles[0] != this && !surroundingTiles[0].IsBlank) { tiletypeInt += 128; }
        if(surroundingTiles[1] != this && !surroundingTiles[1].IsBlank) { tiletypeInt += 64; }
        if(surroundingTiles[2] != this && !surroundingTiles[2].IsBlank) { tiletypeInt += 32; }
        if(surroundingTiles[3] != this && !surroundingTiles[3].IsBlank) { tiletypeInt += 16; }
        if(surroundingTiles[4] != this && !surroundingTiles[4].IsBlank) { tiletypeInt += 8; }
        if(surroundingTiles[5] != this && !surroundingTiles[5].IsBlank) { tiletypeInt += 4; }
        if(surroundingTiles[6] != this && !surroundingTiles[6].IsBlank) { tiletypeInt += 2; }
        if(surroundingTiles[7] != this && !surroundingTiles[7].IsBlank) { tiletypeInt += 1; }


        tileSpriteRenderer.sprite = tileSpriteList[tiletypeInt];
        if (IsBlank) { tileSpriteRenderer.sprite = null; }

    }
}