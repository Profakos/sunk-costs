using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.Rendering;

public class Guest : MonoBehaviour
{
	[Header("ScriptableObjects")]
	/// <summary>
	/// SO containing the hotel's current stats
	/// </summary>
	[SerializeField]
	private HotelStateData hotelStateData;

	/// <summary>
	/// Guest data scriptable object
	/// </summary>
	[SerializeField]
	private GuestData guestData = null;

	private Rigidbody2D rigidBody;
	private SpriteRenderer sprite;
	private SortingGroup sortingGroup;
	
	private SpriteRenderer guaranteedNeedSprite;
	private SpriteRenderer randomNeedSprite;
	private SpriteRenderer guestSadFaceSprite;

	[Header("Activity tracking")]
	[SerializeField]
	private GuestActivity currentActivity;
	[SerializeField]
	private Vector2 target;

	private float speed = 2f;
	private bool moving = true;
	
	private Dictionary<NeedType, float> needs = new Dictionary<NeedType, float>();
	private Dictionary<NeedType, NeedData> needDatas = new Dictionary<NeedType, NeedData>();

	private float luxuryMultiplier = 1;

	[SerializeField]
	private float currentVacationTime;
	[SerializeField]
	private float currentVacationBudget;
	[SerializeField]
	private float vacationTime;
	[SerializeField]
	private float vacationBudget;

	private const float waitingBuffer = 10f;
	private const float pathfindCooldown = 0.5f;

	private float currentPathfindCooldown = 0f;

	private HotelRoom currentRoom = null;
	private List<Vector2> shortestPath = null;

	private float totalWaitingTime = 0f;
	private float totalWrongLuxuryTime = 0f;

	public Vector2 EntrancePoint { get; set; }
	public Vector2 ExitPoint { get; set; }
	public Vector2 DespawnPoint { get; set; }
	public MapManager MapManager { get; set; }
	public float LuxuryMultiplier { get => luxuryMultiplier; set => luxuryMultiplier = value; }
	public float CurrentVacationTime { get => currentVacationTime; set => currentVacationTime = value; }
	public float CurrentVacationBudget { get => currentVacationBudget; set => currentVacationBudget = value; }
	public float CurrentPathfindCooldown { get => currentPathfindCooldown; set => currentPathfindCooldown = value; }
	public float PathfindCooldown => pathfindCooldown;
	public float WaitingBuffer => waitingBuffer;
	public float VacationTime { get => vacationTime; set => vacationTime = value; }
	public float VacationBudget { get => vacationBudget; set => vacationBudget = value; }
	public float TotalWrongLuxuryTime { get => totalWrongLuxuryTime; set => totalWrongLuxuryTime = value; }
	public float TotalWaitingTime { get => totalWaitingTime; set => totalWaitingTime = value; }

