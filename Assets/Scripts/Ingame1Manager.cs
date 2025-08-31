using UnityEngine;
using UnityEngine.Video;

public class Ingame1Manager : MonoBehaviour
{
    // 각 스테이지별 전환 비디오를 순서대로 할당합니다.
    // 예: Element 0 -> Stage 1 비디오, Element 1 -> Stage 2 비디오
    public VideoClip[] stageTransitionVideos;

    // 각 스테이지 선택 버튼에서 이 함수를 호출합니다.
    // stageIndex는 0부터 시작합니다 (예: 1스테이지 버튼은 0, 2스테이지 버튼은 1을 전달).
    public void StartNegotiationForStage(int stageIndex)
    {
        // "Ingame2"는 실제 다음 씬 이름으로 변경해야 할 수 있습니다.
        if (stageTransitionVideos != null && stageIndex >= 0 && stageIndex < stageTransitionVideos.Length)
        {
            VideoClip videoToPlay = stageTransitionVideos[stageIndex];
            if (videoToPlay != null)
            {
                TransitionManager.LoadSceneWithVideo("Ingame2", videoToPlay);
            }
            else
            {
                Debug.LogError($"Video for stage index {stageIndex} is not assigned.");
            }
        }
        else
        {
            Debug.LogError($"Stage index {stageIndex} is out of bounds or stageTransitionVideos array is not set up.");
        }
    }
}
