// TransitionManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public static class TransitionManager
{
    public static VideoClip videoToPlay;
    public static string sceneToLoad;

    public static void LoadSceneWithVideo(string sceneName, VideoClip videoClip)
    {
        sceneToLoad = sceneName;
        videoToPlay = videoClip;
        SceneManager.LoadScene("VideoTransitionScene"); // 동영상 재생 씬의 이름
    }
}