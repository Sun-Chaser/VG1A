// ResultsController.cs
using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultsController : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text highestScoreText;
    public TMP_Text countdownText; 
    public float returnDelay = 10f;
    public AudioSource audioSource;
    public AudioClip bgm;

    private IEnumerator Start()
    {
        // Play music
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = bgm;
        audioSource.loop = false;
        audioSource.Play();
        
        // Show score
        if (scoreText != null)
            scoreText.text = $"Your Score: {GameController.instance.score}";

        // Show highest score (default 0 if none)
        if (highestScoreText != null)
            highestScoreText.text = $"Highest Score: {PlayerPrefs.GetInt("HighestScore", 0)}";

        // Countdown (unscaled time so it works even if Time.timeScale = 0)
        if (countdownText != null)
        {
            float t = returnDelay;
            while (t > 0f)
            {
                int secs = Mathf.CeilToInt(t);
                countdownText.text = $"Returning to Menu in {secs}...";
                t -= Time.unscaledDeltaTime;
                yield return null; // update every frame
            }
        }
        else
        {
            // No countdown text assigned? Just wait the same duration (real time).
            yield return new WaitForSecondsRealtime(returnDelay);
        }

        SceneManager.LoadScene("MainMenu");
    }
}