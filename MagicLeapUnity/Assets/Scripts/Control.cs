using System;
using System.Collections;
using UnityEngine;
using MagicLeap;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine.Networking;
using UnityEngine.XR.MagicLeap;
using Random = System.Random;

public class Control : MonoBehaviour
{
    public Texture2D imageTarget;

    private bool _imageObjectAdded = false;

    private List<Tuple<string, Vector3, AssetBundle>> _assetBundles = new List<Tuple<string, Vector3, AssetBundle>>();

    IEnumerator DownloadAssets(string assetName, Vector3 pos)
    {
        
        DateTime start = DateTime.UtcNow;
        string url = "http://vcm-12481.vm.duke.edu/static/" + assetName;
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url, 0);
        yield return request.SendWebRequest();
        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
        TimeSpan timeDiff = DateTime.UtcNow - start;
        Debug.Log("Downloading" + assetName + "takes" + Convert.ToInt32(timeDiff.TotalMilliseconds));
        
        _assetBundles.Add(new Tuple<string, Vector3, AssetBundle>(assetName, pos, bundle));
    }
    
    void DisplayAssets()
    {
        foreach (var i in _assetBundles)
        {
            DateTime start = DateTime.UtcNow;
            
            var assetName = i.Item1;
            var pos = i.Item2;
            var bundle = i.Item3;
            GameObject gameObject = bundle.LoadAsset<GameObject>(assetName);
            gameObject = Instantiate(gameObject);
            gameObject.transform.position = pos;
            gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            TimeSpan timeDiff = DateTime.UtcNow - start;
            Debug.Log("render time" + assetName + "takes" + Convert.ToInt32(timeDiff.TotalMilliseconds));
        }
    }

    IEnumerator HttpLongPollNotification(string uri)
    {
        Debug.Log("Waiting for push notification");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log("Long poll Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Notification received. Start downloading");
                
                var objList = new List<KeyValuePair<string, Vector3>>()
                {
                    new KeyValuePair<string, Vector3>("bunny", new Vector3(-1.5f, 0f, 0f)),
                    new KeyValuePair<string, Vector3>("lucy", new Vector3(1.5f, 0f, 0f)),
                    new KeyValuePair<string, Vector3>("dragon", new Vector3(0.0f, 0.0f, 1.5f)),
                };

                foreach (var i in objList)
                {
                    StartCoroutine(DownloadAssets(i.Key, i.Value));
                }
                
            }

        }
    }

    IEnumerator Capture()
    {
        while (true)
        {
            MLCamera.CaptureRawImageAsync();
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private string RandomString(int length)
    {
        Random random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    IEnumerator Upload(byte[] jpegBytes)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        string fileName = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz") + RandomString(5) + ".jpg";
        formData.Add(new MultipartFormFileSection(fileName, jpegBytes));

        UnityWebRequest www = UnityWebRequest.Post("http://vcm-9794.vm.duke.edu/upload", formData);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("File upload complete!");
        }
    }


    void Start()
    {

        StartCoroutine(HttpLongPollNotification("http://vcm-12481.vm.duke.edu/push/"));

        MLImageTracker.Start();
        MLImageTracker.Enable();
        MLImageTracker.AddTarget("sea", imageTarget, 0.279f, (target, result) =>
        {
            if (!_imageObjectAdded && result.Status == MLImageTargetTrackingStatus.Tracked)
            {
                Debug.Log("Image recognized.");
                
                DisplayAssets();
                
                _imageObjectAdded = true;
            }
        }, true);
        
        MLResult camera = MLCamera.Start();
        if (camera.IsOk)
        {
            MLCamera.Connect();
            Debug.Log("Camera Enabled");
        }

//        MLCamera.OnRawImageAvailable += delegate(byte[] jpegBytes) { StartCoroutine(Upload(jpegBytes)); };
//        StartCoroutine(Capture());
        
        
    }

    private void Update()
    {
        Debug.Log("Not downloading fps:" + 1.0 / Time.deltaTime);
        
//        if (_canStart)
//        {
//            StartCoroutine(GetRequest("http://vcm-9794.vm.duke.edu/static/heic0406a.tif"));
//            _canStart = false;
//        }
//
    }

    private void OnDestroy()
    {
        MLImageTracker.Stop();
        MLImageTracker.Disable();
    }
}