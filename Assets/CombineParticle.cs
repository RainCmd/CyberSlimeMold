using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CombineParticle : MonoBehaviour
{
    public Shader combineShader;
    public Camera particleCamera;
    private Material combineMaterial;
    private Material GetMaterial()
    {
        if (combineMaterial == null && combineShader)
        {
            combineMaterial = new Material(combineShader);
            combineMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
        return combineMaterial;
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        var material = GetMaterial();
        var tex = particleCamera.activeTexture;
        if (!material && tex) return;

        // 设置参数
        material.SetTexture("_ParticleTex", tex);

        Graphics.Blit(source, destination, material, 0);
    }
    private void OnDisable()
    {
        if (combineMaterial != null)
            DestroyImmediate(combineMaterial);
    }
}
