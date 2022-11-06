using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class b_PlayerMove : MonoBehaviour
{
    // Start is called before the first frame update

    public Queue<Vector3> LocationsToGo;
    Vector3 CurrentDest;

    public float speed = 2f;
    public Rigidbody rigidbody;
    void Start()
    {
        LocationsToGo = new Queue<Vector3>();
        int chunksize = GameObject.FindGameObjectWithTag("ChunkManager").GetComponent<ChunkManager>().ChunksPerSide;

        LocationsToGo.Enqueue(new Vector3(0,transform.position.y,0));
        LocationsToGo.Enqueue(new Vector3(chunksize * 240, transform.position.y,chunksize * 240));

        CurrentDest = LocationsToGo.Dequeue();
        

        
    }

    // Update is called once per frame
    void Update()
    {
        float dist = (CurrentDest - gameObject.transform.position).magnitude;
        if (dist < 4){
            LocationsToGo.Enqueue(CurrentDest);
            CurrentDest = LocationsToGo.Dequeue();
        }
    }

    void FixedUpdate(){
        Vector3 ToDest = (CurrentDest - gameObject.transform.position).normalized;
        ToDest *= speed;

        rigidbody.velocity = ToDest;

        rigidbody.angularVelocity = new Vector3(0, 0.2f, 0);
        
    }
}
