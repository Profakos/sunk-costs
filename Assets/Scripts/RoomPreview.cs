using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPreview : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;
	public HotelSizeData hotelSizeData;
	
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
	
	void LateUpdate()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

		float x = Mathf.Max(Mathf.Min(Mathf.RoundToInt(mousePosition.x * hotelSizeData.Scale), hotelSizeData.MaxX), hotelSizeData.MinX);
		float y = Mathf.Max(Mathf.Min(Mathf.RoundToInt(mousePosition.y * hotelSizeData.Scale), Mathf.Min(hotelSizeData.MinY + hotelSizeData.CurrentHotelHeight - 1, hotelSizeData.MaxY)), hotelSizeData.MinY);


		transform.position = new Vector3(x, y, 0);
	}

	/// <summary>
	/// Updates the preview's sprite
	/// </summary>
	/// <param name="newSpriteRenderer"></param>
	public void UpdateSprite(SpriteRenderer newSpriteRenderer)
	{
		if (newSpriteRenderer == null) return;

		spriteRenderer.sprite = newSpriteRenderer.sprite;
	}

}
