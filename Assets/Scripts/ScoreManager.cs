using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public Text finalScoreText;
    public int finalScore { get; private set; }

    private void Awake()
    {
        finalScore = PlayerPrefs.GetInt("Player Score");
    }

    private void Start()
    {
        finalScoreText.text = finalScore.ToString();
        Invoke("Replay", 2);
    }

    private void Update()
    {
 
    }

    private void Replay()
    {
        SceneManager.LoadScene("Pacman");
    }
}