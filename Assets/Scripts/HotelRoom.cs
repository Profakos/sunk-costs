using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotelRoom : MonoBehaviour
{
	public RoomShapeData roomShape;
	public HotelSizeData hotelSizeData;
	private bool sunk;
	private bool flooded;

	public SpriteRenderer spriteRenderer;

	public bool Sunk { get => sunk; set => sunk = value; }
	public bool Flooded { get => flooded; set => flooded = value; }

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
			flooded = true;
			spriteRenderer.color = Color.yellow;
		}
		
		if(floodedTiles == roomShape.OffsetFromRoomCenter.Length)
		{
			sunk = true;
		}

		if(sinkingHandler != null)
		sinkingHandler.Invoke(flooded || sunk);
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
