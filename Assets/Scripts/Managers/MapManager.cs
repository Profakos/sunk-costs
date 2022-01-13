using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
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

	[Header("References")]
	/// <summary>
	/// The room preview highlight
	/// </summary>
	[SerializeField]
	private RoomPreview preview;

	[Header("Prefabs")]
	[SerializeField]
	private List<HotelRoom> regularRoomTypes;
	[SerializeField]
	private List<HotelRoom> luxuryRoomTypes;
	[SerializeField]
	private GameObject backRoomPrefab;

	/// <summary>
	/// The list of the currently existing backrooms
	/// </summary>
	private List<GameObject> hotelBackRooms = new List<GameObject>();
	/// <summary>
	/// The list of the currently existing hotel rooms
	/// </summary>
	private List<HotelRoom> hotelRooms = new List<HotelRoom>();
	/// <summary>
	/// The list of the currently used coordinates
	/// </summary>
	private List<Vector2> usedCoordinates = new List<Vector2>();

	/// <summary>
	/// The offset between the hotel and the world
	/// </summary>
	private Vector3 worldToHotelOffset;
	/// <summary>
	/// The room index of the currently selected room
	/// </summary>
	private int selectedRoomIndex = 0;

	public float FloorPurchasePrice { get => hotelStateData.FloorPurchasePrice; set => hotelStateData.FloorPurchasePrice = value; }
	public string FloorLabel { get => hotelStateData.FloorLabel; set => hotelStateData.FloorLabel = value; }
	public List<HotelRoom> RegularRoomTypes { get => regularRoomTypes; }
	public List<HotelRoom> LuxuryRoomTypes { get => luxuryRoomTypes; }
	public List<HotelRoom> HotelRooms { get => hotelRooms; }

	void Awake()
	{
		worldToHotelOffset = new Vector3(hotelSizeData.MinX, hotelSizeData.MinY, 0);
		
		hotelStateData.CurrentHotelHeight = hotelStateData.InitialHotelHeight;
		hotelStateData.TotalSpawnedFloors = (int)hotelStateData.InitialHotelHeight;
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
	public void BuildRoom(bool luxuriousSelected)
	{
		if (regularRoomTypes.Count < selectedRoomIndex) return;

		var roomToBuild = luxuriousSelected ? luxuryRoomTypes[selectedRoomIndex] : regularRoomTypes[selectedRoomIndex];

		HotelRoom hotelRoom = roomToBuild.GetComponent<HotelRoom>();

		if (!hotelRoom)
			return;

		float purchasePrice = hotelRoom.roomType.PurchasePrice * hotelRoom.LuxuryMultiplier;

		if (hotelStateData.Money < purchasePrice)
		{
			Debug.Log("Not enough money");
			return;
		}

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

			if (coordinateToCheck.y >= hotelStateData.CurrentHotelHeight)
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

		HotelRoom newRoom = Instantiate(roomToBuild, preview.transform.position, preview.transform.rotation);

		if (!newRoom)
			return;

		hotelRooms.Add(newRoom);

		hotelStateData.Money -= purchasePrice;

		foreach (var coordinate in coordinatesToAdd)
		{
			usedCoordinates.Add(coordinate);
		}

	}

	/// <summary>
	/// Formats the price of the new floor for the label
	/// </summary>
	/// <returns></returns>
	public string GetNewFloorPurchaseLabel()
	{
		return FloorLabel + ", $" + FloorPurchasePrice;
	}

	/// <summary>
	/// Creates a new floor if possible
	/// </summary>
	public void NewFloor()
	{
		if (hotelSizeData != null)
		{
			if (hotelSizeData.MinY + hotelStateData.CurrentHotelHeight + 1 > hotelSizeData.MaxY) return;

			hotelStateData.CurrentHotelHeight++;
			hotelStateData.TotalSpawnedFloors++;

			if (hotelStateData.Money < FloorPurchasePrice)
			{
				Debug.Log("Not enough money");
				return;
			}

			if (backRoomPrefab != null)
			{
				Vector3 newBackRoomPos = new Vector3(backRoomPrefab.transform.position.x, hotelSizeData.MinY + hotelStateData.CurrentHotelHeight - 1, 0);

				GameObject newBackRoom = Instantiate(backRoomPrefab, newBackRoomPos, backRoomPrefab.transform.rotation);

				hotelStateData.Money -= FloorPurchasePrice;
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

		if(hotelStateData.CurrentHotelHeight > 0)
			hotelStateData.CurrentHotelHeight -= 1;

	}

	/// <summary>
	/// Updates the selected room type
	/// </summary>
	/// <param name="index"></param>
	public void UpdatePreview(int index, bool luxuriousSelected)
	{
		selectedRoomIndex = index;

		if (regularRoomTypes.Count < selectedRoomIndex) return;

		var roomToBuild = luxuriousSelected? luxuryRoomTypes[selectedRoomIndex] : regularRoomTypes[selectedRoomIndex];

		preview.UpdateSprite(roomToBuild.GetComponent<SpriteRenderer>());
	}
}
