using System.Collections;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
public class ModelResponse
{
    public Choice[] choices;
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

    [SerializeField]
    InputActionReference _enter;
    [SerializeField]
    private GameObject inputFieldObject;
    [SerializeField]
    private Obstacles obstacles;
    private List<Message> chatLog = new List<Message>();
    public void SendModelRequest()
    {
        StartCoroutine(MakeRequest());
    }

    public string SystemMessage = "";

    IEnumerator MakeRequest()
    {
        ModelRequest modelRequest = new ModelRequest
        {
            model = "D:\\Development\\python\\text-generation-webui-main\\models\\zephir-7b-beta",
            messages = chatLog.ToArray(),
            max_tokens = "64",
            temperature = "0.3",
            top_p = "1.0",
            top_k = "50",
            // stream = "false",
            presence_penalty = "0.0",
            repetition_penalty = "0.0",
            repetition_penalty_range = "512",
            guidance_scale = "1.0"
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
                chatLog.Add(response.choices[0].message);
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

    private static bool parseCommand(ModelResponse response)
    {
        // parse response for { } pattern. if it includes "directional_light.enabled = false" disable gameobject
        var command = response.choices[0].message.content;
        var commandStart = command.IndexOf("{");
        var commandEnd = command.IndexOf("}");
        var action = command.Substring(commandStart + 1, commandEnd - commandStart - 1);
        var targetObject = action.Split(".")[0].Trim();
        var targetProperty = action.Split(".")[1].Trim();
        var targetValue = action.Split("=").ElementAt(1).Trim();
        // TODO: Error handling if targetObject not present in Obstacles.Children, redo request
        Debug.Log(targetObject);
        Debug.Log(targetProperty);
        Debug.Log(targetValue);
        if (targetObject == action.Trim() || targetProperty == action.Trim() || targetValue == action.Trim())
        {
            // no action
            return false;
        }
        return true;
    }

    // Start is called before the first frame update
    void Start()
    {
        chatLog.Add(new Message { role = "system", content = SystemMessage });
        chatLog.Add(new Message { role = "user", content = "Hello. Please move the blue cube a little bit and scale it by half." });
        chatLog.Add(new Message { role = "assistant", content = "I can only do one thing, but here: Poof! Let the blue float along! { \"objects\": \"blue cube\", \"operation\": \"TRANSLATE\", \"value\": 2 }" });

        // GetExchangeRates();
        _enter.action.performed += ctx =>
        {
            Debug.Log(inputFieldObject.active);
            switch (inputFieldObject.active)
            {
                case true:
                    chatLog.Add(new Message { role = "user", content = inputFieldObject.GetComponent<TMP_InputField>().text });
                    SendModelRequest();
                    inputFieldObject.GetComponent<TMP_InputField>().text = "";
                    inputFieldObject.SetActive(false);

                    break;
                case false:
                    Debug.Log("Set active");
                    inputFieldObject.SetActive(true);
                    inputFieldObject.GetComponent<TMP_InputField>().ActivateInputField();
                    break;
            }
        };
    }

    // Update is called once per frame
    void Update()
    {

    }
}
