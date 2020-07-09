using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Control : MonoBehaviour {
    private Rigidbody2D Enemy_Instance;
    public GameObject Enemy;

    // Use this for initialization
    void Start () {
        Enemy_Instance = GetComponent<Rigidbody2D>();
       
    }
	
	// Update is called once per frame
	void Update () {
        if (Enemy_Instance.position[1]<-3)
        {
            Destroy(Enemy);
        }
	}
}
