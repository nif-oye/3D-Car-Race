using UnityEngine;

public class HoverAnimation : MonoBehaviour
{
    public float hoverSpeed = 2f; // Speed of the hover effect
    public float hoverHeight = 10f; // Height of the hover

    private Vector3 originalPosition;

    void Start()
    {
        // Store the original position of the button
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        // Calculate the new Y position using a sine wave for smooth up and down movement
        float newY = originalPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;

        // Apply the new position to the button
        transform.localPosition = new Vector3(originalPosition.x, newY, originalPosition.z);
    }
}
