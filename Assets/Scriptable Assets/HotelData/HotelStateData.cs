using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HotelSize", menuName = "ScriptableObjects/HotelData/HotelState", order = 2)]
public class HotelStateData : ScriptableObject
{

	private float initialHotelHeight = 0;
	[SerializeField]
	private float currentHotelHeight;

	[SerializeField]
	private float money = 2000f;
	public delegate void MoneyChangeDelegate();
	public event MoneyChangeDelegate moneyChangeHandler;

	[SerializeField]
	private float roomRentPerSecond = 1f;

	[SerializeField]
	private float floorPurchasePrice = 15f;
	[SerializeField]
	private string floorLabel = "New Floor";

	public float CurrentHotelHeight { get => currentHotelHeight; set => currentHotelHeight = value; }
	public float InitialHotelHeight { get => initialHotelHeight; }
	public int TotalSpawnedFloors { get; set; }

	public float Money { get => money; set { money = value; if(moneyChangeHandler != null)moneyChangeHandler.Invoke(); } }

	public float FloorPurchasePrice { get => floorPurchasePrice; set => floorPurchasePrice = value; }
	public string FloorLabel { get => floorLabel; set => floorLabel = value; }
	public float RoomRentPerSecond { get => roomRentPerSecond; set => roomRentPerSecond = value; }
}
