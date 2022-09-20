using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour
{
    [SerializeField] int x;
    [SerializeField] int z;

    private float timer = 0;
    private float timeLimit = 0.1f;
    private bool hasBeenMoved = false;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(x, 0, z);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= timeLimit && !hasBeenMoved)
        {
            hasBeenMoved = true;
            transform.position = new Vector3(x, 0, z);
        }
    }
}
