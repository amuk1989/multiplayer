using System;
using UnityEngine.UI;
using UnityEngine;

public class CounterView : MonoBehaviour
{
    [SerializeField] private Text text;

    private void Update()
    {
        text.text = $"Count: {Counter.Count}";
    }
}
