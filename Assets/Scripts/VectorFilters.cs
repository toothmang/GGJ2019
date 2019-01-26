using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class AverageOverFrames
{
    public List<Vector3> values;
    public Vector3 value;
    public List<float> weights;
    public int size = 0;
    public int index = 0;
    public bool useWeights = false;

    public AverageOverFrames(int s = 0)
    {
        size = s;
        values = new List<Vector3>(size);
    }

    public AverageOverFrames(List<float> _weights)
    {
        weights = _weights;
        useWeights = true;
        size = weights.Count;
        values = new List<Vector3>(size);
    }

    public void Clear()
    {
        for(int i = 0; i < values.Count; i++)
        {
            values[i] = Vector3.zero;   
        }
    }

    public void Update(Vector3 v)
    {
        if (values.Count < size)
        {
            values.Add(v);
        }
        else
        {
            values[index] = v;
        }
        index = (index + 1) % size;
        value = Vector3.zero;
        float w = 1.0f / (float)values.Count;

        for (int i = 0; i < values.Count; i++)
        {
            float wi = useWeights ? weights[i] : w;

            value += wi * values[i];
        }
    }
}

public class MaxMagnitudeInHistory
{
    public Queue<KeyValuePair<float, Vector3>> queue = new Queue<KeyValuePair<float, Vector3>>();
    public AverageOverFrames last = new AverageOverFrames(3);
    public Vector3 max = Vector3.zero;
    public float maxTime = 1.0f;
    public bool useCurrentDir = false;
    public bool averageCurrentDir = false;

    public MaxMagnitudeInHistory(float t = 1.0f, bool _currentDir = false, bool _avgDir = false, int _avgCount = 3)
    {
        maxTime = t;
        useCurrentDir = _currentDir;
        averageCurrentDir = _avgDir;
        last = new AverageOverFrames(_avgCount);
    }

    public void Update(Vector3 v)
    {
        float now = Time.time;

        if (queue.Any())
        {
            while (now - queue.First().Key >= maxTime)
            {
                queue.Dequeue();
                if (!queue.Any()) break;
            }
        }

        last.Update(v);

        queue.Enqueue(new KeyValuePair<float, Vector3>(now, v));

        float maxMag = 0.0f;

        Vector3 avg = Vector3.zero;

        float w = 1.0f / (float)queue.Count;

        foreach (var vt in queue)
        {
            avg += w * vt.Value;
            var vm = vt.Value.magnitude;
            if (vm > maxMag)
            {
                max = vt.Value;
                maxMag = vm;
            }
        }

        if (useCurrentDir)
        {
            max = (averageCurrentDir ? last.value.normalized : v.normalized) * maxMag;
        }
    }
}