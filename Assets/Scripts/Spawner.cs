// https://docs.unity3d.com/Manual/UNetCustomSpawning.html
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Spawner))]
public class SpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Refresh Pool"))
        {
            (target as Spawner).RefreshPool();
        }

        if (GUILayout.Button("Delete Pool"))
        {
            var spawner = target as Spawner;

            foreach(var go in spawner.Pool)
            {
                DestroyImmediate(go);
            }
            spawner.Pool = new GameObject[spawner.ObjectPoolSize];
        }
    }
}
#endif

public class Spawner : MonoBehaviour
{
    public int ObjectPoolSize = 5;
    public GameObject SpawnPrefab;
    public GameObject[] Pool;
    //Dictionary<GameObject, float> SpawnTimes = new Dictionary<GameObject, float>();
    SortedDictionary<float, GameObject> ActivatedAt = new SortedDictionary<float, GameObject>();
    public bool keepDisabled = true;
    public Vector3 offset;
    int index = 0;

    public delegate GameObject SpawnDelegate(Vector3 position);
    public delegate void UnSpawnDelegate(GameObject spawned);

    void Start()
    {
        if (ObjectPoolSize != Pool.Length)
        {
            RefreshPool();
        }
    }

    public void RefreshPool()
    {
        Pool = new GameObject[ObjectPoolSize];
        ActivatedAt.Clear();
        for (int i = 0; i < ObjectPoolSize; ++i)
        {
            Pool[i] = (GameObject)Instantiate(SpawnPrefab, transform.position + offset, Quaternion.identity, transform);
            Pool[i].name = name + i;
            if (keepDisabled)
            {
                Pool[i].SetActive(false);
            }
        }

        index = 0;
    }

    public GameObject FromPool(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject result = null;
        if (keepDisabled)
        {
            foreach (var obj in Pool)
            {
                if (!obj.activeSelf)
                {
                    obj.SetActive(true);
                    result = obj;
                    break;
                }
            }
            // If we haven't returned from here, then try going with the oldest object
            if (!result && ActivatedAt.Any())
            {
                result = ActivatedAt.First().Value;
                
                //Debug.Log("No inactive objects, returning " + result.name);
            }
        }
        else
        {
            result = Pool[index];
            index = (index + 1) % Pool.Length;
        }

        if (result)
        {
            result.transform.position = position;
            result.transform.rotation = rotation;
            result.transform.localScale = scale;
            ActivatedAt[Time.time] = result;
        }
        else
        {
            Debug.LogError("Could not grab object from pool, nothing available");
        }

        return result;
    }

    public void UnSpawnObject(GameObject spawned)
    {
        if (spawned == null) return;

        //Debug.Log("Re-pooling object " + spawned.name);
        spawned.transform.parent = transform;
        spawned.transform.position = transform.position + offset;
        spawned.transform.rotation = transform.rotation;

        if (keepDisabled)
        {
            spawned.SetActive(false);
        }
    }
}