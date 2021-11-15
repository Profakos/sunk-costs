using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotelRoom : MonoBehaviour
{
	public RoomShapeData roomShape;
	public HotelSizeData hotelSizeData;

	[SerializeField]
	private int capacity;
	[SerializeField]
	private int guestAmount;
	public Vector2 doorOffset;

	public int Capacity { get => capacity; } 
	public bool AtCapacity => guestAmount >= Capacity;

	public SpriteRenderer spriteRenderer;

	public bool Sunk { get; set; }
	public bool Flooded { get; set; }
	public int GuestAmount { get => guestAmount; set => guestAmount = value; }

	public delegate void SinkingDelegate(bool floodedOrSunk);
	public event SinkingDelegate sinkingHandler;

	void Awake()
	{
		spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	}

	// Start is called before the first frame update
	void Start()
	{
		
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
		return (Vector2)gameObject.transform.position + doorOffset;
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
	
	public struct PathNodeData
	{
        public float dist { get; set; }
        public Vector2 parent { get; set; }
    }
	
	/// <summary>
	/// Finds the shortest path, reverse from end to start
	/// </summary>
	/// <param name="start"></param>
	/// <param name="end"></param>
	/// <returns>The path from start to end</returns>
	public List<Vector2> GetShortestPath(Vector2 start, Vector2 end)
    {
        if(start == end)
        {
            Debug.Log("start == end");
            return new List<Vector2>();
        }
        Dictionary<Vector2, List<Vector2>> graph = roomShape.GetGraph();
        Dictionary<Vector2, PathNodeData> pathData = new Dictionary<Vector2, PathNodeData>();
        HashSet<Vector2> visited = new HashSet<Vector2>();
        HashSet<Vector2> edge = new HashSet<Vector2>();
        pathData.Add(end, new PathNodeData {dist = 0, parent = end});
        visited.Add(end);
        foreach(Vector2 neighbor in graph[end])
        {
            edge.Add(neighbor);
            pathData.Add(neighbor, new PathNodeData {dist = 1, parent = end});
        }
        while(edge.Count != 0)
        {
            // sort edge for distance
            Vector2[] edgearray = new Vector2[edge.Count];
            edge.CopyTo(edgearray);
            Array.Sort<Vector2>(edgearray, (x,y) => pathData[x].dist.CompareTo(pathData[y].dist));
            
            Vector2 current = edgearray[0];
            edge.Remove(current);
            visited.Add(current);

            if(current == start)
            {
                // there is a path
                List<Vector2> solution = new List<Vector2>();
                do
                {
                    solution.Add(current);
                    current = pathData[current].parent;
                } while(current != end);
                solution.Add(end);

                return solution;
            }
            
            foreach(Vector2 neighbor in graph[current])
            {
                if(!visited.Contains(neighbor))
                {
                    if(!edge.Contains(neighbor))
                    {
                        edge.Add(neighbor);
                        pathData.Add(neighbor, new PathNodeData {dist = pathData[current].dist+1, parent = current});
                    }
                    else
                    {
                        if(pathData[neighbor].dist > pathData[current].dist + 1)
                        {
                            PathNodeData old_data = pathData[neighbor];
                            old_data.dist = pathData[current].dist + 1;
                            old_data.parent = current;
                            pathData[neighbor] = old_data;
                        }
                    }
                }
            }
        }

		// no path found
		return new List<Vector2>();
	}
}
