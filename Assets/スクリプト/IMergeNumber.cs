using UnityEngine;

public interface IMergeNumber
{
    int GetNumber();
    Transform transform { get; }
    GameObject gameObject { get; }
}