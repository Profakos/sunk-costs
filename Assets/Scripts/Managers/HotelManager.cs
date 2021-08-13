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

	void Awake()
	{
		preview = GameObject.Find("RoomPreview").gameObject.GetComponent<RoomPreview>();
		hotelSizeData.CurrentHotelHeight = 1;
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

		if (roomToBuild != null)
			Instantiate(roomToBuild, preview.transform.position, preview.transform.rotation);
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
