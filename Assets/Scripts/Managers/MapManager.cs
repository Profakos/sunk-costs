using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
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

	public int TotalSpawnedFloors { get; set; } = 0;

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

	}
	
	/// <summary>
	/// Creates the selected room type at the current location, if possible
	/// </summary>
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

		foreach (Vector3 offset in roomShapeData.OffsetFromRoomCenter)
		{

			Vector3 coordinateToCheck = offset + hotelLocation;
			if (coordinateToCheck.x < 0)
			{
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

			if (usedCoordinates.Contains(coordinateToCheck))
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

		foreach (var coordinate in coordinatesToAdd)
		{
			usedCoordinates.Add(coordinate);
		}

	}

	/// <summary>
	/// Creates a new floor if possible
	/// </summary>
	public void NewFloor()
	{
		if (hotelSizeData != null)
		{
			if (hotelSizeData.MinY + hotelSizeData.CurrentHotelHeight + 1 > hotelSizeData.MaxY) return;

			hotelSizeData.CurrentHotelHeight++;
			TotalSpawnedFloors++;

			if (backRoomPrefab != null)
			{
				Vector3 newBackRoomPos = new Vector3(backRoomPrefab.transform.position.x, hotelSizeData.MinY + hotelSizeData.CurrentHotelHeight - 1, 0);

				GameObject newBackRoom = Instantiate(backRoomPrefab, newBackRoomPos, backRoomPrefab.transform.rotation);

				hotelBackRooms.Add(newBackRoom);
			}
			
		}
	}

	/// <summary>
	/// Sinks the hotel by one floor
	/// </summary>
	public void SinkHotel()
	{

		for (int i = hotelBackRooms.Count - 1; i >= 0; i--)
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

			if (hotelRoom.Sunk)
			{
				hotelRooms.Remove(hotelRoom);
				Destroy(hotelRoom.gameObject);
			}

		}

		for (int i = usedCoordinates.Count - 1; i >= 0; i--)
		{
			var usedCoordinate = usedCoordinates[i];
			usedCoordinates.Remove(usedCoordinate);

			if (usedCoordinate.y > 0)
			{
				usedCoordinates.Add(new Vector2(usedCoordinate.x, usedCoordinate.y - 1));
			}
		}

		hotelSizeData.CurrentHotelHeight -= 1;

	}

	/// <summary>
	/// Updates the selected room type
	/// </summary>
	/// <param name="index"></param>
	public void UpdatePreview(int index)
	{
		selectedRoomIndex = index;

		if (roomTypes.Count < selectedRoomIndex) return;

		var roomToBuild = roomTypes[selectedRoomIndex];

		preview.UpdateSprite(roomToBuild.GetComponent<SpriteRenderer>());
	}
}
