using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotelManager : MonoBehaviour
{
	public HotelSizeData hotelSizeData;

	public RoomPreview preview;
	public int selectedRoomIndex = 0;
	public List<GameObject> roomTypes;

	void Awake()
	{
		preview = GameObject.Find("RoomPreview").gameObject.GetComponent<RoomPreview>();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetMouseButtonDown(0))
			BuildRoom();

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			updatePreview(0);
		}

		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			updatePreview(1);
		}

		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			updatePreview(2);
		}

		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			updatePreview(3);
		}

		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			updatePreview(4);
		}
	}

	public void BuildRoom()
	{
		if (roomTypes.Count < selectedRoomIndex) return;

		var roomToBuild = roomTypes[selectedRoomIndex];

		if (roomToBuild != null)
			Instantiate(roomToBuild, preview.transform.position, preview.transform.rotation);
	}

	public void updatePreview(int index)
	{
		selectedRoomIndex = index;
		
		if (roomTypes.Count < selectedRoomIndex) return;

		var roomToBuild = roomTypes[selectedRoomIndex];

		preview.UpdateSprite(roomToBuild.GetComponent<SpriteRenderer>());
	}
}
