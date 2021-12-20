using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotelRoom : MonoBehaviour
{
	public RoomShapeData roomShape;
	public HotelSizeData hotelSizeData;

	public RoomType roomType;

	[SerializeField]
	private int guestAmount;

	private float needFulfillingRate = 1f;

	public int currentCapacity;
	
	public int Capacity { get => currentCapacity; set { currentCapacity = value; } } 
	public bool AtCapacity => guestAmount >= Capacity;

	public SpriteRenderer spriteRenderer;

	public bool Sunk { get; set; }
	public bool Flooded { get; set; }
	public int GuestAmount { get => guestAmount; set => guestAmount = value; }

	public Vector2 DoorOffset { get => roomType.DoorOffset; }
	public float NeedFulfillingRate { get => needFulfillingRate; set => needFulfillingRate = value; }

	public delegate void SinkingDelegate(bool floodedOrSunk);
	public event SinkingDelegate sinkingHandler;


	void Awake()
	{
		spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	}

	// Start is called before the first frame update
	void Start()
	{
		currentCapacity = roomType.GuestCapacity;
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	/// <summary>
	/// Returns the position of the room's door
	/// </summary>
	/// <returns></returns>
	public Vector2 DoorPosition()
	{
		return (Vector2)gameObject.transform.position + DoorOffset;
	}
	
	/// <summary>
	/// Sinks the room one tile
	/// </summary>
	public void Sink()
	{
		transform.Translate(0, -1f, 0);

		int floodedTiles = 0;

		foreach(Vector2 offset in roomShape.OffsetFromRoomCenter)
		{
			if( (transform.position.y + offset.y) < hotelSizeData.MinY)
			{
				floodedTiles++;
			}
		}

		if(floodedTiles > 0 )
		{
			Flooded = true;
			spriteRenderer.color = Color.yellow;
			Capacity = 0;
		}
		
		if(floodedTiles == roomShape.OffsetFromRoomCenter.Length)
		{
			Sunk = true;
		}

		if(sinkingHandler != null)
		sinkingHandler.Invoke(Flooded || Sunk);
	}

	public void SubscribeSink(SinkingDelegate e)
	{
		sinkingHandler += e;
	}

	public void UnsubscribeSink(SinkingDelegate e)
	{
		if (sinkingHandler == null) return;
		sinkingHandler -= e;
	}
	
	public struct PathDataNode
	{
		public float dist { get; set; }
		public Vector2 parent { get; set; }
	}
	
	public class PathDataComparer : IComparer<Vector2>
	{
		private Dictionary<Vector2, PathDataNode> pd;
		public PathDataComparer(Dictionary<Vector2, PathDataNode> pathData) => pd = pathData;
		public int Compare(Vector2 x, Vector2 y)
		{
			return Comparer<float>.Default.Compare(pd[x].dist, pd[y].dist);
		}
	}

	/// <summary>
	/// Formats the price of the room for the label
	/// </summary>
	/// <returns></returns>
	public string GetPurchaseLabel()
	{
		string purchaseLabel = roomType.RoomLabel;

		foreach(var need in roomType.NeedTypesSatisfied)
		{
			purchaseLabel += " <sprite name=\"" + need.ToString() + "\">";
		}

		purchaseLabel += ", $" + roomType.PurchasePrice;
		return purchaseLabel;
	}
	
	/// <summary>
	/// Finds the shortest path, reverse from end to start
	/// </summary>
	/// <param name="start"></param>
	/// <param name="end"></param>
	/// <returns>The path from start to end</returns>
	public List<Vector2> GetShortestPath(Vector2 start, Vector2 end)
	{
		List<Vector2> solution = new List<Vector2>();
		if(start == end)
		{
			Debug.Log("start == end");
			solution.Add(start);
			return solution;
		}
		Dictionary<Vector2, List<Vector2>> graph = roomShape.GetGraph();
		Dictionary<Vector2, PathDataNode> pathData = new Dictionary<Vector2, PathDataNode>();
		HashSet<Vector2> visited = new HashSet<Vector2>();
		// sorted set contains edges sorted by distance
		SortedSet<Vector2> edge = new SortedSet<Vector2>(new PathDataComparer(pathData));
		// loop invariant:
		// the nodes in edge are the ones in consideration
		// nodes in visited are already known the best path to
		pathData.Add(end, new PathDataNode {dist = 0, parent = end}); // parent of the origin is itself, Vector2 cannot be null
		visited.Add(end);
		foreach(Vector2 neighbor in graph[end])
		{
			pathData.Add(neighbor, new PathDataNode {dist = 1, parent = end});
			edge.Add(neighbor);
		}
		while(edge.Count != 0)
		{
			// pick the closest edge, which cannot be closer by any means
			Vector2 current = edge.Min;
			edge.Remove(current);
			visited.Add(current);

			if(current == start)
			{
				// we found the goal, a path exists
				// trace back to the origin from the chained parents
				do
				{
					solution.Add(current);
					current = pathData[current].parent;
				} while(current != end);
				solution.Add(end);
				return solution;
			}
			
			// add neighbors to edges
			foreach(Vector2 neighbor in graph[current])
			{
				if(visited.Contains(neighbor)) continue;
				if(!edge.Contains(neighbor))
				{
					// new node, 1 step farther than current
					pathData.Add(neighbor, new PathDataNode {dist = pathData[current].dist+1, parent = current});
					edge.Add(neighbor);
				}
				else if(pathData[neighbor].dist > pathData[current].dist + 1)
				{
					// update edge data because shorter path exists
					PathDataNode old_data = pathData[neighbor];
					old_data.dist = pathData[current].dist + 1;
					old_data.parent = current;
					pathData[neighbor] = old_data;
				}
			}
		}
		// no path found
		Debug.Log("cannot find path from " + start + " to " + end + "!");
		return solution;
	}
}
