using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HotelSize", menuName = "ScriptableObjects/HotelData/HotelSize", order = 1)]
public class HotelSizeData : ScriptableObject
{
	[SerializeField]
	private float scale;
	[SerializeField]
	private float minX;
	[SerializeField]
	private float maxX;
	[SerializeField]
	private float minY;
	[SerializeField]
	private float maxY;
	
	public float Scale { get => scale; set => scale = value; }
	public float MinX { get => minX; set => minX = value; }
	public float MaxX { get => maxX; set => maxX = value; }
	public float MinY { get => minY; set => minY = value; }
	public float MaxY { get => maxY; set => maxY = value; }
}