	void Awake()
	{
		rigidBody = gameObject.GetComponent<Rigidbody2D>();
		sprite = gameObject.GetComponent<SpriteRenderer>();
		sortingGroup = gameObject.GetComponent<SortingGroup>();

		guaranteedNeedSprite = gameObject.transform.Find("GuaranteedNeed").GetComponent<SpriteRenderer>();
		randomNeedSprite = gameObject.transform.Find("RandomNeed").GetComponent<SpriteRenderer>();
		guestSadFaceSprite = gameObject.transform.Find("GuestSadFace").GetComponent<SpriteRenderer>();
		guestSadFaceSprite.enabled = false;

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
				if (distanceToTarget < guestData.MinimumTargetDistance)
				{
					FinishArriving();
				}
				break;
			case GuestActivity.Enjoying:
				if (IsAllNeedsSatisfiedByRoom() || VacationBudget <= 0 || CurrentVacationTime <= 0)
				{

					if (moving)
					{
						if (distanceToTarget < guestData.MinimumTargetDistance)
						{
							transform.position = target;
							moving = false;
						}
					}
					else if (IsAtRoomDoor())
					{
						shortestPath = null;
						if (needs.Count > 0 && VacationBudget > 0 && CurrentVacationTime > 0)
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
					float priceToPay = hotelStateData.RoomRentPerSecond * currentRoom.LuxuryMultiplier * Time.deltaTime;

					if (VacationBudget < priceToPay) priceToPay = VacationBudget;

					VacationBudget -= priceToPay;
					hotelStateData.Money += priceToPay;

					TotalWrongLuxuryTime += Time.deltaTime;

					var needDecrease = Time.deltaTime * currentRoom.NeedFulfillingRate;

					foreach (NeedType needBeingFulfilled in currentRoom.roomType.NeedTypesSatisfied)
					{
						if (!needs.ContainsKey(needBeingFulfilled)) continue;

						needs[needBeingFulfilled] -= needDecrease;
						if (needs[needBeingFulfilled] < 0) {
							needs.Remove(needBeingFulfilled);
							needDatas.Remove(needBeingFulfilled);

							if (needBeingFulfilled == guestData.GuaranteedNeed.NeedType)
							{
								guaranteedNeedSprite.enabled = false;
							}
							else
							{
								randomNeedSprite.enabled = false;
							}
						};
					}

					if (!moving)
					{
						if(currentPathfindCooldown <= 0)
						{ 
							// 20% chance to move around in room
							if (Random.Range(0, 5) == 0)
								MoveToNeighbouringTile();

							currentPathfindCooldown = PathfindCooldown;
						}
						else
						{
							currentPathfindCooldown -= Time.deltaTime;
						}
						 
					}
					else
					{
						if (distanceToTarget < guestData.MinimumTargetDistance)
						{
							transform.position = target;	
							moving = false;
						}
					}
					
				}
				break;
			case GuestActivity.Leaving:
				if (distanceToTarget < guestData.MinimumTargetDistance)
				{
					Destroy(this.gameObject);
				}
				break;
			case GuestActivity.Waiting:

				TotalWaitingTime += Time.deltaTime;

				TryFindRoom();

				break;
		}
		
		//ticks down once the hotel has been entered, even while waiting in the lobby
		if (currentActivity != GuestActivity.Arriving)
		{
			CurrentVacationTime -= Time.deltaTime;

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
	/// Checks if the room has satisfied every need it could
	/// </summary>
	/// <returns></returns>
	private bool IsAllNeedsSatisfiedByRoom()
	{
		foreach(NeedType need in currentRoom.roomType.NeedTypesSatisfied)
		{
			if(needs.ContainsKey(need) && needs[need] > 0)
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Checks if a collection of needs contains any that the guest has
	/// </summary>
	/// <returns></returns>
	private bool IsAnyRelevantNeed(NeedType[] needToCheck)
	{
		foreach(NeedType need in needToCheck)
		{
			if (needs.ContainsKey(need)) return true;
		}

		return false;
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
	/// Gets the guest's need duration
	/// </summary>
	/// <returns></returns>
	private float GetNeedDuration(NeedData needData)
	{
		return needData.NeedDuration * LuxuryMultiplier;
	}

	/// <summary>
	/// The guest selects their desires
	/// </summary>
	private void SetupNeeds()
	{
		if (Random.Range(1, 100) < 15 * hotelStateData.CurrentHotelRatingPercentage)
		{
			LuxuryMultiplier = 2f;

			sprite.color = guestData.LuxuryColour;
			guestSadFaceSprite.color = guestData.LuxuryColour;
		}

		var guaranteedNeed = guestData.GuaranteedNeed;

		needs.Add(guaranteedNeed.NeedType, GetNeedDuration(guaranteedNeed));
		needDatas.Add(guaranteedNeed.NeedType, guaranteedNeed);
		guaranteedNeedSprite.sprite = guaranteedNeed.Sprite;

		var randomNeed = guestData.PickNeed;
		needs.Add(randomNeed.NeedType, GetNeedDuration(randomNeed));
		needDatas.Add(randomNeed.NeedType, randomNeed);
		randomNeedSprite.sprite = randomNeed.Sprite;

		UpdateNeedsDisplay();

		float totalNeed = needs.Values.Sum();

		VacationBudget = hotelStateData.RoomRentPerSecond * totalNeed * LuxuryMultiplier;
		VacationTime = totalNeed + WaitingBuffer;

		CurrentVacationBudget = VacationBudget;
		CurrentVacationTime = VacationTime;
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

		List<HotelRoom> visitableRooms = MapManager.HotelRooms.FindAll(r => !r.Flooded && !r.Sunk && r != currentRoom
		&& !r.AtCapacity && IsAnyRelevantNeed(r.roomType.NeedTypesSatisfied));
		
		if (visitableRooms.Count == 0)
		{
			currentPathfindCooldown = PathfindCooldown;
			return;
		}

		List<HotelRoom> roomsMatchingLuxuryLevel = visitableRooms.FindAll(r => r.LuxuryMultiplier == LuxuryMultiplier);

		if(roomsMatchingLuxuryLevel.Count > 0)
		{
			int randomRoomIndex = Random.Range(0, roomsMatchingLuxuryLevel.Count);
			roomToVisit = roomsMatchingLuxuryLevel[randomRoomIndex];
		}
		else
		{
			int randomRoomIndex = Random.Range(0, visitableRooms.Count);
			roomToVisit = visitableRooms[randomRoomIndex];
		}
		
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
			guestSadFaceSprite.enabled = luxuryMultiplier != currentRoom.LuxuryMultiplier;

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
		if (currentActivity == GuestActivity.Leaving) return;

		if(currentRoom != null)
		{
			ChangeRoom(null);
			transform.position = ExitPoint;

			ChangeSortingLayer("GuestBehindHotel");
		}

		target = DespawnPoint;
		currentActivity = GuestActivity.Leaving;
		moving = true;

		LeaveReview();
	} 

	/// <summary>
	/// Changes what surting layer the guest, and its associated sprites are on
	/// </summary>
	/// <param name="layerName"></param>
	private void ChangeSortingLayer(string layerName)
	{
		var sortingLayerId = SortingLayer.NameToID(layerName);
		sortingGroup.sortingLayerID = sortingLayerId;
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
		
	}

	/// <summary>
	/// Leaves a review of the hotel
	/// </summary>
	private void LeaveReview()
	{
		int reviewPoints = hotelStateData.MaxHotelRating;

		//if waited for longer than a percentage of vacation: penalty
		if (TotalWaitingTime > VacationTime * 0.1) reviewPoints--;

		//if the unsatisfied need amount is too large, -1 or -2
		foreach(var need in needs)
		{
			var needData = needDatas[need.Key];

			if (need.Value > GetNeedDuration(needData) * 0.7) reviewPoints--;
			if (need.Value > GetNeedDuration(needData) * 0.4) reviewPoints--;
		}

		//if spent too much time in wrong luxury rooms, -1
		if (totalWrongLuxuryTime > VacationTime * 0.2) reviewPoints--;

		//minimum is one
		if (reviewPoints < 1) reviewPoints = 1;

		guestSadFaceSprite.enabled = reviewPoints < hotelStateData.MaxHotelRating * 0.5f;

		hotelStateData.AddReview(reviewPoints);
	}
}
