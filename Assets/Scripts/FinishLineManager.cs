using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FinishLineManager : MonoBehaviour
{
    public GameObject resultsPanel; // UI Panel to display the player's position
    public Text resultsText; // UI Text to show the player's position
    public Camera mainCamera; // Reference to the main camera that follows the player's car

    private Dictionary<GameObject, int> lapCounts = new Dictionary<GameObject, int>(); // Tracks laps for each car
    private List<GameObject> finishingOrder = new List<GameObject>(); // List to store finishing order

    void Start()
    {
        // // Ensure the results panel is hidden at the start
        // if (resultsPanel != null)
        // {
        //     resultsPanel.SetActive(false);
        // }
        // else
        // {
        //     Debug.LogError("Results Panel is not assigned in the Inspector!");
        // }
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject car = other.gameObject;

        // Ensure we're only counting cars tagged "Car" and not objects like walls, obstacles, etc.
        if (car.CompareTag("Car") || car.CompareTag("Player"))
        {
            // Check if the car is in the dictionary, if not, add it with 0 laps
            if (!lapCounts.ContainsKey(car))
            {
                lapCounts[car] = 0;
            }

            // Increment the lap count
            lapCounts[car]++;

            // Check if this is the second time the car crosses the finish line
            if (lapCounts[car] == 2)
            {
                // Register the car's finishing position
                finishingOrder.Add(car);

                // If the car is the player, display the results panel
                if (car.CompareTag("Player"))
                {
                    Debug.Log("Player finished the race! Displaying results...");
                    // ShowResultsPanel();
                }
            }
        }
    }

    // void ShowResultsPanel()
    // {
    //     if (resultsPanel == null || resultsText == null)
    //     {
    //         Debug.LogError("ResultsPanel or ResultsText is not set in the Inspector!");
    //         return;
    //     }

    //     // Determine the player's position in the race
    //     int playerPosition = finishingOrder.IndexOf(GameObject.FindGameObjectWithTag("Player")) + 1;

    //     // Show results panel and set the text to show the player's finishing position
    //     resultsPanel.SetActive(true);
    //     resultsText.text = $"You came {GetOrdinal(playerPosition)}!";

    //     // Make the panel a child of the main camera to ensure it follows the camera view
    //     resultsPanel.transform.SetParent(mainCamera.transform, false);

    //     Debug.Log($"You came {GetOrdinal(playerPosition)}!");
    // }

    // Helper method to convert a number to an ordinal (1st, 2nd, 3rd, etc.)
    string GetOrdinal(int number)
    {
        if (number <= 0) return number.ToString();
        switch (number % 100)
        {
            case 11:
            case 12:
            case 13:
                return number + "th";
        }
        switch (number % 10)
        {
            case 1:
                return number + "st";
            case 2:
                return number + "nd";
            case 3:
                return number + "rd";
            default:
                return number + "th";
        }
    }
}
