using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class VectorExtensions
{
    public static Vector3 xy(this Vector3 v)
    {
        return new Vector3(v.x, v.y, 0f);
    }

    public static Vector3 xz(this Vector3 v)
    {
        return new Vector3(v.x, 0f, v.z);
    }

    public static Vector3 yz(this Vector3 v)
    {
        return new Vector3(0f, v.y, v.z);
    }

    public static Vector3 Multiply(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static Vector3 Divide(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    public static bool Exactly(this Vector3 v, Vector3 p)
    {
        return (v.x == p.x && v.y == p.y && v.z == p.z);
    }

    public static Vector3 Vec3(this Vector4 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public static float ParameterizedProjection(Vector3 start, Vector3 end, Vector3 p, bool clamp = false)
    {
        var a = p - start;
        var b = p - end;
        var c = end - start;

        var al = a.magnitude;
        var bl = b.magnitude;
        var cl2 = c.sqrMagnitude;
        var cl = Mathf.Sqrt(cl2);
        if (al == 0f) return 0f;
        if (bl == 0f) return 1f;
        if (cl == 0f) return 0f;

        var t = Vector3.Dot(a, c) / cl2;

        if (clamp)
        {
            t = Mathf.Clamp01(t);
        }

        return t;
    }

    public static Vector3 At(this Vector3 v, float t, Vector3 towards)
    {
        return v + (t * towards);
    }

    public static Vector3 ClosestOnLine(Vector3 start, Vector3 end, Vector3 p)
    {
        var t = ParameterizedProjection(start, end, p, true);
        return start.At(t, end - start);
    }

    public static float LineSegmentDistance(Vector3 start, Vector3 end, Vector3 p)
    {
        return (ClosestOnLine(start, end, p) - p).magnitude;
    }

    public static Vector2 Clamp(this Vector2 input, Vector2 min, Vector2 max)
    {
        return new Vector2(
            Mathf.Clamp(input.x, min.x, max.x),
            Mathf.Clamp(input.y, min.y, max.y));
    }

    public static Vector3 Clamp(this Vector3 input, Vector3 min, Vector3 max)
    {
        return new Vector3(
            Mathf.Clamp(input.x, min.x, max.x), 
            Mathf.Clamp(input.y, min.y, max.y), 
            Mathf.Clamp(input.z, min.z, max.z));
    }

    public static Vector4 Clamp(this Vector4 input, Vector4 min, Vector4 max)
    {
        return new Vector4(
            Mathf.Clamp(input.x, min.x, max.x),
            Mathf.Clamp(input.y, min.y, max.y),
            Mathf.Clamp(input.z, min.z, max.z),
            Mathf.Clamp(input.w, min.w, max.w));
    }

    public static Quaternion Quaternion(this Vector4 v)
    {
        return new UnityEngine.Quaternion(v.x, v.y, v.z, v.w);
    }

    public static byte[] ToBytes(this Vector2 v)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(v.x);
            bw.Write(v.y);

            return ms.ToArray();
        }
    }

    public static byte[] ToBytes(this Vector3 v)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(v.x);
            bw.Write(v.y);
            bw.Write(v.z);

            return ms.ToArray();
        }
    }

    public static byte[] ToBytes(this Vector4 v)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(v.x);
            bw.Write(v.y);
            bw.Write(v.z);
            bw.Write(v.w);

            return ms.ToArray();
        }
    }

    public static Vector2 Vec2FromBytes(byte[] buffer)
    {
        using (MemoryStream ms = new MemoryStream(buffer))
        using (BinaryReader br = new BinaryReader(ms))
        {
            return new Vector2(br.ReadSingle(), br.ReadSingle());
        }
    }

    public static Vector3 Vec3FromBytes(byte[] buffer)
    {
        using (MemoryStream ms = new MemoryStream(buffer))
        using (BinaryReader br = new BinaryReader(ms))
        {
            return new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }
    }

    public static Vector4 Vec4FromBytes(byte[] buffer)
    {
        using (MemoryStream ms = new MemoryStream(buffer))
        using (BinaryReader br = new BinaryReader(ms))
        {
            return new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }
    }

    public static void Assign<T>(this T[] values, T value)
    {
        for(int i = 0; i < values.Length; i++)
        {
            values[i] = value;
        }
    }
}
