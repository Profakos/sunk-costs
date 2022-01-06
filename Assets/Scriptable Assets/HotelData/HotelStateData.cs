using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "HotelStateData", menuName = "ScriptableObjects/HotelData/HotelState", order = 2)]
public class HotelStateData : ScriptableObject
{
	public delegate void ValueChangeDelegate();

	private float initialHotelHeight = 0;
	[SerializeField]
	private float currentHotelHeight;

	[SerializeField]
	private float money = 2000f;
	public event ValueChangeDelegate moneyChangeHandler;

	[SerializeField]
	private float roomRentPerSecond = 1f;

	[SerializeField]
	private float floorPurchasePrice = 15f;
	[SerializeField]
	private string floorLabel = "New Floor";

	[SerializeField]
	private int maxHotelRating = 5;
	[SerializeField]
	private float currentHotelRating = 0f;
	public event ValueChangeDelegate ratingChangeHandler;

	[SerializeField]
	public Queue<float> reviews = new Queue<float>();
	[SerializeField]
	private int maxReviewRemembered = 10;

	public float CurrentHotelHeight { get => currentHotelHeight; set => currentHotelHeight = Math.Max(0, value); }
	public float InitialHotelHeight { get => initialHotelHeight; }
	public int TotalSpawnedFloors { get; set; }

	public float Money { get => money; set { money = Math.Max(0, value); if(moneyChangeHandler != null)moneyChangeHandler.Invoke(); } }

	public float FloorPurchasePrice { get => floorPurchasePrice; set => floorPurchasePrice = value; }
	public string FloorLabel { get => floorLabel; set => floorLabel = value; }
	public float RoomRentPerSecond { get => roomRentPerSecond; set => roomRentPerSecond = value; }
	public int MaxHotelRating { get => maxHotelRating; set => maxHotelRating = value; }
	public float CurrentHotelRating { get => currentHotelRating; set { currentHotelRating = Math.Max(0, Math.Min(value, maxHotelRating));
			
			if (ratingChangeHandler != null) ratingChangeHandler.Invoke(); } }
	public float CurrentHotelRatingPercentage { get => currentHotelRating / maxHotelRating; }
	public int MaxReviewRemembered { get => maxReviewRemembered; set => maxReviewRemembered = value; }


	/// <summary>
	/// Adds a review
	/// </summary>
	/// <param name="newReview"></param>
	public void AddReview(float newReview)
	{
		reviews.Enqueue(newReview);
		RecalculateRating();
	}

	/// <summary>
	/// Adds multiple reviews
	/// </summary>
	/// <param name="newReviews"></param>
	public void AddReviews(List<float> newReviews)
	{
		foreach (float newReview in newReviews)
		{
			reviews.Enqueue(newReview);
		}

		RecalculateRating();
	}

	/// <summary>
	/// Recalculates the hotel rating
	/// </summary>
	private void RecalculateRating()
	{
		while (reviews.Count > MaxReviewRemembered) reviews.Dequeue();

		CurrentHotelRating = reviews.AsQueryable().Average();
	}
}
