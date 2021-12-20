using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class Guest : MonoBehaviour
{
	public Vector2 EntrancePoint { get; set; }
	public Vector2 ExitPoint { get; set; }
	public Vector2 DespawnPoint { get; set; }
	public MapManager MapManager { get; set; }

	private Rigidbody2D rigidBody;
	private SpriteRenderer sprite;
	private TextMeshPro needsDisplayText;

	[SerializeField]
	private GuestActivity currentActivity;

	private float minimumTargetDistance = 0.02f;
	
	[SerializeField]
	private Vector2 target;
	private int speed = 2;
	private bool moving = true;
	
	private float durationPerNeed = 10f;

	[SerializeField]
	private Dictionary<NeedType, float> needs = new Dictionary<NeedType, float>();

	private float vacationTime = 30f;
	private float vacationBudget = 30f;

	private float currentPathfindCooldown = 0f;
	private const float pathfindCooldown = 0.5f;

	private HotelRoom currentRoom = null;
	private List<Vector2> shortestPath = null;

	void Awake()
	{
		rigidBody = gameObject.GetComponent<Rigidbody2D>();
		sprite = gameObject.GetComponent<SpriteRenderer>();
		needsDisplayText = gameObject.GetComponentInChildren<TextMeshPro>();
		needsDisplayText.SetText("");

		currentActivity = GuestActivity.Arriving;
		
	}

	// Start is called before the first frame update
	void Start()
	{
		target = EntrancePoint;

		SetupNeeds();

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
				if (!currentRoom.roomType.NeedTypesSatisfied.Intersect(needs.Keys).Any() || vacationBudget <= 0 || vacationTime <= 0)
				{

					if (moving)
					{
						if (distanceToTarget < minimumTargetDistance)
						{
							transform.position = target;
							moving = false;
						}
					}
					else if (IsAtRoomDoor())
					{
						shortestPath = null;
						if (needs.Any() && vacationBudget > 0 && vacationTime > 0)
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
						PathAndMoveTowardsTile(currentRoom.DoorOffset);
					}
				}
				else
				{
					float priceToPay = currentRoom.roomType.PricePerSecond * Time.deltaTime;

					if (vacationBudget < priceToPay) priceToPay = vacationBudget;

					vacationBudget -= priceToPay;
					MapManager.hotelStateData.Money += priceToPay;
					
					var needDecrease = Time.deltaTime * currentRoom.NeedFulfillingRate;

					foreach (NeedType needBeingFulfilled in currentRoom.roomType.NeedTypesSatisfied.Intersect(needs.Keys))
					{
						needs[needBeingFulfilled] -= needDecrease;
						if (needs[needBeingFulfilled] < 0) needs.Remove(needBeingFulfilled);
					}

					if (!moving)
					{
						if(currentPathfindCooldown <= 0)
						{ 
							// 20% chance to move around in room
							if (Random.Range(0, 5) == 0)
								MoveToNeighbouringTile();

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
					
				}
				break;
			case GuestActivity.Leaving:
				if (distanceToTarget < minimumTargetDistance)
				{
					Destroy(this.gameObject);
				}
				break;
			case GuestActivity.Waiting:

				TryFindRoom();

				break;
		}
		
		//ticks down once the hotel has been entered, even while waiting in the lobby
		if (currentActivity != GuestActivity.Arriving)
		{
			vacationTime -= Time.deltaTime;

		}
	}

	/// <summary>
	/// Finds the current room's offset that the guest is standing on
	/// </summary>
	/// <returns>The offset of the guest compared to the current room's pivot</returns>
	private Vector2 FindCurrentOffset()
	{
		return transform.position - currentRoom.transform.position;
	}

	/// <summary>
	/// Finds all the tiles that are one manhatten distance away from the guest
	/// </summary>
	/// <returns>All tiles neighbouring in the cardinal directions</returns>
	private List<Vector2> FindPossibleTilesToMoveTo()
	{
		Vector2 currentOffset = FindCurrentOffset();
		List<Vector2> possibleOffsets = new List<Vector2>(currentRoom.roomShape.GetGraph()[currentOffset]);

		return possibleOffsets;
	}

	/// <summary>
	/// Forces to leave the hotel
	/// </summary>
	public void ForceLeave()
	{
		needs.Clear();
		UpdateNeedsDisplay();
		BeginLeaving();
	}


	/// <summary>
	/// Is the guest at the door?
	/// </summary>
	/// <returns>boolean that displays if the guest is on the door's position</returns>
	private bool IsAtRoomDoor()
	{
		if (currentRoom == null) return true;

		return (Vector2) transform.position == (Vector2) currentRoom.transform.position + currentRoom.DoorOffset;
	}

	/// <summary>
	/// Moves around in the room, looking for a random neighbouring spot
	/// </summary>
	private void MoveToNeighbouringTile()
	{
		List<Vector2> possibleOffsets = FindPossibleTilesToMoveTo();

		if (possibleOffsets.Count == 0)
		{
			moving = false;
		}
		else
		{
			target = (Vector2) currentRoom.transform.position + possibleOffsets[Random.Range(0, possibleOffsets.Count)];
			moving = true;
		}
	}

	/// <summary>
	/// Finds the next tile to move towards 
	/// </summary>
	private void PathAndMoveTowardsTile(Vector2 targetOffset)
	{
		if(shortestPath == null)
		{
			Vector2 myOff = FindCurrentOffset();
			//Debug.Log("find path from "+myOff+" to "+targetOffset);
			shortestPath = currentRoom.GetShortestPath(myOff, targetOffset);
			//Debug.Log("path = "+string.Join(",", shortestPath.ToArray()));
		}
		// needs to have at least 2 targets in it, otherwise reached the end
		if(shortestPath.Count < 2)
		{
			moving = false;
			return;
		}
		// remove the reached target, and set the next
		shortestPath.RemoveAt(0);
		target = (Vector2) currentRoom.transform.position + shortestPath[0];
		moving = true;
	}

	/// <summary>
	/// The guest selects their desires
	/// </summary>
	private void SetupNeeds()
	{
		needs.Add(NeedType.Sleep, durationPerNeed);

		List<NeedType> possibleNeeds = System.Enum.GetValues(typeof(NeedType)).Cast<NeedType>().ToList();
		possibleNeeds.Remove(NeedType.Sleep);
		needs.Add(possibleNeeds[Random.Range(0, possibleNeeds.Count)], durationPerNeed);

		UpdateNeedsDisplay();
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
		&& !r.AtCapacity && needs.Keys.Intersect(r.roomType.NeedTypesSatisfied).Any());
		
		if (visitableRooms.Count == 0)
		{
			currentPathfindCooldown = pathfindCooldown;
			return;
		}

		int randomRoomIndex = Random.Range(0, visitableRooms.Count);
		roomToVisit = visitableRooms[randomRoomIndex];
		
		ChangeSortingLayer("GuestInRoom");

		transform.position = roomToVisit.DoorPosition();

		currentActivity = GuestActivity.Enjoying;
		
		ChangeRoom(roomToVisit);
		
	}

	/// <summary>
	/// Changes the room the guest is in
	/// </summary>
	/// <param name="newRoom"></param>
	private void ChangeRoom(HotelRoom newRoom)
	{
		if (currentRoom != null)
		{
			UpdateNeedsDisplay();
			currentRoom.GuestAmount--;
			currentRoom.UnsubscribeSink(HandleSinking);
		}

		currentRoom = newRoom;

		if (currentRoom != null)
		{
			currentRoom.GuestAmount++;
			currentRoom.SubscribeSink(HandleSinking);
		}

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
		if(currentRoom != null)
		{
			ChangeRoom(null);
			transform.position = ExitPoint;

			ChangeSortingLayer("GuestBehindHotel");

		}

		target = DespawnPoint;
		currentActivity = GuestActivity.Leaving;
		moving = true;
	} 

	private void ChangeSortingLayer(string layerName)
	{
		var sortingLayerId = SortingLayer.NameToID(layerName);
		sprite.sortingLayerID = sortingLayerId;
		needsDisplayText.sortingLayerID = sortingLayerId;
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

	/// <summary>
	/// Updates the need display over the guest
	/// </summary>
	private void UpdateNeedsDisplay()
	{
		string needsDisplay = "";

		foreach (var need in needs.Keys)
		{
			needsDisplay += " <sprite name=\"" + need.ToString() + "\">";
		}

		needsDisplayText.SetText(needsDisplay);
	}
}
