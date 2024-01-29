using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    public int stoneID;
    [Header("ROUTES")]
    public Route commonRoute;   // Outer Route
    public Route finalRoute;

    public List<Node> fullRoute = new List<Node>();

    [Header("NODES")]
    public Node startNode;

    public Node baseNode;   // Node in home base
    public Node currentNode;
    public Node goalNode;

    int routePosition;   // Current position
    int startNodeIndex;
    int steps;  // Dice roll amount
    int doneSteps;

    [Header("BOOLS")]
    public bool isOut;  // In the game
    bool isMoving;

    bool hasTurn;   // For human input

    [Header("SELECTOR")]
    public GameObject selector;

    // Arc Movement
    float amplitude = 0.75f; // hight
    float cTime = 0.0f; // Track time it takes to move from one node to the next

    private void Start()
    {
        startNodeIndex = commonRoute.RequestPosition(startNode.gameObject.transform);
        CreateFullRoute();

        // Deactivate all selectors on board
        SetSelector(false);
    }

    /// <summary>
    /// Add board nodes to fullRoute
    /// </summary>
    private void CreateFullRoute()
    {
        for (int i = 0; i < commonRoute.childNodeList.Count; i++)
        {
            int tempPosition = startNodeIndex + i;
            tempPosition %= commonRoute.childNodeList.Count;

            fullRoute.Add(commonRoute.childNodeList[tempPosition].GetComponent<Node>());
        }

        for (int i = 0; i < finalRoute.childNodeList.Count; i++)
        {
            fullRoute.Add(finalRoute.childNodeList[i].GetComponent<Node>());
        }
    }

    /// <summary>
    /// Coroutine to move
    /// </summary>
    /// <returns></returns>
    IEnumerator Move(int diceNumber)
    {
        if (isMoving)
        {
            yield break;    // Stop the IEnumerator
        }

        isMoving = true;
        while (steps > 0)
        {
            routePosition++; // Increase current position

            // Get the next position
            Vector3 nextPosition = fullRoute[routePosition].gameObject.transform.position;
            // Player's current position
            Vector3 startPosition = fullRoute[routePosition - 1].gameObject.transform.position;
            /*while (MoveToNextNode(nextPosition, 8f))
            {
                yield return null;
            }//*/

            while (MoveInArcToNextNode(startPosition, nextPosition, 4.0f))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
            cTime = 0;  // Reset cTime
            steps--;
            doneSteps++;
        }
        goalNode = fullRoute[routePosition];

        // Check if possible kick = true
        if (goalNode.isTaken)
        {
            // Kick the other stone currently at the goalNode back to it's base
            goalNode.stone.ReturnToBase();
        }

        // Reset current node 
        currentNode.stone = null;
        currentNode.isTaken = false;

        // Set goal node stone to current player stone
        goalNode.stone = this;
        goalNode.isTaken = true;

        // Set current node to current goalNode = current player stone
        currentNode = goalNode;
        goalNode = null;    // Reset goal node

        // Report to GameManager
        // Perform a Wincondition check
        if (WinCondition())
        {
            GameManager.instance.ReportWinning();
        }

        // Switch player
        if (diceNumber < 6)
        {
            GameManager.instance.state = GameManager.States.SWITCH_PLAYER;
        }
        else
        {
            GameManager.instance.state = GameManager.States.ROLL_DICE;
        }

        isMoving = false;
    }


    /// <summary>
    /// Move current position towards goal position and then
    /// check if current positon = goal position
    /// </summary>
    /// <param name="goalPosition"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
    bool MoveToNextNode(Vector3 goalPosition, float speed)
    {
        return goalPosition != (transform.position = Vector3.MoveTowards(transform.position, goalPosition, speed * Time.deltaTime));
    }

    /// <summary>
    /// Create the arc movement of the player when moving between nodes
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="goalPosition"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
    bool MoveInArcToNextNode(Vector3 startPosition, Vector3 goalPosition, float speed)
    {
        cTime += speed * Time.deltaTime;    // The time it takes to move in a curve
        // Tracks player position between two points
        Vector3 myPosition = Vector3.Lerp(startPosition, goalPosition, cTime);

        // Creates the arc movment on the player's y postion
        myPosition.y = amplitude * Mathf.Sin(Mathf.Clamp01(cTime) * Mathf.PI);

        // Track the player position while moving in an arc
        // Then check if the goal position is equal to the player's position
        return goalPosition != (transform.position = Vector3.Lerp(transform.position, myPosition, cTime));
    }

    /// <summary>
    /// Get value of isOut
    /// </summary>
    /// <returns></returns>
    public bool ReturnIsOut()
    {
        return isOut;
    }

    /// <summary>
    /// 
    /// </summary>
    public void LeaveBase()
    {
        steps = 1;  // Move 1 step
        isOut = true;
        routePosition = 0;
        // Start Coroutine
        StartCoroutine(MoveOut());
    }

    /// <summary>
    /// Coroutine to move out of the base
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveOut()
    {
        if (isMoving)
        {
            yield break;    // Stop the IEnumerator
        }

        isMoving = true;
        while (steps > 0)   // Number of steps to move
        {
            //routePositon++; // Increase current position

            // Get the next position
            Vector3 nextPosition = fullRoute[routePosition].gameObject.transform.position;
            // Player's current position
            Vector3 startPosition = baseNode.gameObject.transform.position;
            /*while (MoveToNextNode(nextPosition, 8f))
            {
                yield return null;
            }//*/

            while (MoveInArcToNextNode(startPosition, nextPosition, 4.0f))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
            cTime = 0;
            steps--;
            doneSteps++;
        }
        // Update Node
        goalNode = fullRoute[routePosition];
        // Check for kicking other stone
        if (goalNode.isTaken)
        {
            // Return to start base node
            goalNode.stone.ReturnToBase();
        }

        goalNode.stone = this;
        goalNode.isTaken = true;

        currentNode = goalNode; // Store Node information to current node
        goalNode = null;

        // Report back to game manager to roll the dice
        GameManager.instance.state = GameManager.States.ROLL_DICE;
        isMoving = false;
    }

    /// <summary>
    /// Check for possible moves based on dice roll
    /// </summary>
    /// <param name="diceNumber"></param>
    /// <returns></returns>
    public bool CheckPossibleMove(int diceNumber)
    {
        int tempPosition = routePosition + diceNumber;   // Store possible position based on roll

        // Check that new position is not over the full route
        if (tempPosition >= fullRoute.Count)
        {
            return false;
        }
        // Return whether the new position is taken or not
        // if taken = true then return false = not possible move
        // if taken = false then return true = possible move
        return !fullRoute[tempPosition].isTaken;    
    }

    /// <summary>
    /// Check if can kick a stone based on new position from die roll
    /// </summary>
    /// <param name="stoneID"></param>
    /// <param name="diceNumber"></param>
    /// <returns></returns>
    public bool CheckPossibleKick(int stoneID, int diceNumber)
    {
        int tempPosition = routePosition + diceNumber;   // Store possible position based on roll

        // Check that new position is not over the full route
        if (tempPosition >= fullRoute.Count)
        {
            return false;
        }

        if (fullRoute[tempPosition].isTaken)
        {
            // Check if ID is the same as current player ID
            if (stoneID == fullRoute[tempPosition].stone.stoneID)
            {
                return false;
            }
            return true;    // Can kick stone
        }
        // new position is not taken
        return false;
    }

    /// <summary>
    /// Move current player stone
    /// </summary>
    /// <param name="diceNumber"></param>
    public void StartTheMove(int diceNumber)
    {
        steps = diceNumber;
        StartCoroutine(Move(diceNumber));
    }

    /// <summary>
    /// Send a Stone back to its base node
    /// </summary>
    public void ReturnToBase()
    {
        StartCoroutine(Return());
    }

    /// <summary>
    /// Coroutine to reset a stone's initial values
    /// </summary>
    /// <returns></returns>
    IEnumerator Return()
    {
        // Stop GameManager from making moves while returning to base
        GameManager.instance.ReportTurnPossible(false);
        routePosition = 0;
        currentNode = baseNode;
        goalNode = null;
        isOut = false;
        doneSteps = 0;

        // Initialize the movement back to base node
        Vector3 baseNodePosition = baseNode.transform.position;
        while(MoveToNextNode(baseNodePosition, 30.0f))
        {
            yield return null;
        }

        // Allow GameManager to make moves again
        GameManager.instance.ReportTurnPossible(true);

    }

    /// <summary>
    /// Check if player has won or not
    /// </summary>
    /// <returns></returns>
    bool WinCondition()
    {
        // Check the player's final route nodes
        for (int i = 0; i < finalRoute.childNodeList.Count; i++)
        {
            // Check if any of final route node is not taken
            if (!finalRoute.childNodeList[i].GetComponent<Node>().isTaken)
            {
                return false;
            }
        }
        return true;    // All node is taken
    }

    #region HUMAN INPUT

    /// <summary>
    /// Set selection on/off
    /// </summary>
    /// <param name="on"></param>
    public void SetSelector(bool on)
    {
        selector.SetActive(on);
        // Set player turn to have ability to click 
        hasTurn = on;
    }

    /// <summary>
    /// Activate a stone if player has a turn
    /// </summary>
    void OnMouseDown()
    {
        if (hasTurn)
        {
            // Check if stone is out
            if (!isOut)
            {
                LeaveBase();
            }
            else
            {
                StartTheMove(GameManager.instance.rolledHumanDice);
            }
            // Make it so that all other selectors are deactivated
            GameManager.instance.DeactivateAllSelectors();
        }
    }
    #endregion
}
