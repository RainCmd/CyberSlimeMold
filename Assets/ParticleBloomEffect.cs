using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ParticleBloomEffect : MonoBehaviour
{
    public Shader bloomShader;
    private Material bloomMaterial;

    [Range(0, 4)]
    public float bloomBlurSize = 2.0f;

    public Color bloomColorTint = Color.white;

    private Camera m_Camera;
    private Camera Camera
    {
        get
        {
            if (!m_Camera)
                m_Camera = GetComponent<Camera>();
            return m_Camera;
        }
    }

    private Material GetMaterial()
    {
        if (bloomMaterial == null)
        {
            bloomMaterial = new Material(bloomShader);
            bloomMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
        return bloomMaterial;
    }

    private void OnEnable()
    {
        Camera.targetTexture = RenderTexture.GetTemporary(Camera.pixelWidth, Camera.pixelHeight, 0);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        var material = GetMaterial();
        if (Camera && Camera.targetTexture)
        {
            RenderTexture.ReleaseTemporary(Camera.targetTexture);
            Camera.targetTexture = RenderTexture.GetTemporary(source.width, source.height, 0);
        }
        if (!material) return;

        // 设置参数
        material.SetFloat("_BloomBlurSize", bloomBlurSize);
        material.SetColor("_BloomColorTint", bloomColorTint);

        var rt = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        var rt2 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        Graphics.Blit(source, rt, material, 0);
        Graphics.Blit(rt, rt2, material, 0);
        Graphics.Blit(rt2, rt, material, 0);
        Graphics.Blit(rt, destination, material, 0);
        RenderTexture.ReleaseTemporary(rt);
        RenderTexture.ReleaseTemporary(rt2);
    }

    private void OnDisable()
    {
        if (bloomMaterial != null)
            DestroyImmediate(bloomMaterial);
        if (Camera && Camera.targetTexture)
        {
            RenderTexture.ReleaseTemporary(Camera.targetTexture);
            Camera.targetTexture = null;
        }
    }
}
