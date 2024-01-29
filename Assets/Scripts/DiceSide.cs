using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceSide : MonoBehaviour
{
    bool onGround;
    public int sideValue;

    /// <summary>
    /// Check if dice is on the ground
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            onGround = true;
        }
    }

    /// <summary>
    /// Check if dice is not on ground
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            onGround = false;
        }
    }

    /// <summary>
    /// get onGround value
    /// </summary>
    /// <returns></returns>
    public bool OnGround()
    {
        return onGround;
    }
}
