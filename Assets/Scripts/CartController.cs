using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CartController : MonoBehaviour
{
    CinemachineSplineCart _cart;
    public CinemachineSplineCart Cart { get { return _cart; }}

    void Awake()
    {
        _cart = GetComponent<CinemachineSplineCart>();
    }
}
