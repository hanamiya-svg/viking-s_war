using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityRoyale
{
    public class StartMenuContorller : MonoBehaviour
    {
        // Start is called before the first frame update
        public void OnStartClick()
        {
            SceneManager.LoadScene("Main");
        }

        public void OnExitClick()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}
