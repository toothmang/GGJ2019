using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AudioEvents : Singleton<AudioEvents>
{
    public AudioClip[] HitClips;

    public AudioSource[] Sources;

    public AudioListener listener;
    int sourcesUsed = 0;


    public float timeBetweenPlays = 0.25f;
    float lastPlayed = 0f;
    bool readyToPlay = false;

    SortedDictionary<float, Vector3> playPositions = new SortedDictionary<float, Vector3>();

    public static int RandomClip()
    {
        return UnityEngine.Random.Range(0, Instance.HitClips.Length);
    }

    public static void Play(AudioSource src, int? index = null, float relativeSpeed = 0.0f)
    {
        if (src && !src.isPlaying)
        {
            src.clip = Instance.HitClips[index ?? RandomClip()];
            if (relativeSpeed != 0.0f)
            {
                src.pitch = Mathf.Clamp(relativeSpeed, 0.25f, 2.0f);
            }
            else
            {
                src.pitch = 1.0f;
            }
            src.Play();
        }
    }

    public static void PlayAt(Vector3 position, int? index = null, float relativeSpeed = 0.0f)
    {
        if (Instance.readyToPlay)
        {
            var diff = Instance.listener.transform.position - position;
            Instance.playPositions[diff.magnitude] = position;
        }
        
        //Instance.playPositions.Add(diff.magnitude, position);
    }

    static long Frame = 0;

    private void LateUpdate()
    {
        sourcesUsed = 0;
        if (playPositions.Count > 0 && readyToPlay)
        {
            foreach(var pp in playPositions)
            {
                if (sourcesUsed == Sources.Length) break;

                var src = Sources[sourcesUsed];
                src.clip = Instance.HitClips[RandomClip()];
                src.gameObject.transform.position = pp.Value;
                if (src.isPlaying)
                {
                    src.Stop();
                }
                src.Play();

                sourcesUsed++;
            }
            
            playPositions.Clear();

            lastPlayed = Time.time;
        }

        readyToPlay = Time.time - lastPlayed > timeBetweenPlays;

        Frame++;
    }
}
