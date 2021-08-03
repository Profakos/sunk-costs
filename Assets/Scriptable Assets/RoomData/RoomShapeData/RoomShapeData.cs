using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomShapeData", menuName = "ScriptableObjects/RoomShapeData", order = 1)]
public class RoomShapeData : ScriptableObject
{
	public string shapeName;
	 
	public Vector2[] offsetFromRoomCenter;
}
