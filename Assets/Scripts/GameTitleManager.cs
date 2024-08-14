using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTitleManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        var sinValue = Mathf.Sin(Time.time * 10);
        float size = 0.90f + ((sinValue + 1.0f) / 2.0f) * 0.10f;
        this.transform.localScale = size * Vector3.one;
    }
}
