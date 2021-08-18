using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomShapeData", menuName = "ScriptableObjects/RoomShapeData", order = 1)]
public class RoomShapeData : ScriptableObject
{
	[SerializeField]
	private string shapeName;

	[SerializeField]
	private Vector2[] offsetFromRoomCenter;

	public string ShapeName { get => shapeName; set => shapeName = value; }
	public Vector2[] OffsetFromRoomCenter { get => offsetFromRoomCenter; set => offsetFromRoomCenter = value; }
}
