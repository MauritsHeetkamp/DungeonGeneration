using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public int[] sortArray;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Sort();
        }
    }
    public void Sort()
    {
        bool swapped = false;
        int currentToCheck = 0;
        while (currentToCheck < sortArray.Length - 1)
        {
            if (sortArray[currentToCheck] > sortArray[currentToCheck + 1])
            {
                int backupStackObject = sortArray[currentToCheck + 1];
                sortArray[currentToCheck + 1] = sortArray[currentToCheck];
                sortArray[currentToCheck] = backupStackObject;
                swapped = true;
            }
            currentToCheck++;
        }
        if (swapped)
        {
            Sort();
        }
    }
}
