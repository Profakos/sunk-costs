using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomTypeData", menuName = "ScriptableObjects/RoomData/RoomTypeData", order = 1)]
public class RoomType : ScriptableObject
{
	[SerializeField]
	private int guestCapacity = 0;
	[SerializeField]
	private float pricePerSecond = 1f;
	[SerializeField]
	private float purchasePrice = 0;
	[SerializeField]
	private string roomLabel = "Room";
	[SerializeField]
	private Vector2 doorOffset;

	public int GuestCapacity { get => guestCapacity; set => guestCapacity = value; }
	public float PricePerSecond { get => pricePerSecond; set => pricePerSecond = value; }
	public float PurchasePrice { get => purchasePrice; set => purchasePrice = value; }
	public string RoomLabel { get => roomLabel; set => roomLabel = value; }
	public Vector2 DoorOffset { get => doorOffset; set => doorOffset = value; }
}
