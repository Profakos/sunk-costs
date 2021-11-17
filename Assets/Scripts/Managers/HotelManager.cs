using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HotelManager : MonoBehaviour
{
	public HotelSizeData hotelSizeData;

	private HotelSinkingTimer hotelSinkingTimer = new HotelSinkingTimer();
	private GuestManager guestManager;
	private MapManager mapManager;
	private GameObject debugButtonGroup;

	private UnityEngine.UI.Image timerImage;
	
	void Awake()
	{
		debugButtonGroup = GameObject.Find("DebugButtonGroup").gameObject;
		debugButtonGroup.SetActive(false);

		timerImage = GameObject.Find("TimerImage").gameObject.GetComponent<UnityEngine.UI.Image>();

		guestManager = gameObject.GetComponent<GuestManager>();
		mapManager = gameObject.GetComponent<MapManager>();
	}

	// Start is called before the first frame update
	void Start()
	{
		timerImage.fillAmount = 0;
		
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			if(!EventSystem.current.IsPointerOverGameObject())
			{
				mapManager.BuildRoom();
			}
		}

		if (Input.GetKeyDown(KeyCode.Alpha0))
		{
			debugButtonGroup.SetActive(true);
		}

		AdvanceSinkTimer();
		
	}
	
	/// <summary>
	/// Forces the timer to flip over, no matter what
	/// </summary>
	public void DebugSinkNow()
	{
		mapManager.SinkHotel();
	}

	/// <summary>
	/// Spawns a guest
	/// </summary>
	public void DebugSpawnGuest()
	{
		guestManager.SpawnGuest();
	}

	/// <summary>
	/// Toggles sinking
	/// </summary>
	public void DebugToggleSink()
	{
		hotelSinkingTimer.TimerActive = !hotelSinkingTimer.TimerActive;
	}

	/// <summary>
	/// Deletes guests
	/// </summary>
	public void DebugDeleteGuests()
	{
		guestManager.DeleteGuests();
	}

	/// <summary>
	/// Forces guests to leave
	/// </summary>
	public void DebugForceGuestsLeave()
	{
		guestManager.ForceGuestsLeave();
	}

	/// <summary>
	/// Sinks the hotel one floor if the time has ran out, and resets the timer
	/// </summary>
	private void AdvanceSinkTimer()
	{
		if (hotelSinkingTimer.CheckTimer(mapManager.TotalSpawnedFloors, timerImage))
		{
			mapManager.SinkHotel();
		};
		
	}

	/// <summary>
	/// Handles pressing the button that a new floor
	/// </summary>
	public void NewFloorButton()
	{
		mapManager.NewFloor();

		if (!hotelSinkingTimer.TimerActive && hotelSizeData.CurrentHotelHeight > 1)
		{
			hotelSinkingTimer.TimerActive = true;
		}

		hotelSinkingTimer.CalculateSinkTimerTarget(mapManager.TotalSpawnedFloors);
	}

	/// <summary>
	/// Handless pressing the button that selects the room type to place
	/// </summary>
	/// <param name="index"></param>
	public void UpdatePreviewButton(int index)
	{
		mapManager.UpdatePreview(index);
	}
}
