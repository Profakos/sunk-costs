using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HotelSize", menuName = "ScriptableObjects/HotelSizeData", order = 1)]
public class HotelSizeData : ScriptableObject
{
	public float scale;
	public float minX;
	public float maxX;
	public float minY;
	public float maxY;

	public float currentHotelHeight = 1; 
	 

}
