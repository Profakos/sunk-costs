using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotelManager : MonoBehaviour
{

	public RoomPreview preview;

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
			preview.BuildRoom();
	}
}
