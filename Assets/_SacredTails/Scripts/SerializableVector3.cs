
using Newtonsoft.Json;
using System;
using UnityEngine;

[Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;

    [JsonConstructor]
    public SerializableVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public SerializableVector3(Vector3 vector3)
    {
        x = vector3.x;
        y = vector3.y;
        z = vector3.z;
    }
}

public static class Vector3Extensions
{
    public static Vector3 ToVector3(this SerializableVector3 serializedVector3)
    {
        return new Vector3(serializedVector3.x, serializedVector3.y, serializedVector3.z);
    }

    public static SerializableVector3 FromVector3(this Vector3 vector3)
    {
        return new SerializableVector3(vector3);
    }
}
