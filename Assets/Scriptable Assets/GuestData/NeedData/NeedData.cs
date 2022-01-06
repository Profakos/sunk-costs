using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NeedData", menuName = "ScriptableObjects/GuestData/NeedData", order = 1)]
public class NeedData : ScriptableObject
{
	[SerializeField]
	private string DisplayName;
	[SerializeField]
	private Sprite sprite;
	[SerializeField]
	private NeedType needType;

	private float needDuration = 10f;

	public float NeedDuration { get => needDuration; set => needDuration = value; }
	public Sprite Sprite { get => sprite; set => sprite = value; }
	public NeedType NeedType { get => needType; set => needType = value; }
}
