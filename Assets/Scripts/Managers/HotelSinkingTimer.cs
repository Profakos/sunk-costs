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
	public int TimerSpawnedFloorsCap { get; set; } = 100;
	
	/// <summary>
	/// Calculate the time until the next sink based on the current spawned floor count
	/// </summary>
	/// <param name="totalSpawnedFloors"></param>
	public void CalculateSinkTimerTarget(int totalSpawnedFloors)
	{
		timerTarget = minTimer + variableTimer * Mathf.Max(TimerSpawnedFloorsCap - totalSpawnedFloors, 0) / TimerSpawnedFloorsCap;

	}

	/// <summary>
	/// Check if the timer has ran out
	/// </summary>
	/// <param name="totalFloorCount"></param>
	/// <param name="timerImage"></param>
	/// <returns>Returns if the timer has ran out</returns>
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

	/// <summary>
	/// Sets the fillamount of the timer circle
	/// </summary>
	/// <param name="timerImage"></param>
	private void updateTimerImage(UnityEngine.UI.Image timerImage)
	{
		timerImage.fillAmount = timerCurrent / timerTarget;
	}
}
