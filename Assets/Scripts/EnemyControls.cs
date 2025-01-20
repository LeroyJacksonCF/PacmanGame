using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControls : MonoBehaviour
{
    public Tile currentTile;
    private Tile newCurrentTile;
    public GameObject gridManagerObject;

    [Header ("Personality")]
    public bool isSmart;
    public bool fast;
    public int frozenCount;

    [Header("Models")]
    [SerializeField] private GameObject regularModel;
    [SerializeField] private GameObject fastModel;
    [SerializeField] private ParticleSystem trailVFX;

    [Header("Relativity To Player - don't edit")]
    [SerializeField] private int isPlayerUpDown = 2; // 1 up, 2 mid, 3 down
    [SerializeField] private int isPlayerLeftRight = 2; // 1 left, 2 mid, 3 right

    //Animation stuff
    private Vector3 currentPos;
    private Vector3 nextPos;
    private float movementLerpAmount;
    [SerializeField] private Animator cowAnimator;
    [SerializeField] private Animator chickenAnimator;


    void Update()
    {
        // Lerping enemy between tiles
        if (movementLerpAmount < 1){
            movementLerpAmount += (Time.deltaTime * 20);
            transform.position = Vector3.Lerp(currentPos, nextPos, movementLerpAmount);
        }
    }

    void UpdatePosition()
    {
        currentPos = transform.position;
        nextPos = currentTile.transform.position + new Vector3(0, 1, 0);
        movementLerpAmount = 0f;
    }

    public void SpawnEnemy(Tile startingTile)
    {
        currentTile = startingTile;
        UpdatePosition();

    }

    public void TakeTurn()
    {
        //Gets direction towards player
        var DirectionToPlayer = transform.position - gridManagerObject.GetComponent<GridManager>().ReturnPlayerTile().transform.position;

        
        if (Vector3.Distance(transform.position, gridManagerObject.GetComponent<GridManager>().ReturnPlayerTile().transform.position) < 2){ //gets smart when close
            isSmart = true;
        }
        else
        {
            isSmart = false;
        } 

        if (DirectionToPlayer.x < 0) { isPlayerUpDown = 1; }
        else if (DirectionToPlayer.x == 0) { isPlayerUpDown = 2; }
        else { isPlayerUpDown = 3; }

        if (DirectionToPlayer.z < 0) { isPlayerLeftRight = 1; }
        else if (DirectionToPlayer.z == 0) { isPlayerLeftRight = 2; }
        else { isPlayerLeftRight = 3; }


        //Frozen logic
        if (frozenCount > 0)
        {
            frozenCount -= 1;

            //diasble hats
            if (frozenCount == 1)
            {
                cowAnimator.SetBool("Frozen", false);
                chickenAnimator.SetBool("Frozen", false);
            }


        }
        else
        {
            if (isSmart) //Smarter AI, always b-lines, even into walls, but never takes a wrong move
            {
                if (isPlayerUpDown == 2) // same height as player
                {
                    if (isPlayerLeftRight == 1) { MoveEnemyLeft(); } // then move left
                    else { MoveEnemyRight(); } // then move right
                }
                else if (isPlayerLeftRight == 2) // if same side-to-side as player
                {
                    if (isPlayerUpDown == 1) { MoveEnemyUp(); } // then move up
                    else { MoveEnemyDown(); } // then move down
                }
                else // if neither
                {
                    int randomInt = Random.Range(1, 100);
                    if (randomInt <= 50)
                    {
                        if (isPlayerLeftRight == 1) { MoveEnemyLeft(); } // then move left
                        else { MoveEnemyRight(); } // then move right
                    }
                    else
                    {
                        if (isPlayerUpDown == 1) { MoveEnemyUp(); } // then move up
                        else { MoveEnemyDown(); } // then move down
                    }
                }
            }
            else if (!isSmart) //b-lines until in line with player, then 50% chance to go out of line
            {
                int randomInt = Random.Range(1, 100);
                if (randomInt <= 50)
                {
                    if (isPlayerLeftRight == 1) { MoveEnemyLeft(); } // then move left
                    else { MoveEnemyRight(); } // then move right
                }
                else
                {
                    if (isPlayerUpDown == 1) { MoveEnemyUp(); } // then move up
                    else { MoveEnemyDown(); } // then move down
                }
            }
        }
        if (currentTile == gridManagerObject.GetComponent<GridManager>().ReturnPlayerTile())
        {
            gridManagerObject.GetComponent<GridManager>().GameOver();
        }
    }

        public void MoveEnemyUp()
    {
        newCurrentTile = gridManagerObject.GetComponent<GridManager>().ReturnTileUp(currentTile); //Get new tile
        currentTile.IsOccupied = false; //set old tile to free
        currentTile = newCurrentTile; 
        currentTile.IsOccupied = true; //set new (or old if not moved) tile to occupied
        UpdatePosition();
    }

    public void MoveEnemyDown()
    {
        newCurrentTile = gridManagerObject.GetComponent<GridManager>().ReturnTileDown(currentTile); //Get new tile
        currentTile.IsOccupied = false; //set old tile to free
        currentTile = newCurrentTile; 
        currentTile.IsOccupied = true; //set new (or old if not moved) tile to occupied
        UpdatePosition();
    }

    public void MoveEnemyLeft()
    {
        newCurrentTile = gridManagerObject.GetComponent<GridManager>().ReturnTileLeft(currentTile); //Get new tile
        currentTile.IsOccupied = false; //set old tile to free
        currentTile = newCurrentTile; 
        currentTile.IsOccupied = true; //set new (or old if not moved) tile to occupied
        UpdatePosition();
        gameObject.transform.localScale = new Vector3(1, 1, -1); //Flip Left
    }

    public void MoveEnemyRight()
    {
        newCurrentTile = gridManagerObject.GetComponent<GridManager>().ReturnTileRight(currentTile); //Get new tile
        currentTile.IsOccupied = false; //set old tile to free
        currentTile = newCurrentTile; 
        currentTile.IsOccupied = true; //set new (or old if not moved) tile to occupied
        UpdatePosition();
        gameObject.transform.localScale = new Vector3(1, 1, 1); //Flip Right
    }

    public void SetFast()
    {
        fast = true;
    }

    public bool ReturnIsFast()
    {
        return fast;
    }

    public void FreezeAnimal(int givenFreezeCount) //Sets the frozen hat, and timer
    {
        frozenCount = givenFreezeCount;
        cowAnimator.SetBool("Frozen", true);
        chickenAnimator.SetBool("Frozen", true);
    }

    public void EnableEnemyModel()
    {
        if (fast) { fastModel.SetActive(true); }
        else { regularModel.SetActive(true); }
    }
}
