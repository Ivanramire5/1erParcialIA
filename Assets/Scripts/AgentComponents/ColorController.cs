using UnityEngine;

public class ColorController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Tooltip("El renderer a modificar. Si está vacío, buscará uno en este GameObject.")]
    private Renderer targetRenderer;

    private MaterialPropertyBlock _propBlock;

   
    private static readonly int ColorPropID = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }
    }

    public void SetColor(Color newColor)
    {
        if (targetRenderer == null) return;

        targetRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(ColorPropID, newColor);
        targetRenderer.SetPropertyBlock(_propBlock);
    }
}