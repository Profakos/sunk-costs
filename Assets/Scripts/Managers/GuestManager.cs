using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuestManager : MonoBehaviour
{
	[Header("ScriptableObjects")]
	/// <summary>
	/// SO containing the hotel's current stats
	/// </summary>
	[SerializeField]
	private HotelStateData hotelStateData;

	[Header("Managers")]

	/// <summary>
	/// Map manager reference
	/// </summary>
	[SerializeField]
	private MapManager mapManager;

	[Header("Prefabs")]

	/// <summary>
	/// Basic guest prefab
	/// </summary>
	[SerializeField]
	private Guest guestPrefab = null;


	[Header("Transforms")]

	/// <summary>
	/// Guests vanish here
	/// </summary>
	[SerializeField]
	private Transform guestDespawner = null;
	/// <summary>
	/// Guests appear here
	/// </summary>
	[SerializeField]
	private Transform guestSpawner = null;
	/// <summary>
	/// Guests head to this point to enter the hotel
	/// </summary>
	[SerializeField]
	private Transform guestEntrance = null;
	/// <summary>
	/// Guests exit the hotel here
	/// </summary>
	[SerializeField]
	private Transform guestExit = null;

	[Header("Guest management")]
	/// <summary>
	/// The general time between guest spawns
	/// </summary>
	[SerializeField]
	private float SpawnDelayBetweenWaves = 6f;
	/// <summary>
	/// The current time until the next guest spawn
	/// </summary>
	[SerializeField]
	private float spawnCountdown = 0f;
	/// <summary>
	/// Time between guests within a wave
	/// </summary>
	[SerializeField]
	private float spawnDelayBetweenGuests = .2f;

	void Awake()
	{
		mapManager = gameObject.GetComponent<MapManager>();
		
	}

	// Start is called before the first frame update
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		spawnCountdown -= Time.deltaTime;
		if(spawnCountdown <= 0f)
		{
			StartCoroutine(SpawnGuests());
			spawnCountdown = SpawnDelayBetweenWaves;
		}
	}
	
	/// <summary>
	/// Deletes all guests
	/// </summary>
	public void DeleteGuests()
	{
		//TODO: use a listener channel
		foreach(GameObject guestObject in GameObject.FindGameObjectsWithTag("Guest"))
		{
			Destroy(guestObject);
		}
	}

	/// <summary>
	/// Forces guests to leave
	/// </summary>
	public void ForceGuestsLeave()
	{
		//TODO: use a listener channel
		foreach (GameObject guestObject in GameObject.FindGameObjectsWithTag("Guest"))
		{
			Guest guest = guestObject.GetComponent<Guest>();
			guest.ForceLeave();
		}
	}

	/// <summary>
	/// Coroutine that handles the spawning of guests
	/// </summary>
	/// <returns></returns>
	public IEnumerator SpawnGuests()
	{
		int possibleGuestNum = 1;

		float currentRatingPercentage = hotelStateData.CurrentHotelRatingPercentage;

		if (currentRatingPercentage > 0.4f) possibleGuestNum += 1;
		if (currentRatingPercentage > 0.8f) possibleGuestNum += 1;

		int guestNum = Random.Range(1, possibleGuestNum + 1);

		for (int i = 0; i < guestNum; i++)
		{
			Guest guest = Instantiate(guestPrefab, guestSpawner.position, Quaternion.identity);

			guest.DespawnPoint = guestDespawner.position;
			guest.EntrancePoint = guestEntrance.position;
			guest.ExitPoint = guestExit.position;

			guest.MapManager = mapManager;
			 
			yield return new WaitForSeconds(spawnDelayBetweenGuests);
		}
		 
	}
}
