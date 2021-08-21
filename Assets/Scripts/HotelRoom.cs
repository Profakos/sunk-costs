using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotelRoom : MonoBehaviour
{
	public RoomShapeData roomShape;

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Sink()
	{
		transform.Translate(0, -1f, 0);
	}
}
