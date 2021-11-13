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

	private float minimumTargetDistance = 0.02f;
	
	[SerializeField]
	private Vector2 target;
	private int speed = 2;
	private bool moving = true;
	private float enjoyTimePerRoom = 10f;
	private float enjoyTimeLeft = 0f;
	private int numOfRoomsToVisit = 2;

	private float currentPathfindCooldown = 0f;
	private const float pathfindCooldown = 0.5f;

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
					if(moving)
					{
						if (distanceToTarget < minimumTargetDistance)
						{
							transform.position = target;
							moving = false;
						}
					}
					else if (isAtRoomDoor())
					{
						if (numOfRoomsToVisit > 0)
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
						target = (Vector2)currentRoom.transform.position + currentRoom.doorOffset;
						moving = true;
					}
				}
				else
				{
					if(!moving)
					{
						if(currentPathfindCooldown <= 0)
						{ 
							// 20% chance to move around in room
							if (Random.Range(0, 5) == 0)
								MoveAroundInRoom();

							currentPathfindCooldown = pathfindCooldown;
						}
						else
						{
							currentPathfindCooldown -= Time.deltaTime;
						}
						 
					}
					else
					{
						if (distanceToTarget < minimumTargetDistance)
						{
							transform.position = target;	
							moving = false;
						}
					}

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
	/// Is the guest at the door?
	/// </summary>
	/// <returns></returns>
	private bool isAtRoomDoor()
	{
		if (currentRoom == null) return true;

		return (Vector2) gameObject.transform.position == (Vector2) currentRoom.transform.position + currentRoom.doorOffset;
	}

	/// <summary>
	/// Moves around in the room, looking for a random spot
	/// </summary>
	private void MoveAroundInRoom()
	{
		Vector2 currentOffset = transform.position - currentRoom.transform.position;
		
		List<Vector2> possibleDirs = new List<Vector2>();

		foreach(Vector2 v in currentRoom.roomShape.OffsetFromRoomCenter)
		{
			
			if (Vector2.Distance(v, currentOffset) == 1f)
			{
				possibleDirs.Add(v);
			}
		}

		if (possibleDirs.Count == 0)
		{
			moving = false;
		}
		else
		{
			target = (Vector2) currentRoom.transform.position + possibleDirs[Random.Range(0, possibleDirs.Count)];
			moving = true;
		}
	}

	/// <summary>
	/// Tries to find a room to stay in, returns if it is successful
	/// </summary>
	private void TryFindRoom()
	{		

		if(currentPathfindCooldown > 0f)
		{
			currentPathfindCooldown -= Time.deltaTime;
			return;
		}

		HotelRoom roomToVisit = null;

		List<HotelRoom> visitableRooms = MapManager.hotelRooms.FindAll(r => !r.Flooded && !r.Sunk && r != currentRoom
		&& !r.AtCapacity);

		if(visitableRooms.Count > 0)
		{
			int randomRoomIndex = Random.Range(0, visitableRooms.Count);
			roomToVisit = visitableRooms[randomRoomIndex];
		}
		else
		{
			currentPathfindCooldown = pathfindCooldown;
		}
		
		if (roomToVisit != null)
		{
			sprite.sortingLayerID = SortingLayer.NameToID("GuestInRoom");

			transform.position = (Vector2)roomToVisit.transform.position;

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

		target += new Vector2(0, -1);

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
