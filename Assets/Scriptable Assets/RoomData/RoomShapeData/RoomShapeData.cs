using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomShapeData", menuName = "ScriptableObjects/RoomData/RoomShapeData", order = 1)]
public class RoomShapeData : ScriptableObject
{
	[SerializeField]
	private string shapeName;

	[SerializeField]
	private Vector2[] offsetFromRoomCenter;

	public string ShapeName { get => shapeName; set => shapeName = value; }
	public Vector2[] OffsetFromRoomCenter { get => offsetFromRoomCenter; set => offsetFromRoomCenter = value; }
	private Dictionary<Vector2, List<Vector2>> graph;

    public Dictionary<Vector2, List<Vector2>> GetGraph()
    {
        if(graph == null)
        {
            Vector2[] neighbor_dirs = {new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1)};

            graph = new Dictionary<Vector2, List<Vector2>>();

            for(int i = 0; i < offsetFromRoomCenter.Length; i++)
            {
                Vector2 current = offsetFromRoomCenter[i];
                graph.Add(current, new List<Vector2>());
                for(int d = 0; d < neighbor_dirs.Length; d++)
                {
                    Vector2 candidate = current + neighbor_dirs[d];
                    if(graph.ContainsKey(candidate))
                    {
                        graph[candidate].Add(current);
                        graph[current].Add(candidate);
                    }
                }
            }
        }
        return graph;
    }
}
