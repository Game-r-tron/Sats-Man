using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.IO;
using System.Text;
using TMPro;
using SimpleJSON;
using ZXing;
using ZXing.QrCode;


public class ScoreManager : MonoBehaviour
{
    [Serializable]
    public class Player
    {
        public int score_value;
        public string twitter_handle;
        public string eventID;
        public string id;
        public string bolt11Invoice;
    }

    public Text scoreText;
    public TMP_InputField twitterField;
    public TMP_InputField eventField;
    public String url;
    public Player player = new Player();
    public Image qrCodeImage;
    public Sprite doge;
    public Image bolt;
    public string token ="SECRET";
    
  
    private void Awake()
    {
        player.score_value = PlayerPrefs.GetInt("Player Score");
        scoreText.text = player.score_value.ToString();
        bolt.enabled = false;
        StartCoroutine(PostScore());
    }    

    public IEnumerator CheckPayment()
    {     
        string checkPaymentUrl = "https://games.gamertron.net/satsman-scoreboard/payment-check";

        yield return new WaitForSeconds(2);

        //Build JSON
        string CheckPaymentJSON = "{\"id\": \"" + player.id + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(CheckPaymentJSON);
        
        //Make POST
        var request = new UnityWebRequest (checkPaymentUrl, "POST");
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");
        request.SetRequestHeader("Authorization",token);
        yield return request.SendWebRequest();

        //Handle Response
        //if (request.error != null)
        //{
        //    Debug.Log("Error: " + request.error);
        //}
        //else
        //{
            //Get Response Code
            Debug.Log("Status Code: " + request.responseCode);
            if (request.responseCode != 200)
            {
                StartCoroutine(CheckPayment());
            }
            else
            {
                qrCodeImage.enabled = false;
                bolt.enabled = false;
            }
        //}
    }

    
    public void PaymentMade()
    {
        player.twitter_handle = twitterField.text.TrimStart('@');
        player.eventID = eventField.text;
        qrCodeImage.sprite = doge;
        qrCodeImage.enabled = true;
        StartCoroutine(PaymentMadePost());
    }

    public IEnumerator PaymentMadePost()
    {
        string paymentMadeUrl = "https://games.gamertron.net/satsman-scoreboard/payment-made";

        //Build JSON
        string PaymentMadeJSON = "{\"id\": \"" + player.id + "\"," + "\"twitter_handle\": \"" + player.twitter_handle + "\"," + "\"event\": \"" + player.eventID + "\"}";
        Debug.Log(PaymentMadeJSON);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(PaymentMadeJSON);
		
        //Make POST
        var request = new UnityWebRequest (paymentMadeUrl, "POST");
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");
        request.SetRequestHeader("Authorization",token);
        yield return request.SendWebRequest();

        //Handle Response
        if (request.error != null)
        {
            Debug.Log("Error: " + request.error);
        }
        else
        {
            Debug.Log("Status Code: " + request.responseCode);
        }
    }


    public void Replay()
    {
        SceneManager.LoadScene("Pacman");
    }

    public IEnumerator PostScore()
    {
        string scoreUrl = "https://games.gamertron.net/satsman-scoreboard/api";

        //Build JSON
        string jsonRequest = "{\"score_value\": " + scoreText.text + "}";
        Debug.Log(jsonRequest);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
		
        //Make POST
        var request = new UnityWebRequest (scoreUrl, "POST");
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");
        request.SetRequestHeader("Authorization",token);
        yield return request.SendWebRequest();

        //Handle Response
        if (request.error != null)
        {
            Debug.Log("Error: " + request.error);
        }
        else
        {
            Debug.Log("Status Code: " + request.responseCode);
            
            //Get Data
            JSONNode jsonObj = JSON.Parse(request.downloadHandler.text);
            player.id = jsonObj["id"].Value;
            player.bolt11Invoice = jsonObj["bolt11_invoice"].Value;
            Debug.Log("Player ID: " + player.id.ToString() + System.Environment.NewLine + "Bolt11 Invoice: " + player.bolt11Invoice.ToString());

            Texture2D tex = generateQR(player.bolt11Invoice);
            qrCodeImage.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            bolt.enabled = true;

            StartCoroutine(CheckPayment());
        }
	}

    public Texture2D generateQR(string text)
    {
        var encoded = new Texture2D(256, 256);
        var color32 = Encode(text, encoded.width, encoded.height);
        encoded.SetPixels32(color32);
        encoded.Apply();
        return encoded;
    }

    private static Color32[] Encode(string textForEncoding, int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }
}