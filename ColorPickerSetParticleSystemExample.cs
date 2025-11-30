using UnityEngine;
using UnityEngine.UIElements;

namespace _Assets.Scripts
{
    public class ColorPickerSetParticleSystemExample : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private new ParticleSystem particleSystem;
        private ColorPickerUIToolkit _colorPickerElement;

        private void Awake()
        {
            _colorPickerElement = document.rootVisualElement.Q<ColorPickerUIToolkit>();
            _colorPickerElement.OnColorPicked += UpdateColor;
            UpdateColor(_colorPickerElement.CurrentColor);
        }

        private void OnDestroy()
        {
            _colorPickerElement.OnColorPicked -= UpdateColor;
        }

        private void UpdateColor(Color newColor)
        {
            particleSystem.Stop();
            var main = particleSystem.main;
            var color = main.startColor;
            color.color = newColor;
            main.startColor = color;
            particleSystem.Play();            
        }
    }
}