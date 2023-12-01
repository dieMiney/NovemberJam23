using System.Collections;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using System;
using Michsky.MUIP;
public class ModelResponse
{
    public Choice[] choices;
}
public class ModelResponseModel
{
    public string id;
    public string root;
}

public class ModelsResponse
{
    public ModelResponseModel[] data;
}

public class Message
{
    public string role;
    public string content;
}

public class Choice
{
    // Example response from model
    // "choices": [
    //     {
    //         "index": 0,
    //         "message": {
    //             "role": "assistant",
    //             "content": "Hello! I do not have a physical body, so I do not have a name in the traditional sense. However, I am programmed to respond to various names, including \"assistant,\" \"AI,\" \"bot,\" or any other name you prefer to use when addressing me.\n"
    //         },
    //         "finish_reason": "stop"
    //     }
    // ],
    public int index;
    public Message message;
    public string finish_reason;
}

public class ModelRequest
{
    // Example request to model
    // {
    // "model": "D:\\Development\\python\\text-generation-webui-main\\models\\zephir-7b-beta",
    // "messages": [{"role": "user", "content": "Hello! What is your name?"}],
    // "temperature": 0.75,
    // "top_p": 0.9,
    // "top_k": 20,
    // "repetition_penalty": 1.28,
    // "presence_penalty": 0.55,
    // "repetition_penalty_range": 576,
    // "guidance_scale": 1.25
    // }
    public ModelRequest()
    {
        // default values
        model = "D:\\Development\\python\\text-generation-webui-main\\models\\zephir-7b-beta";
        messages = new Message[] { new Message { role = "user", content = "Hello! What is your name?" } };
        max_tokens = "64";
        temperature = "0.75";
        top_p = "0.9";
        top_k = "20";
        // stream = "false";
        presence_penalty = "0.55";
        repetition_penalty = "1.28";
        repetition_penalty_range = "576";
        guidance_scale = "1.25";
    }

    public string model;
    public Message[] messages;
    public string max_tokens;
    public string temperature;
    public string top_p;
    public string top_k;
    // public string stream { get; set; } // true or false
    public string presence_penalty;
    public string repetition_penalty;
    public string repetition_penalty_range;
    public string guidance_scale;

    // return json for web request
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

}
public class GameAction
{
    [JsonProperty("objects")]
    public string Objects { get; set; }

    [JsonProperty("operation")]
    public string Operation { get; set; }

    [JsonProperty("value")]
    public double Value { get; set; }
}

public class FetchFromModel : MonoBehaviour
{
    private string url = "http://localhost:8000/v1/chat/completions";

    public TextInput playerInputActions;

    [SerializeField]
    private GameObject inputFieldObject;
    [SerializeField]
    private Obstacles obstacles;

    [SerializeField]
    private NotificationManager notificationManager;
    private List<Message> chatLog = new List<Message>();
    private string modelName = "zephyr-7b-beta";
    public void SendModelRequest()
    {
        StartCoroutine(MakeRequest());
    }

    private string SystemMessage = "You are a helpful AI in a virtual game world. You can modify the following GAME OBJECTs: ";
    private string SystemMessageEnd = "You ALWAYS respond to the user and append the action you carried out in the game world FOR THE SYSTEM, which looks something like this in JSON { objects: \"GAME OBJECT\", operation: \"SCALE | ROTATE | TRANSLATE\", value: INT }";

