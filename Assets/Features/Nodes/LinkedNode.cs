﻿using System;
using System.Collections;
using UnityEngine;

public class LinkedNode : MonoBehaviour
{
    public int index;
    public GameObject[] linkedNodeObjects;
    public int[] linkedNodeIndexes;

    private void Start()
    {
        //Assign correct index for each linked Node
        linkedNodeIndexes = new int[linkedNodeObjects.Length];

        for(int i = 0; i < linkedNodeIndexes.Length; i++)
        {
            linkedNodeIndexes[i] = linkedNodeObjects[i].GetComponent<LinkedNode>().index;
        }
    }

    private void Update()
    {
        //DrawLineToLinkedNodes();
    }

    void DrawLineToLinkedNodes()
    {
        foreach (GameObject linkedNode in linkedNodeObjects)
        {
            Debug.DrawLine(transform.position, linkedNode.transform.position, Color.green);
        }
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawSphere(transform.position, 1);
    // }
    //
    // void OnDrawGizmosSelected()
    // {
    //     for (int i = 0; i < linkedNodeObjects.Length; i++)
    //     {
    //         Gizmos.color = Color.cyan;
    //         Gizmos.DrawSphere(linkedNodeObjects[i].transform.position, 1.5f);
    //     }
    // }
}
