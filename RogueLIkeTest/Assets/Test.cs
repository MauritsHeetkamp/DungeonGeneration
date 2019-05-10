using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject yes;

    // Update is called once per frame
    public void Start()
    {
        GameObject neww = Instantiate(yes);
        print(Physics.OverlapBox(neww.transform.position, neww.GetComponent<BoxCollider>().size / 2, neww.transform.rotation).Length);
    }
}
