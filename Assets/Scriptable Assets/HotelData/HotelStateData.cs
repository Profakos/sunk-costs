using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HotelSize", menuName = "ScriptableObjects/HotelData/HotelState", order = 2)]
public class HotelStateData : ScriptableObject
{

	private float initialHotelHeight = 0;
	[SerializeField]
	private float currentHotelHeight;

	public float CurrentHotelHeight { get => currentHotelHeight; set { currentHotelHeight = value; } }
	public float InitialHotelHeight { get => initialHotelHeight; }
	public int TotalSpawnedFloors { get; set; }
	
}
