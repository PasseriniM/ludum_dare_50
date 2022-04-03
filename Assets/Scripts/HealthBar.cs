using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider slider;
    private HealthScript health;

	private void Awake()
	{
		slider = GetComponent<Slider>();
		health = GetComponentInParent<HealthScript>();
	}

	private void LateUpdate()
	{
		slider.value = health.HealthPercent();
	}
}
