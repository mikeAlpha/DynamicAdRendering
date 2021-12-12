using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using LitJson;

public class Parser : MonoBehaviour
{
    public Text _textField;
    public RawImage _frameField;
    private string DisplayInfo = "Order Now!";

    private void Start()
    {
        //StartCoroutine(ConnectWebService()); 
    }

    public void ButtonAction(string type_name)
    {
        string url = "http://lab.greedygame.com/arpit-dev/unity-assignment/templates/" + type_name + ".json";
        StartCoroutine(ConnectWebService(url));
    }

    IEnumerator ConnectWebService(string url)
    {
        print(url);

        using(UnityWebRequest wData = UnityWebRequest.Get(url))
        {
            yield return wData.SendWebRequest();
            if(wData.isNetworkError || wData.isHttpError)
            {
                print("Url Error...Please check " + wData.error);
            }
            else if (wData.isDone)
            {
                JsonData jData = JsonMapper.ToObject(wData.downloadHandler.text);
                if(jData == null)
                {
                    print("No Data");
                }
                else
                {
                    ParseData(jData);
                }
            }
        }
    }

    private void ParseData(JsonData data)
    {
        string type = data["layers"][0]["type"].ToString();

        switch (type)
        {
            case "text":
                SetTextInternal(data);
                break;
            case "frame":
                SetFrameInternal(data);
                break;
        }
    }

    private void SetTextInternal(JsonData data)
    {
        var posDetails = data["layers"][0]["placement"][0]["position"];

        int posx   =      Int32.Parse(posDetails["x"].ToString());
        int posy   =      Int32.Parse(posDetails["y"].ToString());
        int width  =  Int32.Parse(posDetails["width"].ToString());
        int height = Int32.Parse(posDetails["height"].ToString());

        print("text " + posx + " " + posy + " " + width + " " + height);

        _frameField.gameObject.SetActive(false);
        _textField.gameObject.SetActive(true);

        RectTransform rectT = _textField.GetComponent<RectTransform>();
        rectT.anchoredPosition = new Vector2(posx, posy);
        rectT.sizeDelta = new Vector2(width, height);

        if (!data["layers"][0].ContainsKey("operations"))
        {
            _textField.color = Color.black;
            print("No operations");
        }
        else
        {
            var opname = data["layers"][0]["operations"][0]["name"].ToString();
            if (opname == "color")
            {
                var value = data["layers"][0]["operations"][0]["argument"].ToString();

                Color c;
                if (ColorUtility.TryParseHtmlString(value, out c))
                {
                    _textField.color = c;
                }
            }
        }

        _textField.text = DisplayInfo;
    }

    private void SetFrameInternal(JsonData data)
    {
        _textField.gameObject.SetActive(false);
        _frameField.gameObject.SetActive(true);

        var frameDetails = data["layers"][0];
        StartCoroutine(DownloadAndSetImage(frameDetails["path"].ToString() , frameDetails));
    }

    private IEnumerator DownloadAndSetImage(string url, JsonData data)
    {
        print(url);

        using (UnityWebRequest wData = UnityWebRequestTexture.GetTexture(url))
        {
            yield return wData.SendWebRequest();
            if (wData.isNetworkError || wData.isHttpError)
            {
                print("Url Error...Please check " + wData.error);
            }
            else if (wData.isDone)
            {
                var posDetails = data["placement"][0]["position"];

                int posx = Int32.Parse(posDetails["x"].ToString());
                int posy = Int32.Parse(posDetails["y"].ToString());
                int width = Int32.Parse(posDetails["width"].ToString());
                int height = Int32.Parse(posDetails["height"].ToString());

                print("frame " + posx + " " + posy + " " + width + " " + height);

                RectTransform rectT = _frameField.GetComponent<RectTransform>();
                rectT.anchoredPosition = new Vector2(posx, posy);
                rectT.sizeDelta = new Vector2(width, height);

                Texture tex = DownloadHandlerTexture.GetContent(wData);
                _frameField.texture = tex;

                if (!data.ContainsKey("operations"))
                {
                    _frameField.color = Color.white;
                    print("No operations");
                }
                else
                {
                    var opname = data["operations"][0]["name"].ToString();
                    if (opname == "color")
                    {
                        var value = data["operations"][0]["argument"].ToString();

                        Color c;
                        if (ColorUtility.TryParseHtmlString(value, out c))
                        {
                            c.a = 255;
                            _frameField.color = c;
                        }
                    }
                }
            }
        }
    }
}
