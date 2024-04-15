using UnityEngine;

public class FpsLock : MonoBehaviour
{
    public int frameRate;
    
    private void Update()
    {
        frameRate = PlayerPrefs.GetInt("fpsValue");
        Application.targetFrameRate = frameRate;
    }
}
