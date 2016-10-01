// (Unity3D) New monobehaviour script that includes regions for common sections, and supports debugging.
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    #region GlobalVareables
    #region DefaultVareables
    public bool isDebug = false;
    private string debugScriptName = "UIController";
    #endregion

    #region Static
    public static UIController singleton = null;
    #endregion

    #region Public

    #endregion

    #region Private
    private Text currTurnText = null;
    private Text winnerText = null;
    private GameObject restartButton = null;
    #endregion
    #endregion

    #region CustomFunction
    #region Static

    #endregion

    #region Public
    // Updates the text objects that the text objects need to display.
    public void UpdateText()
    {
        string turnStr = (GameController.singleton.XTurn) ? "X" : "O";
        currTurnText.text = turnStr + "'s turn.";
        if (GameController.singleton.GameEnded)
        {
            string winnerStr = (GameController.singleton.XTurn) ? "O" : "X";
            winnerText.text = winnerStr + " won!";
            winnerText.gameObject.SetActive(true);
            restartButton.SetActive(true);
        }
        else
        {
            winnerText.gameObject.SetActive(false);
            restartButton.SetActive(false);
        }
    }

    // Show the end game text and restart button.
    public void ShowLooseScreen()
    {
        winnerText.text = "Draw!";
        winnerText.gameObject.SetActive(true);
        restartButton.SetActive(true);
    }
    #endregion

    #region Private
    // Finds all the UI objects and goes through each and assigns the ones that are needed to thier respective variables.
    private void AssignUIObjects()
    {
        GameObject[] uiObjs = GameObject.FindGameObjectsWithTag("UI");
        foreach(GameObject ui in uiObjs)
        {
            if (ui.gameObject.name == "CurrTurn") currTurnText = ui.GetComponent<Text>();
            else if (ui.gameObject.name == "Winner") winnerText = ui.GetComponent<Text>();
            else if (ui.gameObject.name == "Restart") restartButton = ui;
        }
        
        if (currTurnText == null) PrintErrorDebugMsg("No CurrTurn text object found!");
        if (winnerText == null) PrintErrorDebugMsg("No Winner text object found!");
        if (restartButton == null) PrintErrorDebugMsg("No Restart button object found!");
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

    #endregion
    #endregion

    #region UnityFunctions

    #endregion

    #region Start_Update
    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        PrintDebugMsg("Loaded.");

        if (UIController.singleton != null) PrintErrorDebugMsg("There is already a UIController singlton!");
        else singleton = this;
    }
    // Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
    void Start()
    {
        AssignUIObjects();
        winnerText.gameObject.SetActive(false);
        restartButton.SetActive(false);
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