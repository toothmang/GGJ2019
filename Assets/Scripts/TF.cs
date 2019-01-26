using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[Serializable]
public struct TF
{
    [Flags]
    public enum Fields
    {
        None = 0,
        Position = 1,
        Rotation = 2,
        Scale = 4,
        Parent = 8,
        All = Position | Rotation | Scale | Parent
    }

    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public uint parent;

    public static uint GetParentID(Transform tf)
    {
        if (tf.parent == null)
        {
            return 0;
        }

        //if (IDBag.Instance.ObjectIDs.ContainsKey(tf.parent.gameObject))
        //{
        //    return IDBag.Instance.ObjectIDs[tf.parent.gameObject];
        //}

        else
        {
            return 0;
        }
    }

    public static TF identity = new TF
    {
        position = Vector3.zero,
        rotation = Quaternion.identity,
        scale = Vector3.one,
        parent = 0
    };

    public TF(Transform tf)
    {
        position = tf.localPosition;
        rotation = tf.localRotation;
        scale = tf.localScale;
        parent = GetParentID(tf);
    }

    //public TF(BinaryReader br)
    //{
    //    position = br.ReadVector3();
    //    rotation = br.ReadQuaternion();
    //    scale = br.ReadVector3();
    //    parent = br.ReadUInt32();
    //}

    //public void Write(BinaryWriter bw)
    //{
    //    bw.Write(position);
    //    bw.Write(rotation);
    //    bw.Write(scale);
    //    bw.Write(parent);
    //}

    public void Update(Transform tf)
    {
        position = tf.localPosition;
        rotation = tf.localRotation;
        scale = tf.localScale;
        parent = GetParentID(tf);
    }

    public void Apply(Transform tf)
    {
        tf.localPosition = position;
        tf.localRotation = rotation;
        tf.localScale = scale;
    }

    public Fields Diff(TF tf)
    {
        Fields fields = Fields.None;

        if (!position.Exactly(tf.position))
        {
            fields |= Fields.Position;
        }
        if (!rotation.Exactly(tf.rotation))
        {
            fields |= Fields.Rotation;
        }
        if (!scale.Exactly(tf.scale))
        {
            fields |= Fields.Scale;
        }
        if (parent != tf.parent)
        {
            fields |= Fields.Parent;
        }

        return fields;
    }

    public static TF Lerp(TF a, TF b, float t)
    {
        return new TF
        {
            position = Vector3.Lerp(a.position, b.position, t),
            rotation = Quaternion.Slerp(a.rotation, b.rotation, t),
            scale = Vector3.Lerp(a.scale, b.scale, t)
        };
    }
}