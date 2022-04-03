using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerCustom : MonoBehaviour
{
	public Text TimerText;
	private float timer;

	void Update()
	{
		timer += Time.deltaTime;
		int minutes = Mathf.FloorToInt(timer / 60F);
		int seconds = Mathf.FloorToInt(timer % 60F);
		int milliseconds = Mathf.FloorToInt((timer * 100F) % 100F);
		TimerText.text = minutes.ToString("00") + ":" + seconds.ToString("00") + ":" + milliseconds.ToString("00");
	}
}