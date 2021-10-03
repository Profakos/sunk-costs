using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guest : MonoBehaviour
{
	public Vector2 EntrancePoint { get; set; }
	public Vector2 ExitPoint { get; set; }
	public Vector2 DespawnPoint { get; set; }

	private Rigidbody2D rigidBody;
	private SpriteRenderer sprite;

	private int speed = 2;

	[SerializeField]
	private GuestActivity currentActivity;

	private float minimumTargetDistance = 0.01f;
	
	[SerializeField]
	private Vector2 target;

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

		float distanceToTarget = (target - (Vector2)transform.position).magnitude;

		switch (currentActivity)
		{
			case GuestActivity.Arriving:
				if(distanceToTarget < minimumTargetDistance)
				{
					sprite.sortingLayerID = SortingLayer.NameToID("GuestInRoom");
					target = ExitPoint;
					currentActivity = GuestActivity.Enjoying;
				}
				break;
			case GuestActivity.Enjoying:
				if (distanceToTarget < minimumTargetDistance)
				{
					sprite.sortingLayerID = SortingLayer.NameToID("GuestBehindHotel");
					target = DespawnPoint;
					currentActivity = GuestActivity.Leaving;
				}
				break;
			case GuestActivity.Leaving:
				break;
		}
	}

	private void FixedUpdate()
	{


		MoveGuest(target);
	}
	
	private void MoveGuest(Vector2 target)
	{
		Vector2 dir = target - (Vector2)transform.position;
		dir.Normalize();
		rigidBody.MovePosition((Vector2)transform.position + dir * speed * Time.deltaTime);
	}


}
