using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HotelManager : MonoBehaviour
{

	[Header("ScriptableObjects")]
	/// <summary>
	/// SO containing the hotel's dimensions
	/// </summary>
	[SerializeField]
	private HotelSizeData hotelSizeData;

	/// <summary>
	/// SO containing the hotel's current stats
	/// </summary>
	[SerializeField]
	private HotelStateData hotelStateData;


	[Header("Managers")]
	/// <summary>
	/// Manager that handles the spawning of guests
	/// </summary>
	[SerializeField]
	private GuestManager guestManager = null;

	/// <summary>
	/// Manager that handles the creation of rooms
	/// </summary>
	[SerializeField]
	private MapManager mapManager = null;
	
	/// <summary>
	/// Elastic timer that manages the hotel's sinking
	/// </summary>
	private HotelSinkingTimer hotelSinkingTimer = new HotelSinkingTimer();
	
	[Header("UI elements")]
	[SerializeField]
	private GameObject debugButtonGroup = null;
	[SerializeField]
	private GameObject luxuryRoomButtonGroup = null;
	[SerializeField]
	private GameObject regularRoomButtonGroup = null;
	[SerializeField]
	private Button newFloorButton = null;
	[SerializeField]
	private Image timerImage = null;
	[SerializeField]
	private TextMeshProUGUI moneyDisplay = null;
	[SerializeField]
	private Image ratingImage = null;
	
	/// <summary>
	/// Toggled by selecting the desired building menu
	/// </summary>
	private bool luxuriousSelected = false;

	void Awake()
	{
		Screen.SetResolution(1600, 900, false);

		debugButtonGroup.SetActive(false);
		
		hotelStateData.Money = 500;
		hotelStateData.MoneyChangeHandler += UpdateMoneyDisplay;

		hotelStateData.CurrentHotelRating = 0;
		hotelStateData.RatingChangeHandler += UpdateRatingDisplay;

		List<float> initReviews = new List<float>();
		for (int i = 0; i < hotelStateData.MaxReviewRemembered; i++) initReviews.Add(1);
		hotelStateData.AddReviews(initReviews);
		
		SetupPurchaseButtons(luxuryRoomButtonGroup.GetComponentsInChildren<Button>(true), mapManager.LuxuryRoomTypes, luxuryRoomButtonGroup, true);
		SetupPurchaseButtons(regularRoomButtonGroup.GetComponentsInChildren<Button>(true), mapManager.RegularRoomTypes, regularRoomButtonGroup, false);
		
		newFloorButton.GetComponentInChildren<TextMeshProUGUI>().SetText(mapManager.GetNewFloorPurchaseLabel());
	}
	
	void OnDestroy()
	{
		hotelStateData.MoneyChangeHandler -= UpdateMoneyDisplay;
		hotelStateData.RatingChangeHandler -= UpdateRatingDisplay;
	}

	// Start is called before the first frame update
	void Start()
	{
		timerImage.fillAmount = 0;

		UpdateMoneyDisplay();
		UpdateRatingDisplay();
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
	private void SetupPurchaseButtons(Button[] roomButtons, List<HotelRoom> roomObjects, GameObject buttonGroup, bool hide)
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
		ratingImage.fillAmount = hotelStateData.CurrentHotelRatingPercentage;
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
