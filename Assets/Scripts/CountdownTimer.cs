using System.Collections;
using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public int countdownTime = 3; // Time for the countdown, set in inspector
    public TextMeshProUGUI countdownDisplay; // TextMesh Pro UI for countdown display
    public AudioClip countdownSound; // Sound for each countdown number
    public AudioClip goSound; // Sound for "Go!"
    public AudioSource audioSource; // AudioSource to play the sounds

    private void Start()
    {
        StartCoroutine(CountdownToStart());
    }

    IEnumerator CountdownToStart()
    {
        // Loop through the countdown numbers
        for (int i = countdownTime; i > 0; i--)
        {
            countdownDisplay.text = i.ToString();
            if (countdownSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(countdownSound);
            }
            yield return new WaitForSeconds(1f);
        }

        // Show "Go!" at the end of the countdown
        countdownDisplay.text = "Go!";
        if (goSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(goSound);
        }

        // Hide the countdown display after 1 second
        yield return new WaitForSeconds(1f);
        countdownDisplay.gameObject.SetActive(false);
    }
}
