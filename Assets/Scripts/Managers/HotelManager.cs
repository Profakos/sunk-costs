using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HotelManager : MonoBehaviour
{
	public HotelSizeData hotelSizeData;

	private HotelSinkingTimer hotelSinkingTimer = new HotelSinkingTimer();
	private MapManager mapManager;

	private UnityEngine.UI.Image timerImage;
	
	void Awake()
	{

		timerImage = GameObject.Find("TimerImage").gameObject.GetComponent<UnityEngine.UI.Image>();

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

		AdvanceSinkTimer();
		
	}

	private void AdvanceSinkTimer()
	{
		if (this.hotelSinkingTimer.CheckTimer(mapManager.TotalFloorCount, timerImage))
		{
			mapManager.SinkHotel();
		};
		
	}

	public void NewFloorButton()
	{
		mapManager.NewFloor();

		if (!hotelSinkingTimer.TimerActive && hotelSizeData.CurrentHotelHeight > 1)
		{
			hotelSinkingTimer.TimerActive = true;
		}

		hotelSinkingTimer.CalculateSinkTimerTarget(mapManager.TotalFloorCount);
	}

	public void UpdatePreviewButton(int index)
	{
		mapManager.UpdatePreview(index);
	}
}
