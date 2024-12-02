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
    public bool hasIceStormPU;
    public bool isTempTile;
    public bool turnedGrass;
    public bool isIce;
    public GameObject dirt;
    public GameObject ExtraScoreCube;
    public GameObject boostG;
    public GameObject boostB;
    public GameObject boostR;
    public int bombState;
    public GameObject bombTileUntouched;
    public int TempTileRemainder;
    public GameObject TempTilePU;
    public GameObject TempTile03;
    public GameObject TempTile02;
    public GameObject TempTile01;
    public GameObject IceStormPU;

    private int boostTurns;
    public List<Tile> surroundingTiles;
    public int tiletypeInt = 0;
    public List<Sprite> tileSpriteList;
    public List<Sprite> tileSpriteListGrass;
    public List<Sprite> tileSpriteListDirtIce;
    public List<Sprite> tileSpriteListIce;
    public List<Sprite> tileSpriteListUnder;
    public SpriteRenderer tileSpriteRenderer;
    public SpriteRenderer tileSpriteRendererUnder;

    [Header("Ingame UI")]
    [Tooltip("Art / VFX stuff")]
    [SerializeField] ParticleSystem chestVFX;
    [SerializeField] ParticleSystem chestDelayedVFX;
    [SerializeField] AudioSource chestOpenSFX;
    [SerializeField] ParticleSystem iceStormCloseVFX;
    [SerializeField] ParticleSystem iceStormFarVFX;


    public void TurnMountain()
    {
        IsBlank = true;
        IsOccupied = true;
        dirt.SetActive(false);
    }

    public void TurnDefault()
    {
        IsOccupied = false;
        hasScoreCube = false;
        boostG.SetActive(false);
        boostB.SetActive(false);
        boostR.SetActive(false);
        hasBoost = false;
        if (bombState == 0) { dirt.SetActive(true); }
        hasTempTilePU = false;
        TempTilePU.SetActive(false);
        //isIce = false;
        hasIceStormPU = false;
        IceStormPU.SetActive(false);
    }

    public bool CanTurnPowerup()
    {
        if (!IsOccupied && !hasScoreCube && !hasBoost && !isTempTile && bombState != 1 && !hasIceStormPU) { return true; }
        else { return false; }
    }
    public void TurnScoreCube()
    {
        ExtraScoreCube.SetActive(true);
        hasScoreCube = true;
    }

    public void ClaimScoreCube()
    {
        gridManagerObject.GetComponent<GridManager>().AddScore(50);
        hasScoreCube = false;
        ExtraScoreCube.SetActive(false);
        chestVFX.Play();
        chestOpenSFX.Play();
        chestDelayedVFX.Play();
        gridManagerObject.GetComponent<GridManager>().currentNumOfPUs -= 1;
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
        if ((bombState == 0 || bombState == 2) && gameObject.GetComponent<Tile>() != gridManagerObject.GetComponent<GridManager>().ReturnPlayerTile())
        {
            bombState = 1;
            bombTileUntouched.SetActive(true);
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

        if (isIce) //Become Ice
        {
            tileSpriteRenderer.sprite = tileSpriteListIce[tiletypeInt];
            turnedGrass = true;
        }
        else // Become Grass
        {
            tileSpriteRenderer.sprite = tileSpriteListGrass[tiletypeInt];
            turnedGrass = true;
        }
    }

    public void TurnIceStormPU()
    {
        hasIceStormPU = true;
        IceStormPU.SetActive(true);
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
        isIce = false;
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

    public void TurnIce()
    {
        isIce = true;
    }

    public void ChangeTileSprite()
    { //get a returned list of the compass tile directions
        surroundingTiles = new List<Tile>();
        surroundingTiles = gridManagerObject.GetComponent<GridManager>().ReturnCompassTiles(this, true);

        tiletypeInt = 0;

        //TopTile
        if(surroundingTiles[0] != this && !surroundingTiles[0].IsBlank) { tiletypeInt += 128; }
        if(surroundingTiles[1] != this && !surroundingTiles[1].IsBlank) { tiletypeInt += 64; }
        if(surroundingTiles[2] != this && !surroundingTiles[2].IsBlank) { tiletypeInt += 32; }
        if(surroundingTiles[3] != this && !surroundingTiles[3].IsBlank) { tiletypeInt += 16; }
        if(surroundingTiles[4] != this && !surroundingTiles[4].IsBlank) { tiletypeInt += 8; }
        if(surroundingTiles[5] != this && !surroundingTiles[5].IsBlank) { tiletypeInt += 4; }
        if(surroundingTiles[6] != this && !surroundingTiles[6].IsBlank) { tiletypeInt += 2; }
        if(surroundingTiles[7] != this && !surroundingTiles[7].IsBlank) { tiletypeInt += 1; }

        if (isIce && turnedGrass) //Top Tile Ice
        {
            tileSpriteRenderer.sprite = tileSpriteListIce[tiletypeInt];
        }
        else if (isIce) //Top Tile Dirt Ice
        {
            tileSpriteRenderer.sprite = tileSpriteListDirtIce[tiletypeInt];
        }
        else if (turnedGrass) //Top Tile Grass
        {
            tileSpriteRenderer.sprite = tileSpriteListGrass[tiletypeInt];
        }
        else //Top Tile Dirt
        {
            tileSpriteRenderer.sprite = tileSpriteList[tiletypeInt];
        }

        if (IsBlank) { 
            tileSpriteRenderer.sprite = null;
        }
    }

    public void ChangeUnderTileSprite()
    {
        //under Tile
        if (tileSpriteRenderer is not null)
        {
            tileSpriteRendererUnder.sprite = tileSpriteListUnder[tiletypeInt];
        }

        if (IsBlank)
        {
            tileSpriteRendererUnder.sprite = null;
        }
    }

    public void IceStormVFXBurst(bool closeOrFar)
    {
        if (closeOrFar) { iceStormCloseVFX.Play(); }
        else { iceStormFarVFX.Play(); }
    }
}