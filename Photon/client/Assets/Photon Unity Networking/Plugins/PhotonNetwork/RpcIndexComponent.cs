using UnityEngine;
using System.Collections;

public class RpcIndexComponent : MonoBehaviour 
{
    public string[] RpcIndex;

    public void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public byte GetShortcut(string method)
    {
        for (int i = 0; i < RpcIndex.Length; i++)
        {
            string name = RpcIndex[i];
            if (name.Equals(method)) return (byte)i;
        }

        return byte.MaxValue;
    }
}
