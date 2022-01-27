using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bullet;
    
    private float     spawnBufferTime;
    private float     bulletSpeedTarget;
    
    private Transform player;
    
    private void Start()
    {
        player = FindObjectOfType<Player>().transform;
        
        spawnBufferTime   = Settings.SpawnBuffer;
        bulletSpeedTarget = Settings.BulletSpeed;
        InvokeRepeating(nameof(SpawnBullet), 1, spawnBufferTime);
    }

    private void SpawnBullet()
    {
        Vector3 pos       = Random.insideUnitCircle * 100f;
        var     dir       = (player.position - pos).normalized;
        var     moveSpeed = Random.Range(bulletSpeedTarget * .75f, bulletSpeedTarget * 1.25f);
        
        var newBullet = Instantiate(bullet, pos, Quaternion.identity).GetComponent<Bullet>();
        newBullet.Init(dir, moveSpeed);
    }
}
