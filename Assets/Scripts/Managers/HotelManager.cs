using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
	
	public HotelSinkingTimer()
	{
	}

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

public class HotelManager : MonoBehaviour
{
	public HotelSizeData hotelSizeData;

	public RoomPreview preview;
	public int selectedRoomIndex = 0;
	public List<GameObject> roomTypes;
	public GameObject backRoomPrefab;

	public List<GameObject> hotelBackRooms = new List<GameObject>();
	public List<HotelRoom> hotelRooms = new List<HotelRoom>();
	public List<Vector2> usedCoordinates = new List<Vector2>();

	public Vector3 worldToHotelOffset;

	private int totalFloorCount = 0;
	private HotelSinkingTimer hotelSinkingTimer = new HotelSinkingTimer();

	private UnityEngine.UI.Image timerImage; 

	void Awake()
	{
		worldToHotelOffset = new Vector3(hotelSizeData.MinX, hotelSizeData.MinY, 0);

		timerImage = GameObject.Find("TimerImage").gameObject.GetComponent<UnityEngine.UI.Image>();

		preview = GameObject.Find("RoomPreview").gameObject.GetComponent<RoomPreview>();
		hotelSizeData.CurrentHotelHeight = hotelSizeData.InitialHotelHeight;
		NewFloor();
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
				BuildRoom();
			}
		}

		AdvanceSinkTimer();
		
	}

	private void AdvanceSinkTimer()
	{
		if (this.hotelSinkingTimer.CheckTimer(totalFloorCount, timerImage))
		{
			SinkHotel();
		};
		
	}

	private void BuildRoom()
	{
		if (roomTypes.Count < selectedRoomIndex) return;

		var roomToBuild = roomTypes[selectedRoomIndex];

		HotelRoom hotelRoom = roomToBuild.GetComponent<HotelRoom>();

		if (!hotelRoom)
			return;

		RoomShapeData roomShapeData = hotelRoom.roomShape;

		Vector3 hotelLocation = preview.transform.position - worldToHotelOffset;

		List<Vector3> coordinatesToAdd = new List<Vector3>();

		foreach (Vector3 offset in roomShapeData.OffsetFromRoomCenter)  {

			Vector3 coordinateToCheck = offset + hotelLocation;
			if (coordinateToCheck.x < 0) {
				Debug.Log("Too left");
				return;
			}

			if (coordinateToCheck.y < 0)
			{
				Debug.Log("Too low");
				return;
			}

			if (coordinateToCheck.x > hotelSizeData.MaxX - worldToHotelOffset.x)
			{
				Debug.Log("Too left");
				return;
			}

			if (coordinateToCheck.y >= hotelSizeData.CurrentHotelHeight)
			{
				Debug.Log("Too high");
				return;
			}

			if(usedCoordinates.Contains(coordinateToCheck))
			{ 
				Debug.Log("Location blocked");
				return;
			}

			coordinatesToAdd.Add(coordinateToCheck);
		}

		if (!roomToBuild) return;

		GameObject newRoomObject = Instantiate(roomToBuild, preview.transform.position, preview.transform.rotation);
		HotelRoom newRoom = newRoomObject.GetComponent<HotelRoom>();

		if (!newRoom)
			return;

		hotelRooms.Add(newRoom);

		foreach(var coordinate in coordinatesToAdd)
		{
			usedCoordinates.Add(coordinate);
		}

	}
	
	private void NewFloor()
	{
		if (hotelSizeData != null)
		{

			if (hotelSizeData.MinY + hotelSizeData.CurrentHotelHeight + 1 > hotelSizeData.MaxY) return;

			hotelSizeData.CurrentHotelHeight++;
			totalFloorCount++;
			
			if (backRoomPrefab != null)
			{
				Vector3 newBackRoomPos = new Vector3(backRoomPrefab.transform.position.x, hotelSizeData.MinY + hotelSizeData.CurrentHotelHeight - 1, 0);

				GameObject newBackRoom = Instantiate(backRoomPrefab, newBackRoomPos, backRoomPrefab.transform.rotation);

				hotelBackRooms.Add(newBackRoom);
			}

			if(!hotelSinkingTimer.TimerActive && hotelSizeData.CurrentHotelHeight > 1)
			{
				hotelSinkingTimer.TimerActive = true;
			}

			hotelSinkingTimer.CalculateSinkTimerTarget(totalFloorCount);

		}
	}
	
	private void SinkHotel()
	{

		for(int i = hotelBackRooms.Count - 1; i >=  0; i--)
		{
			var hotelBackroom = hotelBackRooms[i];
			hotelBackroom.transform.Translate(0, -1f, 0);
			if (hotelBackroom.transform.position.y < hotelSizeData.MinY)
			{
				hotelBackRooms.Remove(hotelBackroom);
				Destroy(hotelBackroom);
			}
		}
		 
		for (int i = hotelRooms.Count - 1; i >= 0; i--)
		{
			var hotelRoom = hotelRooms[i];
			hotelRoom.Sink();

			if(hotelRoom.Sunk)
			{
				hotelRooms.Remove(hotelRoom);
				Destroy(hotelRoom.gameObject);
			}

		}

		for (int i = usedCoordinates.Count - 1; i >= 0; i--)
		{
			var usedCoordinate = usedCoordinates[i];
			usedCoordinates.Remove(usedCoordinate);

			if(usedCoordinate.y > 0)
			{
				usedCoordinates.Add(new Vector2(usedCoordinate.x, usedCoordinate.y - 1));
			}
		}
		
		hotelSizeData.CurrentHotelHeight -= 1;
		
	}

	private void UpdatePreview(int index)
	{
		selectedRoomIndex = index;
		
		if (roomTypes.Count < selectedRoomIndex) return;

		var roomToBuild = roomTypes[selectedRoomIndex];

		preview.UpdateSprite(roomToBuild.GetComponent<SpriteRenderer>());
	}
}
