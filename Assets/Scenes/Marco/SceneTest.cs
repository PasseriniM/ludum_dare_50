using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTest : MonoBehaviour
{
    public HealthScript unit;
	public float damage = 20;

	private void Update()
	{
		if (Input.GetKey(KeyCode.Space))
		{
			unit.ApplyDamage(damage * Time.deltaTime);
		}
	}
}
