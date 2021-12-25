using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GuestData", menuName = "ScriptableObjects/GuestData/GuestData", order = 1)]
public class GuestData : ScriptableObject
{

	[SerializeField]
	private NeedData guaranteedNeed;

	[SerializeField]
	private List<NeedData> randomNeeds;

	private float minimumTargetDistance = 0.02f;
	private Color luxuryColour = new Color(0.38f, 0.32f, 0.52f);

	public List<NeedData> RandomNeeds { get => randomNeeds; set => randomNeeds = value; }
	public float MinimumTargetDistance { get => minimumTargetDistance; set => minimumTargetDistance = value; }
	public Color LuxuryColour { get => luxuryColour; set => luxuryColour = value; }
	public NeedData GuaranteedNeed { get => guaranteedNeed; set => guaranteedNeed = value; }

	public NeedData PickNeed { get => randomNeeds[Random.Range(0, randomNeeds.Count)]; }
}
