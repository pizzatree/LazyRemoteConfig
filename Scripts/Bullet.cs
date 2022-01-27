using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bullet : MonoBehaviour
{
    private float   moveSpeed;
    private Vector3 direction;
    
    public void Init(Vector3 direction, float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
        this.direction = direction;
    }

    private void Update() 
        => transform.Translate(direction * moveSpeed * Time.deltaTime);

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Game Over");
        SceneManager.LoadScene(0);
    }
}
