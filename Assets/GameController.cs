// (Unity3D) New monobehaviour script that includes regions for common sections, and supports debugging.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    #region GlobalVareables
    #region DefaultVareables
    public bool isDebug = false;
    private string debugScriptName = "GameController";
    #endregion

    #region Static
    public static GameController singleton = null;
    #endregion

    #region Public
    public GameObject gridSpot = null;
    public Vector2 gridCenter = Vector2.zero;
    public bool xStarts = true;
    #endregion

    #region Private
    private bool xTurn = true;
    private List<GameObject> gridSpots = new List<GameObject>();
    private float gridSpotSpacing = 2;
    private bool gameEnded = false;
    private int turnNum = 0;
    #endregion
    #endregion

    #region CustomFunction
    #region Static

    #endregion

    #region Public
    // Called from the GridSpots when they are clicked. This is where win conditions are checked.
    public void NextTurn()
    {
        xTurn = (xTurn) ? false : true;

        UIController.singleton.UpdateText();
        CheckWinConditions();

        turnNum++;
        if(turnNum >= 9)
        {
            gameEnded = true;
            UIController.singleton.ShowLooseScreen();
        }
    }

    // Reload the scene.
    public void Restart()
    {
        SceneManager.LoadScene("sceneOne");
    }
    #endregion

    #region Private
    // Sets up the grid of 9 spaces.
    // Adds spots from left to right and top to bottom.
    private void SetUpGrid()
    {
        Vector2 currPos = gridCenter;
        currPos.x -= gridSpotSpacing;
        currPos.y += gridSpotSpacing;

        for(int i = 0; i < 3; i++)
        {
            for(int ii = 0; ii < 3; ii++)
            {
                GameObject currObj = Instantiate(gridSpot) as GameObject;
                currObj.transform.position = currPos;
                currPos.x += gridSpotSpacing;
                gridSpots.Add(currObj);
            }

            currPos.y -= gridSpotSpacing;
            currPos.x = gridCenter.x - gridSpotSpacing;
        }
    }

      // Checks for win conditions by checking each GridSpot in the List.
     // Then checks adjacent GridSpots to see if they are the same and repeat for those if they are.
     private void CheckWinConditions()
    {
        for(int i = 0; i < 9; i++)
        {
            GridSpot gsScript = gridSpots[i].GetComponent<GridSpot>();
            if(gsScript.State != State.None)
            {
                /* GridSpot placements
                0 | 1 | 2
                3 | 4 | 5
                6 | 7 | 8
                */
                #region StraightConditions
                // Check the left if not on edge
                if(i == 2 || i == 5 || i == 8)
                {
                    if(gridSpots[i - 1].GetComponent<GridSpot>().State == gsScript.State)
                    {
                        if (gridSpots[i - 2].GetComponent<GridSpot>().State == gsScript.State) Victory(gsScript.State);
                    }
                }
                // Check the right if not on edge
                if (i == 0 || i == 3 || i == 6)
                {
                    if (gridSpots[i + 1].GetComponent<GridSpot>().State == gsScript.State)
                    {
                        if (gridSpots[i + 2].GetComponent<GridSpot>().State == gsScript.State) Victory(gsScript.State);
                    }
                }
                // Check the top if not on edge
                if (i == 6 || i == 7 || i == 8)
                {
                    if (gridSpots[i - 3].GetComponent<GridSpot>().State == gsScript.State)
                    {
                        if (gridSpots[i - 6].GetComponent<GridSpot>().State == gsScript.State) Victory(gsScript.State);
                    }
                }
                // Check the bottom if not on edge
                if (i == 0 || i == 1 || i == 2)
                {
                    if (gridSpots[i + 3].GetComponent<GridSpot>().State == gsScript.State)
                    {
                        if (gridSpots[i + 6].GetComponent<GridSpot>().State == gsScript.State) Victory(gsScript.State);
                    }
                }
                #endregion

                #region AngleConditions
                // Check top left if not on edge
                if (i == 8)
                {
                    if (gridSpots[i - 4].GetComponent<GridSpot>().State == gsScript.State)
                    {
                        if (gridSpots[i - 8].GetComponent<GridSpot>().State == gsScript.State) Victory(gsScript.State);
                    }
                }
                // Check top right if not on edge
                if (i == 6)
                {
                    if (gridSpots[i - 2].GetComponent<GridSpot>().State == gsScript.State)
                    {
                        if (gridSpots[i - 4].GetComponent<GridSpot>().State == gsScript.State) Victory(gsScript.State);
                    }
                }
                // Check bottom left if not on edge
                if (i == 2)
                {
                    if (gridSpots[i + 2].GetComponent<GridSpot>().State == gsScript.State)
                    {
                        if (gridSpots[i + 4].GetComponent<GridSpot>().State == gsScript.State) Victory(gsScript.State);
                    }
                }
                // Check bottom right if not on edge
                if (i == 0)
                {
                    if (gridSpots[i + 4].GetComponent<GridSpot>().State == gsScript.State)
                    {
                        if (gridSpots[i + 8].GetComponent<GridSpot>().State == gsScript.State) Victory(gsScript.State);
                    }
                }
                #endregion
            }
        }
    }

    // Shows who won.
    private void Victory(State winner)
    {
        string winnerStr = (winner == State.X) ? "X" : "O";
        PrintDebugMsg(winnerStr + " won!");
        UIController.singleton.UpdateText();
        gameEnded = true;
    }
    #endregion

    #region Debug
    private void PrintDebugMsg(string msg)
    {
        if (isDebug) Debug.Log(debugScriptName + "(" + this.gameObject.name + "): " + msg);
    }
    private void PrintWarningDebugMsg(string msg)
    {
        Debug.LogWarning(debugScriptName + "(" + this.gameObject.name + "): " + msg);
    }
    private void PrintErrorDebugMsg(string msg)
    {
        Debug.LogError(debugScriptName + "(" + this.gameObject.name + "): " + msg);
    }
    #endregion

    #region Getters_Setters
    public bool XTurn
    {
        get
        {
            return xTurn;
        }
    }
    public bool GameEnded
    {
        get
        {
            return gameEnded;
        }
    }
    #endregion
    #endregion

    #region UnityFunctions
    
    #endregion

    #region Start_Update
    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        PrintDebugMsg("Loaded.");

        if (GameController.singleton != null) PrintErrorDebugMsg("There is already a GameController singlton!");
        else singleton = this;
    }
    // Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
    void Start()
    {
        if (gridSpot == null) PrintErrorDebugMsg("No GridSpot set!");
        SetUpGrid();
        xTurn = xStarts;
    }
    // This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    void FixedUpdate()
    {

    }
    // Update is called every frame, if the MonoBehaviour is enabled.
    void Update()
    {

    }
    // LateUpdate is called every frame after all other update functions, if the Behaviour is enabled.
    void LateUpdate()
    {

    }
    #endregion
}