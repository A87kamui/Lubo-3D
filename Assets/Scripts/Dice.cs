using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    Rigidbody rb;
    Vector3 initialPosition;    // Dice starting position

    bool hasLanded;
    bool thrown;

    public DiceSide[] diceSides;
    public int diceValue;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;   // Store current position in start of the game
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Check dice is thrown, not moving (sleeping) and not landed
        if (rb.IsSleeping() && !hasLanded && thrown)
        {
            hasLanded = true;
            rb.useGravity = false;
            rb.isKinematic = true;   // So other objects cannot run into it

            // Side value check
            SideValueCheck();
        }
        else if (rb.IsSleeping() && hasLanded && diceValue == 0)
        {
            // Roll dice 
            RollAgain();
        }
    }

    /// <summary>
    /// Reroll dice
    /// </summary>
    void RollAgain()
    {
        ResetDice();
        thrown = true;
        rb.useGravity = true;
        rb.AddTorque(Random.Range(0, 500), Random.Range(0, 500), Random.Range(0, 500));
    }

    /// <summary>
    /// Dice roll mechanic
    /// </summary>
    public void RollDice()
    {
        ResetDice();
        if (!thrown && !hasLanded)
        {
            thrown = true;
            rb.useGravity = true;

            // Rotate the dice
            rb.AddTorque(Random.Range(0, 500), Random.Range(0, 500), Random.Range(0, 500));
        }
        else if (thrown && hasLanded)
        {
            // Reset Dice to initial position
            ResetDice();
        }
    }

    /// <summary>
    /// Set dice back into initial position
    /// </summary>
    void ResetDice()
    {
        transform.position = initialPosition;
        rb.isKinematic = false;
        thrown = false;
        hasLanded = false;
        rb.useGravity = false;
    }

    /// <summary>
    /// Check switch side of dice has landed and get the oppsite side value
    /// </summary>
    void SideValueCheck()
    {
        diceValue = 0;
        foreach(DiceSide side in diceSides)
        {
            if (side.OnGround())
            {
                diceValue = side.sideValue;
                // Send the side value to GameManager
                GameManager.instance.RollDice(diceValue);
            }
        }
    }
}
