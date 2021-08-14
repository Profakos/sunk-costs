using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HotelManager : MonoBehaviour
{
	public HotelSizeData hotelSizeData;

	public RoomPreview preview;
	public int selectedRoomIndex = 0;
	public List<GameObject> roomTypes;
	public GameObject backRoomPrefab;

	public List<HotelRoom> hotelRooms = new List<HotelRoom>();
	public HashSet<Vector2> usedCoordinates = new HashSet<Vector2>();

	public Vector3 worldToHotelOffset;

	void Awake()
	{
		worldToHotelOffset = new Vector3(hotelSizeData.MinX, hotelSizeData.MinY, 0);

		preview = GameObject.Find("RoomPreview").gameObject.GetComponent<RoomPreview>();
		hotelSizeData.CurrentHotelHeight = hotelSizeData.InitialHotelHeight;
		NewFloor();
	}

    // Start is called before the first frame update
    void Start()
    {
        
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

		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			NewFloor();
		}

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			UpdatePreview(0);
		}

		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			UpdatePreview(1);
		}

		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			UpdatePreview(2);
		}

		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			UpdatePreview(3);
		}

		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			UpdatePreview(4);
		}
	}


	public void BuildRoom()
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
		HotelRoom newRoom = roomToBuild.GetComponent<HotelRoom>();

		if (!newRoom)
			return;

		hotelRooms.Add(newRoom);

		foreach(var coordinate in coordinatesToAdd)
		{
			usedCoordinates.Add(coordinate);
		}

	}

	public void NewFloor()
	{
		if (hotelSizeData != null)
		{

			if (hotelSizeData.MinY + hotelSizeData.CurrentHotelHeight + 1 > hotelSizeData.MaxY) return;

			hotelSizeData.CurrentHotelHeight += 1;
			
			if (backRoomPrefab != null)
			{
				Vector3 newBackRoomPos = new Vector3(backRoomPrefab.transform.position.x, hotelSizeData.MinY + hotelSizeData.CurrentHotelHeight - 1, 0);

				GameObject newBackRoom = Instantiate(backRoomPrefab, newBackRoomPos, backRoomPrefab.transform.rotation);
				 
			}

		}
	}

	public void UpdatePreview(int index)
	{
		selectedRoomIndex = index;
		
		if (roomTypes.Count < selectedRoomIndex) return;

		var roomToBuild = roomTypes[selectedRoomIndex];

		preview.UpdateSprite(roomToBuild.GetComponent<SpriteRenderer>());
	}
}
