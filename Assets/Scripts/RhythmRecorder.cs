using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmRecorder : MonoBehaviour
{
    public static RhythmRecorder Instance;
    public Color hitColor;
    public Color replayColor;
    public float hitGlowTime = 0.25f;
    public float lastHitTimer = 5.0f;
    public bool loopRhythm = true;
    // Time to wait after the nth shot
    List<float> hits = new List<float>();
    bool hitsComputed = false;

    Material material;
    Vector3 ogScale;
    Color ogColor;

    public delegate void BeatHit();
    public event BeatHit OnHit;

	// Use this for initialization
	void Start () {
        var mr = GetComponent<MeshRenderer>();
        if (mr)
        {
            material = mr.material;
            ogColor = material.color;
        }

        ogScale = transform.localScale;
        Instance = this;

        OnHit += () => { StartCoroutine(Hit(replayColor)); };
    }
	
	// Update is called once per frame
	void Update () {
        if (hits.Count > 0 && !hitsComputed && (Time.unscaledTime - hits[hits.Count - 1]) > lastHitTimer)
        {
            ComputeHits();
            StartCoroutine(PlayHits());
        }

        string output = "\n";
        for (int j = 0; j < hits.Count; j++)
        {
            output += string.Format("hit {0}: {1}\n", j, hits[j]);
        }
        Console.Instance.extra = output;
    }

    void AddHit(float hitTime)
    {
        if (hitsComputed)
        {
            StopCoroutine(PlayHits());
            hits.Clear();
            hitsComputed = false;
        }
        hits.Add(hitTime);
    }

    void ComputeHits()
    {
        var firstHit = hits[0];
        for(int i = 0; i < hits.Count; i++)
        {
            hits[i] = hits[i] - firstHit;
        }
        hitsComputed = true;
    }

    IEnumerator Hit(Color startColor)
    {
        // Stops any other instances of this coroutine
        StopCoroutine("Hit");

        material.color = ogColor;
        transform.localScale = ogScale;
        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);

        Vector3 startScale = ogScale * 1.1f;

        for(float t = 0.0f; t < hitGlowTime; t += Time.unscaledDeltaTime)
        {
            float it = t / hitGlowTime;
            material.color = Color.Lerp(startColor, ogColor, it);
            transform.localScale = Vector3.Lerp(startScale, ogScale, it);
            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        }
        material.color = ogColor;
        transform.localScale = ogScale;
    }

    IEnumerator PlayHits()
    {
        do
        {
            float startTime = Time.unscaledTime;
            for (int i = 0; i < hits.Count; i++)
            {
                if (i >= hits.Count) yield break;
                while ((Time.unscaledTime - startTime) < hits[i])
                {
                    yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
                }

                OnHit();
            }

            yield return new WaitForSecondsRealtime(2.0f);

        } while (loopRhythm);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hands")
        {
            AddHit(Time.unscaledTime);
            StartCoroutine(Hit(hitColor));
        }
    }
}
