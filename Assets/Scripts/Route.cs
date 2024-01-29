using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Route : MonoBehaviour
{
    Transform[] childNodes;
    public List<Transform> childNodeList = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        FillNodes();
    }

    /// <summary>
    /// Visualize the nodes connection
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green; // conneciton color 
        FillNodes();

        for (int i = 0; i < childNodeList.Count; i++)
        {
            Vector3 currentPosition = childNodeList[i].position;
            if (i > 0)
            {
                Vector3 previousPostion = childNodeList[i-1].position;
                Gizmos.DrawLine(previousPostion, currentPosition);
            }
        }
    }

    /// <summary>
    /// Fill list with nodes
    /// </summary>
    void FillNodes()
    {
        childNodeList.Clear();

        childNodes = GetComponentsInChildren<Transform>();

        // Add all nodes that is not the parent object
        foreach(Transform child in childNodes)
        {
            if (child != this.transform)    // This = Parent
            {
                childNodeList.Add(child);
            }
        }
    }

    /// <summary>
    /// Get index of node based on its transfrom position
    /// </summary>
    /// <param name="nodeTransfrom"></param>
    /// <returns></returns>
    public int RequestPosition(Transform nodeTransfrom)
    {
        return childNodeList.IndexOf(nodeTransfrom);
    }
}
