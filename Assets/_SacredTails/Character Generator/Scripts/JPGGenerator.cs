using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;

public class JPGGenerator : MonoBehaviour {
    public GameObject loadingPanel;
    public Button generator;
    public Camera _camera;
    public CharacterDatabase characterDatabase;

    private Camera Camera => _camera == null ? _camera = Camera.main : _camera;

    public void GenerateRandomJPG() { 
        
    }
    
    public void GenerateRandomPNG() { 
        
    }

    private void GernerateImage() {
        generator.interactable = false;

        generator.interactable = true;
    }


    public void Capture() {
        Camera.gameObject.SetActive(true);
        RenderTexture activeRenderTexture = RenderTexture.active;
        RenderTexture.active = Camera.targetTexture;

        Camera.Render();

        Texture2D image = new Texture2D(Camera.targetTexture.width, Camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, Camera.targetTexture.width, Camera.targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = activeRenderTexture;

        byte[] bytes = image.EncodeToPNG();
        Destroy(image);

        string persistenDirectoryPath = $"{Application.persistentDataPath}/{characterDatabase.CaptureImagePath}/";
        if (!Directory.Exists(persistenDirectoryPath)) {
            Directory.CreateDirectory(persistenDirectoryPath);
        }

        string filepath = Application.persistentDataPath + "/Screenshots/" + System.DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + ".png";
        File.WriteAllBytes(filepath, bytes);

        _camera.gameObject.SetActive(false);
        Save(filepath);
    }

    void Save(string filePath) {
        //new NativeShare().AddFile(filePath)
        //    .SetCallback((result, shareTarget) => SacredTailsLog.LogMessage("Share result: " + result + ", selected app: " + shareTarget))
        //    .Share();
    }
}
