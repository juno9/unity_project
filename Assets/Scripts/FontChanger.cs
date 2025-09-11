using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필요

public class FontChanger : MonoBehaviour
{
    public TMP_FontAsset newFontAsset; // 인스펙터에서 할당할 새 폰트 에셋

    [ContextMenu("Change All Fonts in Scene")]
    void ChangeAllFonts()
    {
        if (newFontAsset == null)
        {
            Debug.LogError("새 폰트 에셋이 할당되지 않았습니다. 인스펙터에서 'New Font Asset' 필드에 폰트 에셋을 할당해주세요.");
            return;
        }

        // 씬의 모든 TextMeshProUGUI 컴포넌트를 찾습니다.
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();

        foreach (TextMeshProUGUI textMesh in allTexts)
        {
            if (textMesh != null)
            {
                textMesh.font = newFontAsset;
                // 필요하다면 Material Preset도 변경할 수 있습니다.
                // textMesh.fontSharedMaterial = newFontAsset.material;
            }
        }
        Debug.Log("모든 TextMeshProUGUI 폰트 변경 완료!");
    }
}
