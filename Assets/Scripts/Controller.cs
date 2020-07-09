using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : Enemy_Spawning {
    private Rigidbody2D rb;
    public float horizontal_speed = 5f;
    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        
 	}

 
    // Quaterion target = Quaterion.Euler(0, 0, 0);
    // Update is called once per frame
    void FixedUpdate () {

        float moveHorizontal = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(1 * moveHorizontal, 0);
    }
}
