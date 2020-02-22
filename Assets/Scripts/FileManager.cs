using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FileManager : MonoBehaviour
{
    public RawImage rawImage;
    // Start is called before the first frame update
    string folderPath;
    bool startChecking = false;
    float countingInterval = 1f;
    float time = 0;
    long lastCount=-1;
    string screenshotLocation;
    void Start()
    {
        folderPath = Application.persistentDataPath;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            folderPath = EditorUtility.OpenFolderPanel("Save Directory", "", "");
            startChecking = true;
            Debug.Log(folderPath);
        }

        if(Input.GetKeyDown(KeyCode.Return))
        {
            lastCount++;
            StartCoroutine(CRSaveScreenshot());
        }
        if (startChecking)
        {
            time += Time.deltaTime;
            if (time > countingInterval)
            {
                time = 0;
                DirectoryInfo d = new DirectoryInfo(folderPath);
                long amt = DirCount(d);
                FileInfo[] fis = d.GetFiles();

                if (lastCount < 0)
                    lastCount = amt;
                else if (lastCount != amt)
                {
                    StartCoroutine(LoadLatestFile(d));

                    lastCount = amt;
                }


            }
        }
       
    }
    public static long DirCount(DirectoryInfo d)
    {
        long i = 0;
        // Add file sizes.
        FileInfo[] fis = d.GetFiles();
        foreach (FileInfo fi in fis)
        {
            if (fi.Extension.Contains("png"))
                i++;
        }
        return i;
    }

    IEnumerator CRSaveScreenshot()
    {
        yield return new WaitForEndOfFrame();

        string fileName = "Screenshot" +System.DateTime.Now.Day+System.DateTime.Now.Month+System.DateTime.Now.Year+ System.DateTime.Now.Hour + System.DateTime.Now.Minute + System.DateTime.Now.Second + ".png";
        screenshotLocation = folderPath +"/"+ fileName;
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }


        Camera currentCamera = Camera.main;
        int tw = 2048; 
        int th = 2048; 
        RenderTexture rt = new RenderTexture(tw, th, 24, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 4;

        currentCamera.targetTexture = rt;

        currentCamera.Render();//

        //Create the blank texture container
        Texture2D thumb = new Texture2D(tw, th, TextureFormat.RGB24, false);

        //Assign rt as the main render texture, so everything is drawn at the higher resolution
        RenderTexture.active = rt;

        //Read the current render into the texture container, thumb
        thumb.ReadPixels(new Rect(0, 0, tw, th), 0, 0, false);

        byte[] bytes = thumb.EncodeToPNG();
        Object.Destroy(thumb);

        File.WriteAllBytes(screenshotLocation, bytes);

        RenderTexture.active = null;

        currentCamera.targetTexture = null;

        rt.DiscardContents();

        StartCoroutine(WaitAndOpen(screenshotLocation));

        IEnumerator WaitAndOpen(string path)
        {
            yield return new WaitUntil(() => File.Exists(path));
            Application.OpenURL(path);
        }
    }

    IEnumerator LoadLatestFile(DirectoryInfo d)
    {
        FileInfo[] fis = d.GetFiles();
        string dir = fis[fis.Length - 1].FullName;
        Debug.Log(dir);
        UnityWebRequest www = UnityWebRequest.Get(dir);
        yield return www.SendWebRequest();

        if (!www.isNetworkError && !www.isHttpError)
        {

            Texture2D texture2D = new Texture2D(2048, 2048);
            texture2D.LoadImage(www.downloadHandler.data);
            rawImage.gameObject.SetActive(true);
            rawImage.texture = texture2D;
            //playersManager.CheckAndCreateCharacter(texture2D, isRandomPos);
        }

    }

    public void CloseImage()
    {
        rawImage.gameObject.SetActive(false);
    }
}
