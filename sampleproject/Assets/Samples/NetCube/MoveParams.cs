using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct MoveParams : IComponentData
{
    public float HorizontalSpeed;
    public float VerticalSpeed;
}
