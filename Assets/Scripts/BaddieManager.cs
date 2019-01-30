using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieManager : MonoBehaviour {
    public int MaxEnemies = 4;

    public Vector2 RadiusBounds = new Vector2(2.0f, 10.0f);
    public Vector2 ScaleBounds = new Vector2(0.1f, 0.3f);
    public Vector2 FireRateBounds = new Vector2(0.1f, 2.0f);
    public Vector2 FireAccuracyBounds = new Vector2(0.9f, 0.99f);
    public Vector2 ProjectileSpeedBounds = new Vector2(5.0f, 20.0f);

    Camera mainCamera;

	// Use this for initialization
	void Start () {
        var mcGO = GameObject.Find("CameraMain");
        if (mcGO)
        {
            mainCamera = mcGO.GetComponent<Camera>();
        }
        else if (WebVRManager.Instance && WebVRManager.Instance.mainCamera)
        {
            mainCamera = WebVRManager.Instance.mainCamera;
        }
	}
	
	// Update is called once per frame
	void Update () {
		while (TurretBehavior.Instances.Count < MaxEnemies)
        {
            Vector3 spawnPos = Random.insideUnitSphere * Random.Range(RadiusBounds.x, RadiusBounds.y);

            if (spawnPos.y < transform.position.y)
            {
                spawnPos.y = transform.position.y + (transform.position.y - spawnPos.y);
                //spawnPos = Random.insideUnitSphere * Random.Range(RadiusBounds.x, RadiusBounds.y);
            }
            if (spawnPos.z < transform.position.z)
            {
                spawnPos.z = transform.position.z + (transform.position.z - spawnPos.z);
            }

            // Orient the turret so it's facing the target
            //var camPos = WebVRManager.Instance.mainCamera.transform.position;
            var camPos = mainCamera.transform.position;
            var spawnToCam = camPos - spawnPos;

            var newTurretGO = SpawnBank.Instance.TurretSpawner.FromPool(spawnPos, 
                Quaternion.identity, Vector3.one * Random.Range(ScaleBounds.x, ScaleBounds.y));

            newTurretGO.transform.rotation = Quaternion.FromToRotation(
                newTurretGO.transform.forward, spawnToCam.normalized);

            var newTurret = newTurretGO.GetComponent<TurretBehavior>();
            newTurret.HitPoints = 1;
            newTurret.FireAt = mainCamera.transform;
            newTurret.fireAccuracyPercent = Random.Range(FireAccuracyBounds.x, FireAccuracyBounds.y);
            newTurret.fireArc = (TurretBehavior.FireArc)Random.Range(0, 3);
            newTurret.movementMode = (TurretBehavior.MovementMode)Random.Range(0, 3);
            newTurret.refireDelay = Random.Range(FireRateBounds.x, FireRateBounds.y);
            newTurret.projectileSpeed = Random.Range(ProjectileSpeedBounds.x, ProjectileSpeedBounds.y);
            newTurret.Start();
            TurretBehavior.Instances.Add(newTurret);
        }
	}
}
