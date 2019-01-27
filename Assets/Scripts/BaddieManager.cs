using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieManager : MonoBehaviour {
    public int MaxEnemies = 4;

    public Vector2 RadiusBounds = new Vector2(2.0f, 10.0f);
    public Vector2 ScaleBounds = new Vector2(0.1f, 0.3f);
    public Vector2 FireRateBounds = new Vector2(0.1f, 2.0f);
    public Vector2 FireAccuracyBounds = new Vector2(0.9f, 0.99f);

    public Transform AimFor;

	// Use this for initialization
	void Start () {
		
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

            Quaternion spawnRot = Quaternion.FromToRotation(Vector3.forward, (new Vector3(spawnPos.x, transform.position.y, spawnPos.z) - transform.position).normalized);

            var newTurretGO = SpawnBank.Instance.TurretSpawner.FromPool(spawnPos, spawnRot, Vector3.one * Random.Range(ScaleBounds.x, ScaleBounds.y));
            var newTurret = newTurretGO.GetComponent<TurretBehavior>();
            newTurret.HitPoints = 1;
            newTurret.FireAt = AimFor;

            newTurret.fireAccuracyPercent = Random.Range(FireAccuracyBounds.x, FireAccuracyBounds.y);
            newTurret.fireArc = (TurretBehavior.FireArc)Random.Range(0, 2);
            newTurret.fireMode = (TurretBehavior.FireMode)Random.Range(0, 3);
            newTurret.movementMode = (TurretBehavior.MovementMode)Random.Range(0, 3);
            newTurret.refireDelay = Random.Range(FireRateBounds.x, FireRateBounds.y);
            newTurret.Start();
            TurretBehavior.Instances.Add(newTurret);
        }
	}

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, RadiusBounds.x);
        Gizmos.DrawWireSphere(transform.position, RadiusBounds.y);
    }
}
