using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPreview : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;


	void LateUpdate()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

		float scale = 1;

		float minX = -8;
		float maxX = 8;
		float minY = -4;
		float maxY = 4;

		float x = Mathf.Max(Mathf.Min(Mathf.RoundToInt(mousePosition.x * scale), maxX), minX);
		float y = Mathf.Max(Mathf.Min(Mathf.RoundToInt(mousePosition.y * scale), maxY), minY);


		transform.position = new Vector3(x, y, 0);
	}

	void Awake()
	{
		spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
	}

	//updates the preview's sprite
	public void UpdateSprite(SpriteRenderer newSpriteRenderer)
	{
		if (newSpriteRenderer == null) return;

		spriteRenderer.sprite = newSpriteRenderer.sprite;
	}

}
