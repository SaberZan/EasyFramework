using UnityEditor;
using UnityEngine;

public class CameraCapture : EditorWindow
{
    [MenuItem("Easy/摄像机截图")]
    private static void OpenAssetWindow()
    {
        var win = GetWindow<CameraCapture>();
    }

    private Camera m_cam = null;
    private int m_width = 1920;
    private int m_height = 1080;

    void OnGUI()
    {
        m_cam = (Camera)EditorGUILayout.ObjectField("选择摄像机", m_cam, typeof(Camera), true, null);
        m_width = EditorGUILayout.IntField("宽度", m_width);
        m_height = EditorGUILayout.IntField("高度", m_height);

        if (m_cam != null && GUILayout.Button("截图"))
        {
            captureCamera(m_cam);
        }
    }

    private void captureCamera(Camera camera)
    {
        RenderTexture rt = new RenderTexture(m_width, m_height, 0);
        // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机
        camera.targetTexture = rt;
        camera.Render();

        // 激活这个rt, 并从中中读取像素。
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D(m_width, m_height, TextureFormat.RGB24,false);
        Rect rect = new Rect(0, 0, m_width, m_height);
        screenShot.ReadPixels(rect, 0, 0);// 注：这个时候，它是从RenderTexture.active中读取像素
        screenShot.Apply();

        // 重置相关参数，以使用camera继续在屏幕上显示
        camera.targetTexture = null;
        //ps: camera2.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        GameObject.DestroyImmediate(rt);
        // 最后将这些纹理数据，成一个png图片文件
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = Application.dataPath + "/Screenshot.png";
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("截屏了一张照片: {0}", filename));
    }
}
