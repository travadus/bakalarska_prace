using UnityEngine;

public class QuitGameUI : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}