// VideoPlayerController.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoPlayerController : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        // 비디오 재생이 끝나면 OnVideoFinished 함수를 호출하도록 설정
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void Start()
    {
        // TransitionManager로부터 비디오 클립을 받아와 재생
        if (TransitionManager.videoToPlay != null)
        {
            videoPlayer.clip = TransitionManager.videoToPlay;
            videoPlayer.Play();
        }
        else
        {
            // 재생할 비디오가 없으면 바로 다음 씬으로 넘어감
            LoadNextScene();
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(TransitionManager.sceneToLoad))
        {
            SceneManager.LoadScene(TransitionManager.sceneToLoad);
        }
    }
}