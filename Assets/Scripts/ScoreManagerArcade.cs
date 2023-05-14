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

public class ScoreManagerArcade : MonoBehaviour
{
    [Serializable]
    public class Player
    {
        public int score_value;
        public string eventID;
        public string id;
        public string bolt11Invoice;
    }

    public Text scoreText;
    public Text payText;
    public Text submitText;
    public String url;
    public Player player = new Player();
    public Image qrCodeImage;
    public Image boltImage;
    public Image cameraImage;
    string token;
  
    private void Start()
    { 
        player.score_value = PlayerPrefs.GetInt("Player Score");
        scoreText.text = player.score_value.ToString();
        token = ApiToken.Value;
        boltImage.enabled = false;
        cameraImage.enabled = false;
        payText.enabled = false;
        submitText.enabled = false;

        StartCoroutine(PostScore());
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
            boltImage.enabled = true;
            payText.enabled = true;

            StartCoroutine(CheckPayment());
        }
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
                PaymentMade();
            }
        //}
    }

    public void PaymentMade()
    {
        boltImage.enabled = false;
        payText.enabled = false;
        
        string paymentUrl = "https://games.gamertron.net/satsman-scoreboard/enter-details/" + player.id + "/miami23";
        Debug.Log("paymentUrl: " + paymentUrl);
        
        //Construct new QR Code
        Texture2D tex = generateQR(paymentUrl);
        qrCodeImage.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        cameraImage.enabled = true;
        submitText.enabled = true;
    }

    public void Replay()
    {
        SceneManager.LoadScene("Pacman");
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

    public void OpenLightningURI()
    {
        string uri = "lightning:" + player.bolt11Invoice.ToString();
               
        Application.OpenURL(uri);
    }

    public void OpenPaymentURL()
    {     
        string paymentUrl = "https://games.gamertron.net/satsman-scoreboard/enter-details/" + player.id;

        Application.OpenURL(paymentUrl);

    }
}