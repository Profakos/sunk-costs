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
	private GameObject luxuryRoomButtonGroup;
	private Button[] luxuryRoomButtons;
	private GameObject regularRoomButtonGroup;
	private Button[] regularRoomButtons;
	
	private bool luxuriousSelected = false;

	private Image timerImage;
	private TextMeshProUGUI moneyDisplay;
	private Image ratingImage;

	void Awake()
	{
		debugButtonGroup = GameObject.Find("DebugButtonGroup");
		debugButtonGroup.SetActive(false);

		timerImage = GameObject.Find("TimerImage").GetComponent<UnityEngine.UI.Image>();
		moneyDisplay = GameObject.Find("MoneyDisplay").GetComponent<TextMeshProUGUI>();
		ratingImage = GameObject.Find("HotelRatingFull").GetComponent<UnityEngine.UI.Image>();

		guestManager = gameObject.GetComponent<GuestManager>();
		mapManager = gameObject.GetComponent<MapManager>();

		hotelStateData.Money = 500;
		hotelStateData.moneyChangeHandler += UpdateMoneyDisplay;
		UpdateMoneyDisplay();
		hotelStateData.CurrentHotelRating = 4;
		hotelStateData.ratingChangeHandler += UpdateRatingDisplay;
		UpdateRatingDisplay();
		
		luxuryRoomButtonGroup = GameObject.Find("LuxuryRoomButtonGroup"); ;
		regularRoomButtonGroup = GameObject.Find("RegularRoomButtonGroup");
		
		SetupPurchaseButtons(luxuryRoomButtonGroup.GetComponentsInChildren<Button>(true), mapManager.luxuryRoomTypes, luxuryRoomButtonGroup, true);
		SetupPurchaseButtons(regularRoomButtonGroup.GetComponentsInChildren<Button>(true), mapManager.regularRoomTypes, regularRoomButtonGroup, false);

		TextMeshProUGUI newFloorButton = GameObject.Find("SelectFloor").GetComponentInChildren<TextMeshProUGUI>();
		newFloorButton.SetText(mapManager.GetNewFloorPurchaseLabel());
	}
	
	void OnDestroy()
	{
		hotelStateData.moneyChangeHandler -= UpdateMoneyDisplay;
		hotelStateData.ratingChangeHandler -= UpdateRatingDisplay;
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
				mapManager.BuildRoom(luxuriousSelected);
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
	/// Sets the text of the purchase buttons
	/// </summary>
	/// <param name="roomButtons"></param>
	/// <param name="roomObjects"></param>
	/// <param name="buttonGroup"></param>
	/// <param name="hide"></param>
	private void SetupPurchaseButtons(Button[] roomButtons, List<GameObject> roomObjects, GameObject buttonGroup, bool hide)
	{
		for (int buttonIndex = 0, roomIndex = 0; buttonIndex < roomButtons.Length && roomIndex < roomObjects.Count; buttonIndex++, roomIndex++)
		{
			TextMeshProUGUI buttonText = roomButtons[buttonIndex].GetComponentInChildren<TextMeshProUGUI>();
			HotelRoom room = roomObjects[roomIndex].GetComponent<HotelRoom>();
			buttonText.SetText(room.GetPurchaseLabel());
		}

		if (hide) buttonGroup.SetActive(false);
	}

	/// <summary>
	/// Updates the money display
	/// </summary>
	private void UpdateMoneyDisplay()
	{
		moneyDisplay.SetText("$" + (int)hotelStateData.Money);
	}

	/// <summary>
	/// Updates the rating display
	/// </summary>
	private void UpdateRatingDisplay()
	{
		ratingImage.fillAmount = hotelStateData.CurrentHotelRating / hotelStateData.MaxHotelRating;
	}

	/// <summary>
	/// Handless pressing the button that selects the room type to place
	/// </summary>
	/// <param name="index"></param>
	public void UpdatePreviewButton(int index)
	{
		mapManager.UpdatePreview(index, luxuriousSelected);
	}
	
	/// <summary>
	/// Selects if we want to see the regular or a luxurious room set
	/// TODO: make this per room type instead
	/// </summary>
	/// <param name="newLuxuriousSelectedValue"></param>
	public void SelectLuxuriousness(bool newLuxuriousSelectedValue)
	{
		luxuriousSelected = newLuxuriousSelectedValue;

		mapManager.UpdatePreview(0, luxuriousSelected);

		luxuryRoomButtonGroup.SetActive(luxuriousSelected);
		regularRoomButtonGroup.SetActive(!luxuriousSelected);
	}
}
