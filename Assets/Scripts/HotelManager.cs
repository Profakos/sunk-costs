using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotelManager : MonoBehaviour
{

	public GameObject preview;

	void Awake()
	{
		preview = GameObject.Find("RoomPreview").gameObject;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
