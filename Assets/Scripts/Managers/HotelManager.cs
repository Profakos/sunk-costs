using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HotelManager : MonoBehaviour
{
	public HotelSizeData hotelSizeData;
	public HotelStateData hotelStateData;

	private HotelSinkingTimer hotelSinkingTimer = new HotelSinkingTimer();
	private GuestManager guestManager;
	private MapManager mapManager;
	private GameObject debugButtonGroup;
	private GameObject gameplayButtonGroup;

	private Image timerImage;
	private TextMeshProUGUI moneyDisplay;
	
	void Awake()
	{
		debugButtonGroup = GameObject.Find("DebugButtonGroup");
		debugButtonGroup.SetActive(false);

		timerImage = GameObject.Find("TimerImage").GetComponent<UnityEngine.UI.Image>();
		moneyDisplay = GameObject.Find("MoneyDisplay").GetComponent<TextMeshProUGUI>();
		
		guestManager = gameObject.GetComponent<GuestManager>();
		mapManager = gameObject.GetComponent<MapManager>();

		hotelStateData.Money = 500;
		hotelStateData.moneyChangeHandler += UpdateMoneyDisplay;
		UpdateMoneyDisplay();

		gameplayButtonGroup = GameObject.Find("GameplayButtonGroup");

		Button[] buttons = gameplayButtonGroup.GetComponentsInChildren<Button>(true); 
		List<GameObject> roomObjects = mapManager.roomTypes;
		 
		for(int buttonIndex = 0, roomIndex = 0; buttonIndex < buttons.Length && roomIndex < roomObjects.Count; buttonIndex++, roomIndex++)
		{
			TextMeshProUGUI buttonText = buttons[buttonIndex].GetComponentInChildren<TextMeshProUGUI>();
			HotelRoom room = roomObjects[roomIndex].GetComponent<HotelRoom>();
			buttonText.SetText(room.GetPurchaseLabel());
		}

		TextMeshProUGUI newFloorButton = GameObject.Find("SelectFloor").GetComponentInChildren<TextMeshProUGUI>();
		newFloorButton.SetText(mapManager.GetNewFloorPurchaseLabel());
	}

	void OnDestroy()
	{
		hotelStateData.moneyChangeHandler -= UpdateMoneyDisplay;
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
		StartCoroutine(guestManager.SpawnGuests());
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
		if (hotelSinkingTimer.CheckTimer(hotelStateData.TotalSpawnedFloors, timerImage))
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

		if (!hotelSinkingTimer.TimerActive && hotelStateData.CurrentHotelHeight > 1)
		{
			hotelSinkingTimer.TimerActive = true;
		}

		hotelSinkingTimer.CalculateSinkTimerTarget(hotelStateData.TotalSpawnedFloors);
	}

	/// <summary>
	/// Updates the money display
	/// </summary>
	private void UpdateMoneyDisplay()
	{
		moneyDisplay.SetText("$" + (int)hotelStateData.Money);
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
