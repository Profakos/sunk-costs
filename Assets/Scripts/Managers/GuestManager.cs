using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuestManager : MonoBehaviour
{
	private MapManager mapManager;

	public GameObject guestPrefab;

	private Transform guestDespawner;
	private Transform guestSpawner;

	private Transform guestEntrance;
	private Transform guestExit;

	public float spawnDelay = 6f;
	public float timeUntilNextGuestWave = 0f;

	void Awake()
	{
		mapManager = gameObject.GetComponent<MapManager>();

		guestDespawner = GameObject.Find("GuestDespawner").transform;
		guestSpawner = GameObject.Find("GuestSpawner").transform;

		guestEntrance = GameObject.Find("GuestEntrance").transform;
		guestExit = GameObject.Find("GuestExit").transform;
	}

	// Start is called before the first frame update
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		timeUntilNextGuestWave -= Time.deltaTime;
		if(timeUntilNextGuestWave <= 0f)
		{
			StartCoroutine(SpawnGuests());
			timeUntilNextGuestWave = spawnDelay;
		}
	}
	
	/// <summary>
	/// Deletes all guests
	/// </summary>
	public void DeleteGuests()
	{
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
		int guestNum = Random.Range(1, 3);

		for(int i = 0; i < guestNum; i++)
		{
			GameObject guestComponent = Instantiate(guestPrefab, guestSpawner.position, Quaternion.identity);
			Guest guest = guestComponent.GetComponent<Guest>();

			guest.DespawnPoint = guestDespawner.position;
			guest.EntrancePoint = guestEntrance.position;
			guest.ExitPoint = guestExit.position;

			guest.MapManager = mapManager;
			 
			yield return new WaitForSeconds(.2f);
		}
		 
	}
}
