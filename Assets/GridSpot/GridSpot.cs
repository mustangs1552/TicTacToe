// (Unity3D) New monobehaviour script that includes regions for common sections, and supports debugging.
using UnityEngine;
using System.Collections;

public enum State
{
    X,
    O,
    None
}

public class GridSpot : MonoBehaviour
{
    #region GlobalVareables
    #region DefaultVareables
    public bool isDebug = false;
    private string debugScriptName = "GridSpot";
    #endregion

    #region Static

    #endregion

    #region Public

    #endregion

    #region Private
    private State state = State.None;
    private GameObject xChild = null;
    private GameObject oChild = null;
    #endregion
    #endregion

    #region CustomFunction
    #region Static

    #endregion

    #region Public

    #endregion

    #region Private
    // Show the child X object.
    private void XClaimed()
    {
        PrintDebugMsg("X claimed this!");
        xChild.SetActive(true);
        state = State.X;
    }
    // Show the child O object.
    private void OClaimed()
    {
        PrintDebugMsg("O claimed this!");
        oChild.SetActive(true);
        state = State.O;
    }
    // Hide the child X and O objects.
    private void Hide()
    {
        PrintDebugMsg("Hiding...");

        xChild.SetActive(false);
        oChild.SetActive(false);
        state = State.None;
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
    public State State
    {
        get
        {
            return state;
        }
    }
    #endregion
    #endregion

    #region UnityFunctions
    // Find out if user clicked on a GridSpot. If so then call the proper function depending on whose turn it is.
    void OnMouseUp()
    {
        if (state == State.None && !GameController.singleton.GameEnded)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.transform.gameObject == this.gameObject)
            {
                PrintDebugMsg("Hit a GridSPot.");

                if (GameController.singleton.XTurn == true) XClaimed();
                else OClaimed();

                GameController.singleton.NextTurn();
            }
        }
    }
    #endregion

    #region Start_Update
    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        PrintDebugMsg("Loaded.");
    }
    // Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
    void Start()
    {
        xChild = (this.transform.GetChild(0).gameObject.name == "X") ? this.transform.GetChild(0).gameObject : this.transform.GetChild(1).gameObject;
        oChild = (this.transform.GetChild(1).gameObject.name == "O") ? this.transform.GetChild(1).gameObject : this.transform.GetChild(0).gameObject; ;
        Hide();
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