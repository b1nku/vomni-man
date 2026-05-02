using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomColor : MonoBehaviour
{
    [SerializeField] Material[] _colors;

    void Awake()
    {
        gameObject.GetComponent<Renderer>().material = _colors[Random.Range(0, _colors.Length)];
    }

    
}
