using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HotelSinkingTimer
{
	private float timerCurrent = 0f;
	private float timerTarget = 0f;
	private bool timerActive = false;

	private float minTimer = 5f;
	private float variableTimer = 15f;

	public float TimerCurrent { get => timerCurrent; set => timerCurrent = value; }
	public float TimerTarget { get => timerTarget; set => timerTarget = value; }
	public bool TimerActive { get => timerActive; set => timerActive = value; }
	public int TimerFloorCountCap { get; set; } = 100;
	
	public void CalculateSinkTimerTarget(int totalFloorCount)
	{
		timerTarget = minTimer + variableTimer * Mathf.Max(TimerFloorCountCap - totalFloorCount, 0) / TimerFloorCountCap;

	}

	public bool CheckTimer(int totalFloorCount, UnityEngine.UI.Image timerImage)
	{
		if (!timerActive) return false;

		timerCurrent += Time.deltaTime;

		if (timerCurrent >= timerTarget)
		{
			timerCurrent = 0f;
			CalculateSinkTimerTarget(totalFloorCount);
			updateTimerImage(timerImage);
			return true;
		}
		else
		{
			updateTimerImage(timerImage);
			return false;
		}
	}

	private void updateTimerImage(UnityEngine.UI.Image timerImage)
	{
		timerImage.fillAmount = timerCurrent / timerTarget;
	}
}