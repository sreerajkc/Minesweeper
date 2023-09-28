using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private int spawnCount = 0;
    [SerializeField] private GameObject cubePref;
    [SerializeField] private float radius;

    // Start is called before the first frame update
    void Start()
    {
        Spawn();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Spawn()
    {
        float angle = 0;
        float angleDiff = 180f / (spawnCount - 1);

        Vector2 spawnPosition;

        for (int i = 0; i < spawnCount; i++)
        {
            spawnPosition = new Vector2(Mathf.Sin((angle - 90) * Mathf.Deg2Rad) , Mathf.Cos((angle - 90) * Mathf.Deg2Rad) ) * radius;
            Instantiate(cubePref, spawnPosition, Quaternion.identity);
            angle += angleDiff;
        }
    }
}
