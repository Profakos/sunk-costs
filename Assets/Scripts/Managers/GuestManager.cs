using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuestManager : MonoBehaviour
{
	public GameObject guestPrefab;

	private Transform guestSpawner;
	private Transform guestDespawner;

	void Awake()
	{
		guestDespawner = GameObject.Find("GuestDespawner").transform;
		guestSpawner = GameObject.Find("GuestSpawner").transform;
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
		guest.ExitPoint = guestDespawner.position;
	}
}
