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
		SpawnGuest();
	}

	// Update is called once per frame
	void Update()
	{
		
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

	public void SpawnGuest()
	{
		GameObject guestComponent = Instantiate(guestPrefab, guestSpawner.position, Quaternion.identity);
		Guest guest = guestComponent.GetComponent<Guest>();

		guest.DespawnPoint = guestDespawner.position;
		guest.EntrancePoint = guestEntrance.position;
		guest.ExitPoint = guestExit.position;

		guest.MapManager = mapManager;
	}
}
