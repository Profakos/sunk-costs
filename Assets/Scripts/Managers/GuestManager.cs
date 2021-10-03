using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuestManager : MonoBehaviour
{
	public GameObject guestPrefab;

	private Transform guestDespawner;
	private Transform guestSpawner;

	private Transform guestEntrance;
	private Transform guestExit;

	void Awake()
	{
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

	private void SpawnGuest()
	{
		GameObject guestComponent = Instantiate(guestPrefab, guestSpawner.position, Quaternion.identity);
		Guest guest = guestComponent.GetComponent<Guest>();

		guest.DespawnPoint = guestDespawner.position;
		guest.EntrancePoint = guestEntrance.position;
		guest.ExitPoint = guestExit.position;
	}
}
