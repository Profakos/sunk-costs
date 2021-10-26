using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guest : MonoBehaviour
{
	public Vector2 EntrancePoint { get; set; }
	public Vector2 ExitPoint { get; set; }
	public Vector2 DespawnPoint { get; set; }
	public MapManager MapManager { get; set; }

	private Rigidbody2D rigidBody;
	private SpriteRenderer sprite;


	[SerializeField]
	private GuestActivity currentActivity;

	private float minimumTargetDistance = 0.01f;
	
	[SerializeField]
	private Vector2 target;
	private int speed = 2;
	private bool moving = true;
	private float enjoyTimePerRoom = 10f;
	private float enjoyTimeLeft = 0f;
	private int numOfRoomsToVisit = 2;

	private HotelRoom currentRoom = null;

	void Awake()
	{
		rigidBody = gameObject.GetComponent<Rigidbody2D>();
		sprite = gameObject.GetComponent<SpriteRenderer>();
		currentActivity = GuestActivity.Arriving;
	}

	// Start is called before the first frame update
	void Start()
	{
		target = EntrancePoint;

	}
	
	// Update is called once per frame
	void Update()
	{

		if(transform.position.x >= DespawnPoint.x)
		{
			Destroy(this.gameObject);
			return;
		}

		ChangeBehaviour();
	}
	
	private void FixedUpdate()
	{
		if(moving)
		MoveGuest(target);
	}

	private void OnDestroy()
	{
		if (currentRoom != null)
		{
			currentRoom.GuestAmount--;
			currentRoom.UnsubscribeSink(HandleSinking);
		}
	}

	/// <summary>
	/// Changes the current guest behaviour
	/// </summary>
	private void ChangeBehaviour()
	{
		float distanceToTarget = (target - (Vector2)transform.position).magnitude;

		switch (currentActivity)
		{
			case GuestActivity.Arriving:
				if (distanceToTarget < minimumTargetDistance)
				{
					FinishArriving();
				}
				break;
			case GuestActivity.Enjoying:
				if (enjoyTimeLeft <= 0)
				{
					if(numOfRoomsToVisit > 0)
					{
						TryFindRoom();
					}
					else
					{
						BeginLeaving();
					}
				}
				else
				{
					enjoyTimeLeft -= Time.deltaTime;
				}
				break;
			case GuestActivity.Leaving:
				break;
			case GuestActivity.Waiting:

				TryFindRoom();

				break;
		}
	}

	/// <summary>
	/// Tries to find a room to stay in, returns if it is successful
	/// </summary>
	private void TryFindRoom()
	{
		HotelRoom roomToVisit = null;

		List<HotelRoom> visitableRooms = MapManager.hotelRooms.FindAll(r => !r.Flooded && !r.Sunk && r != currentRoom
		&& !r.AtCapacity);

		if(visitableRooms.Count > 0)
		{
			int randomRoomIndex = Random.Range(0, visitableRooms.Count);
			roomToVisit = visitableRooms[randomRoomIndex];
		}
		
		if (roomToVisit != null)
		{
			sprite.sortingLayerID = SortingLayer.NameToID("GuestInRoom");

			int offsetIndex = Random.Range(0, roomToVisit.roomShape.OffsetFromRoomCenter.Length);

			transform.position = (Vector2)roomToVisit.transform.position + roomToVisit.roomShape.OffsetFromRoomCenter[offsetIndex];

			currentActivity = GuestActivity.Enjoying;
			enjoyTimeLeft = enjoyTimePerRoom;

			ChangeRoom(roomToVisit);

			numOfRoomsToVisit -= 1;
		}
		
	}

	/// <summary>
	/// Changes the room the guest is in
	/// </summary>
	/// <param name="newRoom"></param>
	private void ChangeRoom(HotelRoom newRoom)
	{
		if (currentRoom != null)
		{
			currentRoom.GuestAmount--;
			currentRoom.UnsubscribeSink(HandleSinking);
		}

			currentRoom = newRoom;
			currentRoom.GuestAmount++;

		if (currentRoom != null)
			currentRoom.SubscribeSink(HandleSinking);

	}

	/// <summary>
	/// Signs up to the room's sinking delegate
	/// </summary>
	/// <param name="floodedOrSunk"></param>
	private void HandleSinking(bool floodedOrSunk)
	{
		if(!floodedOrSunk)
		{
			transform.Translate(0, -1f, 0);
		}
		else
		{
			Destroy(this.gameObject);
		}
	}

	/// <summary>
	/// The guest starts to leave the hotel
	/// </summary>
	private void BeginLeaving()
	{
		transform.position = ExitPoint;
		sprite.sortingLayerID = SortingLayer.NameToID("GuestBehindHotel");
		target = DespawnPoint;
		currentActivity = GuestActivity.Leaving;
		moving = true;
	} 

	/// <summary>
	/// Finishes arriving to the hotel
	/// </summary>
	private void FinishArriving()
	{
		moving = false;
		currentActivity = GuestActivity.Waiting;
	}

	/// <summary>
	/// Moves the guest towards the target position
	/// </summary>
	/// <param name="target"></param>
	private void MoveGuest(Vector2 target)
	{
		Vector2 dir = target - (Vector2)transform.position;
		dir.Normalize();
		rigidBody.MovePosition((Vector2)transform.position + dir * speed * Time.deltaTime);
	}


}