    IEnumerator MakeRequest()
    {
        ModelRequest modelRequest = new ModelRequest
        { // fblgit/juanako-7b-UNA
          //berkeley-nest/Starling-LM-7B-alpha
          // zephyr-7b-alpha
            model = modelName,
            messages = chatLog.ToArray(),
            max_tokens = "128",
            temperature = "0.45",
            top_p = "0.8",
            top_k = "50",
            // stream = "false",
            presence_penalty = "0.0",
            repetition_penalty = "0.0",
            repetition_penalty_range = "512",
            guidance_scale = "1.1"
        };
        Debug.Log("Sending" + modelRequest);
        Debug.Log(JsonUtility.ToJson(modelRequest));
        var modelReqestJsonString = JsonConvert.SerializeObject(modelRequest);
        Debug.Log(modelReqestJsonString);

        UnityWebRequest request = UnityWebRequest.Post(url, modelReqestJsonString, "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error); // model worker not reachable
        }
        else
        {
            Debug.Log("Received" + request.downloadHandler.text);
            // derserialize json
            var response = JsonConvert.DeserializeObject<ModelResponse>(request.downloadHandler.text);

            // var Rates = JsonConvert.DeserializeObject<Rates>(request.downloadHandler.text);

            // exchange = GameObject.Find("MainText").GetComponent<Text>();
            Debug.Log(response.choices[0].message.content);
            // Parse answer for { something } syntax. if it includes "directional_light.enabled = false" disable gameobject
            GameAction gameAction = ParseGameAction(response.choices[0].message.content);

            // remove { something } from answer
            var cleanedResponse = Regex.Replace(response.choices[0].message.content, @"\{(.+?)\}", "");
            StartCoroutine(voiceRequest(cleanedResponse));
            notificationManager.title = "AI";
            notificationManager.description = cleanedResponse;
            notificationManager.Open();

            if (gameAction == null)
            {
                // Start redo request
                Debug.Log("Redo request");
                // StartCoroutine(MakeRequest());
                inputFieldObject.GetComponent<TMP_InputField>().text = "";
                inputFieldObject.SetActive(false);
            }
            else
            {

                Debug.Log($"Objects: {gameAction.Objects}, Operation: {gameAction.Operation}, Value: {gameAction.Value}");
                foreach (var obstacle in obstacles.Children)
                {
                    if (gameAction.Objects.Contains(obstacle.ObstacleName))
                    {
                        switch (gameAction.Operation.ToLower())
                        {
                            case "scale":
                            case "resize":
                            case "grow":
                            case "shrink":
                            case "enlarge":
                                obstacle.transform.localScale = new Vector3((float)gameAction.Value, (float)gameAction.Value, (float)gameAction.Value);
                                break;
                            case "rotate":
                            case "spin":
                            case "turn":
                                obstacle.transform.Rotate(0, (float)gameAction.Value, 0, Space.Self);
                                break;
                            case "move":
                            case "translate":
                            case "moveby":
                            case "translateby":
                            case "shift":
                            case "offset":
                                obstacle.transform.Translate(0, (float)gameAction.Value, 0, Space.Self);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            chatLog.Add(response.choices[0].message);

        }
        yield return null;
    }

    private IEnumerator voiceRequest(string cleanedResponse)
    {
        UnityWebRequest requestSetSentence = UnityWebRequest.Post("http://127.0.0.1:8002/voice", JsonConvert.SerializeObject(new { sentence = cleanedResponse }), "application/json");
        yield return requestSetSentence.SendWebRequest();
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("http://127.0.0.1:8002/voice", AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                // play audio
                AudioSource audioSource = GetComponent<AudioSource>();
                audioSource.clip = myClip;
                audioSource.Play();
            }
        }
        yield return null;
    }

    private static GameAction ParseGameAction(string input)
    {
        Match match = Regex.Match(input, @"\{(.+?)\}");
        if (match.Success)
        {
            string json = "{" + match.Groups[1].Value + "}";
            return JsonConvert.DeserializeObject<GameAction>(json);
        }
        return null;
    }


    // Start is called before the first frame update
    void Start()
    {
        // get available models
        StartCoroutine(GetModels());

        inputFieldObject.SetActive(false);
        // Debug.Log(obstacles.Children.Count);
        var objectNames = obstacles.Children.Select(x => "\"" + x.ObstacleName + "\"").ToArray();
        string objectNamesString = string.Join(", ", objectNames);
        var finalSysteMessage = SystemMessage + objectNamesString + SystemMessageEnd;
        Debug.Log("system message: " + finalSysteMessage);
        chatLog.Add(new Message { role = "system", content = finalSysteMessage });
        chatLog.Add(new Message { role = "user", content = "Hello. Please move the blue cube a little bit and scale it by half." });
        chatLog.Add(new Message { role = "assistant", content = "I will move the blue cube. I dont want to scale it. Deal with it. { \"objects\": \"blue cube\", \"operation\": \"TRANSLATE\", \"value\": 2 }" });

        // GetExchangeRates();
    }

    public void EnterPressed()
    {
        switch (inputFieldObject.active)
        {
            case true:
                if (inputFieldObject.GetComponent<TMP_InputField>().text != "")
                {

                    chatLog.Add(new Message { role = "user", content = inputFieldObject.GetComponent<TMP_InputField>().text });
                    SendModelRequest();
                    inputFieldObject.GetComponent<TMP_InputField>().text = "";
                }
                inputFieldObject.SetActive(false);

                break;
            case false:
                inputFieldObject.SetActive(true);
                inputFieldObject.GetComponent<TMP_InputField>().ActivateInputField();
                break;
        }
    }

    IEnumerator GetModels()
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:8000/v1/models");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error); // model worker not reachable
        }
        else
        {
            Debug.Log("Received" + request.downloadHandler.text);
            // derserialize json
            var response = JsonConvert.DeserializeObject<ModelsResponse>(request.downloadHandler.text);
            Debug.Log(response.data[0].id);
            modelName = response.data[0].id;
        }
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
