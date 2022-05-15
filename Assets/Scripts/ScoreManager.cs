using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.IO;
using System.Text;
using TMPro;


public class ScoreManager : MonoBehaviour
{
    [Serializable]
    public class Player
    {
        
        public int score_value;
        public String score_date;
        public string twitter_handle;

    }

    public Text scoreText;
    public TextMeshProUGUI twitterField;
    public String url;
    public Player player = new Player();
  
    private void Awake()
    {
        player.score_value = PlayerPrefs.GetInt("Player Score");
        player.score_date = DateTime.Now.ToString("s");
        scoreText.text = player.score_value.ToString();
    }

    private void Start()
    {
        //Submit();
        //Invoke("Replay", 2);
    }

    public void Submit()
    {
        player.twitter_handle = twitterField.text;
        postScore();
    }

    private void Replay()
    {
        SceneManager.LoadScene("Pacman");
    }

    private void postScore()
    {
		string url = "http://localhost:8000/scoreboard/api";
		//string myAccessKey = "myAccessKey";
		//string mySecretKey = "mySecretKey";

        //Convert to JSON
        string jsonData = JsonUtility.ToJson(player, true);
        Debug.Log(jsonData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
		
        var request = new UnityWebRequest (url, "POST");
    
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
    
        request.SendWebRequest();

        if (request.error != null)
        {
            Debug.Log("Error: " + request.error);
        }
        else
        {
            Debug.Log("Status Code: " + request.responseCode);
        }
	}

}