using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class basicTime : MonoBehaviour
{
    // Start is called before the first frame update
    Transform transform;
    public float timespeed =  0.01f;
    void Start()
    {
        transform = gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(timespeed,0,0), Space.Self);

        if (transform.rotation.eulerAngles.x > 180){
            transform.localRotation = Quaternion.Euler(0,0,0);
        }

    }
}
