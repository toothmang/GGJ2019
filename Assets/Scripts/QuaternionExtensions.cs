using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class QuaternionExtensions
{
    public static byte[] ToBytes(this Quaternion q)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(q.x);
            bw.Write(q.y);
            bw.Write(q.z);
            bw.Write(q.w);

            return ms.ToArray();
        }
    }

    public static Quaternion FromBytes(byte[] buffer)
    {
        using (MemoryStream ms = new MemoryStream(buffer))
        using (BinaryReader br = new BinaryReader(ms))
        {
            return new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }
    }

    public static bool Exactly(this Quaternion v, Quaternion p)
    {
        return v.x == p.x && v.y == p.y && v.z   == p.z && v.w == p.w;
    }

    public static Vector4 Vector4(this Quaternion q)
    {
        return new Vector4(q.x, q.y, q.z, q.w);
    }

    public static Quaternion Add(Quaternion a, Quaternion b)
    {
        return (a.Vector4() + b.Vector4()).Quaternion();
    }

    public static Quaternion Subtract(Quaternion a, Quaternion b)
    {
        return (a.Vector4() - b.Vector4()).Quaternion();
    }

    public static Quaternion zero = new Quaternion(0f, 0f, 0f, 0f);
}
