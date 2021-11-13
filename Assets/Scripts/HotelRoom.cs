using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotelRoom : MonoBehaviour
{
	public RoomShapeData roomShape;
	public HotelSizeData hotelSizeData;

	[SerializeField]
	private int capacity;
	[SerializeField]
	private int guestAmount;
	public Vector2 doorOffset;

	public int Capacity { get => capacity; } 
	public bool AtCapacity => guestAmount >= Capacity;

	public SpriteRenderer spriteRenderer;

	public bool Sunk { get; set; }
	public bool Flooded { get; set; }
	public int GuestAmount { get => guestAmount; set => guestAmount = value; }

	public delegate void SinkingDelegate(bool floodedOrSunk);
	public event SinkingDelegate sinkingHandler;

	void Awake()
	{
		spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
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
	/// Returns the position of the room's door
	/// </summary>
	/// <returns></returns>
	public Vector2 DoorPosition()
	{
		return (Vector2)gameObject.transform.position + doorOffset;
	}
	
	/// <summary>
	/// Sinks the room one tile
	/// </summary>
	public void Sink()
	{
		transform.Translate(0, -1f, 0);

		int floodedTiles = 0;

		foreach(Vector2 offset in roomShape.OffsetFromRoomCenter)
		{
			if( (transform.position.y + offset.y) < hotelSizeData.MinY)
			{
				floodedTiles++;
			}
		}

		if(floodedTiles > 0 )
		{
			Flooded = true;
			spriteRenderer.color = Color.yellow;
		}
		
		if(floodedTiles == roomShape.OffsetFromRoomCenter.Length)
		{
			Sunk = true;
		}

		if(sinkingHandler != null)
		sinkingHandler.Invoke(Flooded || Sunk);
	}

	public void SubscribeSink(SinkingDelegate e)
	{
		sinkingHandler += e;
	}

	public void UnsubscribeSink(SinkingDelegate e)
	{
		if (sinkingHandler == null) return;
		sinkingHandler -= e;
	}
	
}
