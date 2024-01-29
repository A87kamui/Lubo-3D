using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    /// <summary>
    /// Create a player entity
    /// </summary>
    [System.Serializable]
    public class Entity
    {
        public string playerName;
        public Stone[] myStones;
        public bool hasTurn;

        public enum PlayerTypes
        {
            HUMAN,
            CPU,
            NO_PLAYER
        }

        public PlayerTypes playerType;
        public bool hasWon;
    }

    public List<Entity> playerList = new List<Entity>();

    // STATEMACHINE
    public enum States
    {
        WAITING,
        ROLL_DICE,
        SWITCH_PLAYER
    }

    public States state;

    public int activePlayer;
    bool switchingPlayer;
    bool turnPossible = true;

    // HUMAN INPUTS
    // GAMEOBJECT FOR OUT BUTTON
    public GameObject rollButton;
    [HideInInspector] public int rolledHumanDice;

    public Dice dice;

    // Set instance to GameManager
    // Allows other scripts to access GameManager at any point
    private void Awake()
    {
        instance = this;

        for (int i = 0; i < playerList.Count; i++)
        {
            if (SaveSettings.players[i] == "HUMAN")
            {
                playerList[i].playerType = Entity.PlayerTypes.HUMAN;
            }
            if (SaveSettings.players[i] == "CPU")
            {
                playerList[i].playerType = Entity.PlayerTypes.CPU;
            }
        }
    }

    private void Start()
    {
        ActivateButton(false);

        int randomPlayer = Random.Range(0, playerList.Count);
        activePlayer = randomPlayer;
        Information.instance.ShowMessage(playerList[activePlayer].playerName + " starts first!");
    }

    /// <summary>
    /// Starts the game
    /// </summary>
    private void Update()
    {
        // CPU turn
        if (playerList[activePlayer].playerType == Entity.PlayerTypes.CPU)
        {
            switch (state)
            {
                case States.ROLL_DICE:
                    {
                        if (turnPossible)
                        {
                            StartCoroutine(RollDiceDelay());
                            state = States.WAITING; // Wait for dice roll to complete
                        }
                    }
                    break;
                case States.WAITING:
                    {
                        // Idle
                    }
                    break;
                case States.SWITCH_PLAYER:
                    {
                        if (turnPossible)
                        {
                            StartCoroutine(SwitchPlayer());
                            state = States.WAITING;
                        }
                    }
                    break;
            }
        }

        if (playerList[activePlayer].playerType == Entity.PlayerTypes.HUMAN)
        {
            switch (state)
            {
                case States.ROLL_DICE:
                    {
                        if (turnPossible)
                        {
                            // Deactivate Highlight
                            ActivateButton(true);
                            state = States.WAITING; // Wait for dice roll to complete
                        }
                    }
                    break;
                case States.WAITING:
                    {
                        // Idle
                    }
                    break;
                case States.SWITCH_PLAYER:
                    {
                        if (turnPossible)
                        {
                            // Deactivate Button
                            // Deactivate Highlights

                            StartCoroutine(SwitchPlayer());
                            state = States.WAITING;
                        }
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// CPU rolls dice
    /// </summary>
    void CPUDice()
    {
        dice.RollDice();
    }

    /// <summary>
    /// Dice Roll mechanic
    /// </summary>
    public void RollDice(int _diceNumber)   // Call from Dice.cs
    {
        int diceNumber = _diceNumber;

        if (playerList[activePlayer].playerType == Entity.PlayerTypes.CPU)
        {
            if (diceNumber == 6)    //Need to roll a 6 to leave base
            {
                // Check the start node is empty
                CheckStartNode(diceNumber);
            }
            if (diceNumber < 6)
            {
                // check for kick
                MoveAStone(diceNumber);
            }
        }

        if (playerList[activePlayer].playerType == Entity.PlayerTypes.HUMAN)
        {
            rolledHumanDice = _diceNumber;
            HumanRollDice() ;
        }
        Information.instance.ShowMessage(playerList[activePlayer].playerName + " has rolled " + diceNumber);
    }

    // Create a 2 sec delay then call RollDice method
    IEnumerator RollDiceDelay()
    {
        yield return new WaitForSeconds(2.0f);
        CPUDice();
    }

    /// <summary>
    /// Check if Starting node is occupied 
    /// </summary>
    /// <param name="diceNumber"></param>
    void CheckStartNode(int diceNumber)
    {
        // Is anyone on the start node
        bool startNodeFull = false;
        // Check all Stones to see if they are on the starting node
        for (int i = 0; i < playerList[activePlayer].myStones.Length; i++)
        {
            if (playerList[activePlayer].myStones[i].currentNode ==
                playerList[activePlayer].myStones[i].startNode)
            {
                startNodeFull = true;
                break; // Brake from for loop
            }
        }
        if (startNodeFull)
        {
            // Move a Stone
            MoveAStone(diceNumber);
            Debug.Log("Start Node is full");
        }
        else // StartNode is empty
        {
            // If at least a stone is inside the base
            for (int i = 0; i < playerList[activePlayer].myStones.Length; i++)
            {
                if (!playerList[activePlayer].myStones[i].ReturnIsOut())
                {
                    // Leave the base
                    playerList[activePlayer].myStones[i].LeaveBase();
                    // Then set state to waiting
                    state = States.WAITING;
                    return; // Leave the method
                }    
            }
            // Move a Stone
            MoveAStone(diceNumber);
        }
    }

    /// <summary>
    /// Check if new location has a stone to kick or
    /// is a position to move to
    /// </summary>
    /// <param name="diceNumber"></param>
    void MoveAStone(int diceNumber)
    {
        List<Stone> moveableStones = new List<Stone>(); // Keep track of stones that can move
        List<Stone> moveKickStones = new List<Stone>(); // Keep track of stones that are kicked

        // Fill the list
        for (int i = 0; i < playerList[activePlayer].myStones.Length; i++)
        {
            // Check if a stone is out of the base
            if (playerList[activePlayer].myStones[i].ReturnIsOut())
            {
                // Check if possible kick = true
                if (playerList[activePlayer].myStones[i].CheckPossibleKick(playerList[activePlayer].myStones[i].stoneID, diceNumber))
                {
                    moveKickStones.Add(playerList[activePlayer].myStones[i]);
                    continue;   // continue to next i number
                }

                // Check if possible move = true
                if (playerList[activePlayer].myStones[i].CheckPossibleMove(diceNumber))
                {
                    moveableStones.Add(playerList[activePlayer].myStones[i]);
                }
            }
        }

        // Perform Kick if possible
        if (moveKickStones.Count > 0)
        {
            int number = Random.Range(0, moveKickStones.Count);
            moveKickStones[number].StartTheMove(diceNumber);
            state = States.WAITING;
            return; // Stop here 
        }

        // Perform move if possible
        if (moveableStones.Count > 0)
        {
            int number = Random.Range(0, moveableStones.Count); // Randomly select a stone to moves
            moveableStones[number].StartTheMove(diceNumber);
            state = States.WAITING;
            return; // Stop here 
        }
        // None is possible
        // Switch the player
        state = States.SWITCH_PLAYER;
        Debug.Log("Switch player");
    }

    /// <summary>
    /// Create a delay when switching players
    /// </summary>
    /// <returns></returns>
    IEnumerator SwitchPlayer()
    {
        if (switchingPlayer)
        {
            yield break;
        }

        switchingPlayer = true;
        yield return new WaitForSeconds(2.0f);
        // Set next player
        SetNextActivePlayer();

        switchingPlayer = false;
    }

    /// <summary>
    /// Set the next player to keep playing until there is 1 player left
    /// who has not won
    /// </summary>
    void SetNextActivePlayer()
    {
        activePlayer++;
        activePlayer %= playerList.Count;

        int available = 0;
        // Check how many players have not won and available to play
        for (int i = 0; i < playerList.Count; i++)
        {
            if (!playerList[i].hasWon)
            {
                available++;
            }
        }

        // If there are still active players, call SetNextActivePlayer to increase active player count
        if (playerList[activePlayer].hasWon && available > 1)
        {
            SetNextActivePlayer();
            return;
        }
        else if (available < 2) // Only 1 player left available
        {
            // Game Over Screen
            SceneManager.LoadScene("Game Over");
            state = States.WAITING;
            return;
        }

        // Set current active player to roll dice
        Information.instance.ShowMessage(playerList[activePlayer].playerName + "'s turn!");
        state = States.ROLL_DICE;
    }

    /// <summary>
    /// Set turnPossible 
    /// </summary>
    public void ReportTurnPossible(bool possible)
    {
        turnPossible = possible;
    }

    /// <summary>
    /// Report if player has won
    /// </summary>
    public void ReportWinning()
    {
        // Show some UI
        playerList[activePlayer].hasWon = true;

        // Save the winners
        for (int i = 0; i < SaveSettings.winners.Length; i++)
        {
            if (SaveSettings.winners[i] == "")
            {
                SaveSettings.winners[i] = playerList[activePlayer].playerName;
                break;
            }
        }
    }

    //___________________________________
    #region HUMAN INPUT

    /// <summary>
    /// Activate button in game
    /// </summary>
    /// <param name="on"></param>
    void ActivateButton(bool on)
    {
        rollButton.SetActive(on);
    }

    /// <summary>
    /// Deactivate all selectors in game
    /// </summary>
    public void DeactivateAllSelectors()
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            for (int j = 0; j < playerList[i].myStones.Length; j++)
            {
                playerList[i].myStones[j].SetSelector(false);
            }
        }
    }

    /// <summary>
    /// Roll Dice for Human player on the roll button click
    /// </summary>
    public void HumanRoll()
    {
        dice.RollDice();
        ActivateButton(false);
    }

    /// <summary>
    /// Roll Dice function
    /// Gets list of stones that can move
    /// </summary>
    public void HumanRollDice()
    {
        // Roll Dice
        //rolledHumanDice = Random.Range(1, 7);
        Debug.Log("Human Rolled: " + rolledHumanDice);

        // Movable List
        List<Stone> moveableStones = new List<Stone>(); // Keep track of stones that can move

        // Fill the list
        /*for (int i = 0; i < playerList[activePlayer].myStones.Length; i++)
        {
            // Check if a stone is out of the base
            if (playerList[activePlayer].myStones[i].ReturnIsOut())
            {
                // Check if possible kick = true
                if (playerList[activePlayer].myStones[i].CheckPossibleKick(playerList[activePlayer].myStones[i].stoneID, rolledHumanDice))
                {
                    moveableStones.Add(playerList[activePlayer].myStones[i]);
                    continue;   // continue to next i number
                }

                // Check if possible move = true
                if (playerList[activePlayer].myStones[i].CheckPossibleMove(rolledHumanDice))
                {
                    moveableStones.Add(playerList[activePlayer].myStones[i]);
                }
            }
        } //*/

        // start Node full check
        // Is anyone on the start node
        bool startNodeFull = false;
        // Check all Stones to see if they are on the starting node
        for (int i = 0; i < playerList[activePlayer].myStones.Length; i++)
        {
            if (playerList[activePlayer].myStones[i].currentNode ==
                playerList[activePlayer].myStones[i].startNode)
            {
                startNodeFull = true;
                break; // Brake from for loop
            }
        }

        // Number rolled < 6
        if (rolledHumanDice < 6)
        {
            /*for (int i = 0; i < playerList[activePlayer].myStones.Length; i++)
            {
                // Make sure player is out already
                if (playerList[activePlayer].myStones[i].ReturnIsOut())
                {
                    if (playerList[activePlayer].myStones[i].CheckPossibleKick(playerList[activePlayer].myStones[i].stoneID, rolledHumanDice))
                    {
                        moveableStones.Add(playerList[activePlayer].myStones[i]);
                        continue;
                    }

                    if (playerList[activePlayer].myStones[i].CheckPossibleMove(rolledHumanDice))
                    {
                        moveableStones.Add(playerList[activePlayer].myStones[i]);
                    }
                }
            }//*/
            moveableStones.AddRange(PossibleStones());
        }

        // Number rolled == 6 and startNode is not filled
        if (rolledHumanDice == 6 && !startNodeFull)
        {
            Debug.Log("Rolled == 6 and start node is not full");
            // Inside base check for each player token
            for (int i = 0; i < playerList[activePlayer].myStones.Length; i++)
            {
                // Make sure player is out already
                if (!playerList[activePlayer].myStones[i].ReturnIsOut())
                {
                    moveableStones.Add(playerList[activePlayer].myStones[i]);
                }
            }

            // Outside check
            moveableStones.AddRange(PossibleStones());
        }
        // Number rolled == 6 and startNode is filled
        else if (rolledHumanDice == 6 && startNodeFull)
        {
            Debug.Log("Dice rolled == 6 and starNode is full");
            moveableStones.AddRange(PossibleStones());
        }

        // Activate all possible selectors
        if (moveableStones.Count > 0)
        {
            for (int i = 0; i < moveableStones.Count; i++)
            {
                moveableStones[i].SetSelector(true);
            }
        }
        else
        {
            state = States.SWITCH_PLAYER;
        }        
    }

    /// <summary>
    /// Creates a list of Stones that can take a turn
    /// </summary>
    /// <returns></returns>
    List<Stone> PossibleStones()
    {
        List<Stone> tempList = new List<Stone>();

        for (int i = 0; i < playerList[activePlayer].myStones.Length; i++)
        {
            // Make sure player is out already
            if (playerList[activePlayer].myStones[i].ReturnIsOut())
            {
                if (playerList[activePlayer].myStones[i].CheckPossibleKick(playerList[activePlayer].myStones[i].stoneID, rolledHumanDice))
                {
                    tempList.Add(playerList[activePlayer].myStones[i]);
                    continue;
                }

                if (playerList[activePlayer].myStones[i].CheckPossibleMove(rolledHumanDice))
                {
                    tempList.Add(playerList[activePlayer].myStones[i]);
                }
            }
        }

        return tempList;
    }
    #endregion
}
