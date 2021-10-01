using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guest : MonoBehaviour
{
	public Vector3 ExitPoint { get; set; }

	private Rigidbody2D rigidBody;

	private int speed = 10;

	void Awake()
	{
		rigidBody = gameObject.GetComponent<Rigidbody2D>();
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(transform.position.x >= ExitPoint.x)
		{
			Destroy(this.gameObject);
			return;
		}

		Vector3 m_Input = new Vector3(1, 0, 0);

		rigidBody.MovePosition(transform.position + m_Input * speed * Time.deltaTime);
    }
}
