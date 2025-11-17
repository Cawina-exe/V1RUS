using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Drag your Circle Prefab here in the Inspector
    public GameObject circlePrefab;

    // Set this to your circle's size in the Inspector (e.g., 1.0)
    public float circleRadius = 1.0f;

    // NEW: This is the total number of circles in the game.
    public int totalCirclesToSpawn = 20;

    // This will track which circle we're on (1, 2, ... 20)
    private int currentTargetNumber;

    void Start()
    {
        StartGame(); // Start the game when the scene loads
    }

    // This function sets up the game from the beginning
    void StartGame()
    {
        // Reset the counter to 1
        currentTargetNumber = 1;

        // Spawn the VERY FIRST circle (Circle #1)
        SpawnNextCircle();
    }

    // This is our new function that spawns ONE circle
    void SpawnNextCircle()
    {
        // --- This is the anti-overlap logic ---
        int maxSpawnAttempts = 20;
        int currentSpawnAttempts = 0;
        Vector2 spawnPos = Vector2.zero;
        bool spotIsClear = false;

        do
        {
            // Find a random position
            float randomX = Random.Range(-7f, 7f);
            float randomY = Random.Range(-4f, 4f);
            spawnPos = new Vector2(randomX, randomY);

            currentSpawnAttempts++;
            if (currentSpawnAttempts > maxSpawnAttempts)
            {
                Debug.LogError("Could not find a clear spot to spawn circle " + currentTargetNumber);
                break;
            }

            // Check if the spot is clear of other colliders (like a previous circle)
            spotIsClear = (Physics2D.OverlapCircle(spawnPos, circleRadius) == null);

        } while (!spotIsClear);
        // --- End of anti-overlap logic ---

        // Only spawn if we found a clear spot
        if (spotIsClear)
        {
            // Create the new circle
            GameObject circleGO = Instantiate(circlePrefab, spawnPos, Quaternion.identity);

            // Tell the circle what number it is (e.g., 1, or 2, or 3...)
            circleGO.GetComponent<ClickableCircle>().Initialize(this, currentTargetNumber);
        }
    }

    // This function is called by the ClickableCircle script
    public void CircleClicked(int numberClicked)
    {
        // We only check if the clicked number is the one we're waiting for
        if (numberClicked == currentTargetNumber)
        {
            // SUCCESS! They clicked the right one.
            Debug.Log("Correct! Clicked: " + numberClicked);

            // Move to the next number
            currentTargetNumber++;

            // CHECK FOR WIN CONDITION
            if (currentTargetNumber > totalCirclesToSpawn)
            {
                Debug.Log("YOU WIN! All 20 circles clicked.");
                // Restart the game from the beginning
                StartGame();
            }
            else
            {
                // If the game is not won, spawn the next circle
                SpawnNextCircle();
            }
        }
        else
        {
            // FAILURE! They clicked the wrong one (this shouldn't happen in this mode,
            // but it's good to have)
            Debug.Log("Wrong! Clicked " + numberClicked + ", expected " + currentTargetNumber);

            // Find the circle they were *supposed* to click and destroy it
            ClickableCircle targetCircle = FindObjectOfType<ClickableCircle>();
            if (targetCircle != null)
            {
                Destroy(targetCircle.gameObject);
            }

            // Restart the game
            StartGame();
        }
    }
}