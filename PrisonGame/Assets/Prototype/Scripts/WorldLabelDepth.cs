using UnityEngine;

namespace PrisonGame.Prototype
{
    // Default legacy text ignores world depth. Use the live font atlas with a depth-tested shader.
    [RequireComponent(typeof(TextMesh), typeof(MeshRenderer))]
    public sealed class WorldLabelDepth : MonoBehaviour
    {
        [SerializeField] private Shader labelShader;
        private Material material;
        private TextMesh label;
        private void OnEnable()
        {
            label = GetComponent<TextMesh>();
            if (material == null) material = new Material(labelShader);
            GetComponent<MeshRenderer>().sharedMaterial = material;
            RefreshAtlas(label.font);
            Font.textureRebuilt += RefreshAtlas;
        }
        private void RefreshAtlas(Font font)
        { if (font != null && font == label.font) material.mainTexture = font.material.mainTexture; }
        private void OnDisable() { Font.textureRebuilt -= RefreshAtlas; }
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
