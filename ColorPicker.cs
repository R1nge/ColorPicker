using UnityEngine;
using UnityEngine.UIElements;

namespace _Assets.Scripts
{
    public class ColorPicker : MonoBehaviour
    {
        [SerializeField] private UIDocument editorDocument;
        public Color CurrentColor = Color.white;
        public Vector2 PaletteSize = new (300, 300);
        public float PointerSize = 32f;

        private void Awake()
        {
            var colorPicker = editorDocument.rootVisualElement.Q<ColorPickerUIToolkit>("ColorPickerUIToolkit");
            colorPicker.dataSource = this;
            colorPicker.PointerSize = PointerSize;
            colorPicker.CurrentColor = CurrentColor;
            colorPicker.PaletteSize = PaletteSize;
            
            editorDocument.rootVisualElement.schedule.Execute(colorPicker.Init);
        }
    }
}