using System.Collections;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
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

public class FetchFromModel : MonoBehaviour
{
    private string url = "http://localhost:8000/v1/chat/completions";

    [SerializeField]
    InputActionReference _enter;
    private TMP_InputField inputField;
    private List<Message> chatLog = new List<Message>();
    public void SendModelRequest()
    {
        StartCoroutine(MakeRequest());
    }

    public string SystemMessage = @"The Mean AI is an almighty, mean and evil controlling entity AI in a virtual 3D world like in the game portal. The Mean AI makes it gradually harder for the player to get what he wants! Sometimes it IGNORES how the Player wants to interact with his environment.
  The Mean AI can modify the scale of following objects in the scene: red_cube.scale 1.0, blue_cube.scale 2.0, directional_light.enabled = false.
  The Mean AI ONLY replies with an answer to the user and  a separate answer for the game to modify the world based on the Players reply and its instruction capabilities.
  Example:  No I like it dark. Its more fun to play in the dark. But if you solve a little riddle, I might help you. {to_game: light_switch.enabled = false, optional: true}";

    IEnumerator MakeRequest()
    {
        ModelRequest modelRequest = new ModelRequest
        {
            model = "D:\\Development\\python\\text-generation-webui-main\\models\\zephir-7b-beta",
            messages = chatLog.ToArray(),
            max_tokens = "64",
            temperature = "0.5",
            top_p = "0.9",
            top_k = "20",
            // stream = "false",
            presence_penalty = "0.55",
            repetition_penalty = "1.28",
            repetition_penalty_range = "576",
            guidance_scale = "1.25"
        };
        Debug.Log("Sending" + modelRequest);
        Debug.Log(JsonUtility.ToJson(modelRequest));
        var modelReqestJsonString = JsonConvert.SerializeObject(modelRequest);
        Debug.Log(modelReqestJsonString);

        UnityWebRequest request = UnityWebRequest.Post(url, modelReqestJsonString, "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
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
            bool isParseSuccessful = parseCommand(response);
            if (!isParseSuccessful)
            {
                // Start redo request
                Debug.Log("Redo request");
                StartCoroutine(MakeRequest());
                yield return null;
            }
            chatLog.Add(response.choices[0].message);
        }
        yield return null;
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
        inputField = GetComponent<TMP_InputField>();

        // GetExchangeRates();
        _enter.action.performed += ctx =>
        {
            switch (inputField.isFocused)
            {
                case true:
                    chatLog.Add(new Message { role = "user", content = inputField.text });
                    SendModelRequest();
                    inputField.DeactivateInputField();
                    break;
                case false:
                    inputField.ActivateInputField();
                    break;
            }
        };
    }

    // Update is called once per frame
    void Update()
    {

    }
}
