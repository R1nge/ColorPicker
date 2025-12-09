using System;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ColorPickerUIToolkit : VisualElement, IDisposable
{
    //TODO: two way binding 
    //TODO: hue slider
    //TODO: update hue slider gradient
    //TODO: update color sliders when hue has changed
    private VisualElement _gradientArea;
    private Texture2D _gradientTexture;
    private Slider _redColorSlider, _greenColorSlider, _blueColorSlider, _alphaColorSlider;
    private VisualElement _pointer;

    private bool _draggingPointer;

    //Red color
    private float _hue, _saturation = 1, _value = 1;
    private Color[] _palettePixelsColors;

    public Color CurrentColor = Color.white;
    public Vector2 PaletteSize = new(300, 300);
    public float PointerSize = 32;
    public Action<Color> OnColorPicked;


    public void Init()
    {
        hierarchy.Clear();
        var content = new VisualElement { name = "Content" };

        hierarchy.Add(content);

        var title = CreateTitle();
        content.Add(title);

        _gradientArea = CreateGradientArea(content);
        var gradient = CreateGradient(_gradientArea);

        content.Add(_gradientArea);
        _gradientArea.RegisterCallback<PointerMoveEvent>(PointerMoveEvent);

        _pointer = CreatePointer();
        gradient.Add(_pointer);
        RegisterPointerCallbacks(_pointer);

        _palettePixelsColors = new Color[_gradientTexture.width * _gradientTexture.height];


        _gradientArea.RegisterCallback<PointerDownEvent>(PointerDown);
        _gradientArea.RegisterCallback<MouseLeaveEvent>(evt => PointerUp(null));
        RegisterCallback<PointerUpEvent>(PointerUp);

        var sliders = new VisualElement { name = "Sliders" };

        CreateColorSliders(ref sliders);
        content.Add(sliders);

        RegisterColorSlidersCallbacks();
        UpdateGradientTexture();
        OnColorChanged(true, true);
    }

    public ColorPickerUIToolkit()
    {
        Init();
    }

    private VisualElement CreateGradient(VisualElement parent)
    {
        var gradient = new VisualElement
        {
            name = "Gradient",
            style =
            {
                width = new StyleLength(PaletteSize.x),
                height = new StyleLength(PaletteSize.y)
            }
        };

        const Align gradientAlignSelf = Align.Center;
        gradient.style.alignSelf = gradientAlignSelf;

        _gradientTexture = new Texture2D(64, 64, TextureFormat.RGB24, false)
        {
            filterMode = FilterMode.Point,
            hideFlags = HideFlags.HideAndDontSave
        };
        gradient.style.backgroundImage = _gradientTexture;

        parent.Add(gradient);

        return gradient;
    }

    private VisualElement CreateGradientArea(VisualElement content)
    {
        var gradientArea = new VisualElement
        {
            name = "GradientArea",
            style =
            {
                width = new StyleLength(PaletteSize.x),
                height = new StyleLength(PaletteSize.y)
            }
        };

        const Align gradientAlignSelf = Align.Center;
        gradientArea.style.alignSelf = gradientAlignSelf;
        content.Add(gradientArea);

        return gradientArea;
    }

    private VisualElement CreateTitle()
    {
        var title = new VisualElement
        {
            name = "Title"
        };

        var titleLabel = new Label
        {
            name = "TitleLabel",
            text = "Color picker"
        };

        const Align titleAlignSelf = Align.Center;
        titleLabel.style.alignSelf = titleAlignSelf;

        title.Add(titleLabel);

        return title;
    }

    public void Dispose()
    {
        UnRegisterColorSlidersCallbacks();
    }

    private void RegisterPointerCallbacks(VisualElement pointer)
    {
        pointer.RegisterCallback<PointerDownEvent>(PointerDown);
        pointer.RegisterCallback<PointerUpEvent>(PointerUp);
    }

    private VisualElement CreatePointer()
    {
        var pointer = new VisualElement
        {
            name = "Pointer",
            style =
            {
                width = new StyleLength(PointerSize),
                height = new StyleLength(PointerSize),
                color = new StyleColor(new Color(255, 255, 255, 255)),
                backgroundColor = new StyleColor(new Color(255, 255, 255, 255))
            }
        };

        return pointer;
    }

    private void CreateColorSliders(ref VisualElement sliders)
    {
        _redColorSlider = new Slider("Red", 0f, 1f)
        {
            name = "RedSlider",
            showInputField = true,
            value = 1
        };
        sliders.Add(_redColorSlider);

        _greenColorSlider = new Slider("Green", 0f, 1f)
        {
            name = "GreenSlider",
            showInputField = true
        };
        sliders.Add(_greenColorSlider);

        _blueColorSlider = new Slider("Blue", 0f, 1f)
        {
            name = "BlueSlider",
            showInputField = true
        };
        sliders.Add(_blueColorSlider);

        _alphaColorSlider = new Slider("Alpha", 0f, 1f)
        {
            name = "AlphaSlider",
            showInputField = true,
            value = 1
        };
        sliders.Add(_alphaColorSlider);
    }

    private void RegisterColorSlidersCallbacks()
    {
        _redColorSlider.RegisterValueChangedCallback(OnRedColorChanged);
        _greenColorSlider.RegisterValueChangedCallback(OnGreenColorChanged);
        _blueColorSlider.RegisterValueChangedCallback(OnBlueColorChanged);
        _alphaColorSlider.RegisterValueChangedCallback(OnAlphaColorChanged);
    }

    private void UnRegisterColorSlidersCallbacks()
    {
        _redColorSlider.UnregisterValueChangedCallback(OnRedColorChanged);
        _greenColorSlider.UnregisterValueChangedCallback(OnGreenColorChanged);
        _blueColorSlider.UnregisterValueChangedCallback(OnBlueColorChanged);
        _alphaColorSlider.UnregisterValueChangedCallback(OnAlphaColorChanged);
    }

    private void PointerDown(PointerDownEvent evt)
    {
        _draggingPointer = true;
        var translate = _pointer.style.translate.value;
        translate.x = evt.localPosition.x - PointerSize / 2f;
        translate.y = evt.localPosition.y - PointerSize / 2f;
        _pointer.style.translate = translate;
        UpdateColorFromPosition(GetPointerPosition());
    }

    private void PointerMoveEvent(PointerMoveEvent pointer)
    {
        Debug.LogWarning($"Dragging: {_draggingPointer}");
        if (_draggingPointer)
        {
            var halfWidth = PointerSize / 2f;
            var halfHeight = PointerSize / 2f;
            var translate = _pointer.style.translate.value;
            var positionX = Math.Clamp(translate.x.value + pointer.deltaPosition.x, -halfWidth,
                PaletteSize.x - halfWidth);
            var positionY = Math.Clamp(translate.y.value + pointer.deltaPosition.y, -halfHeight,
                PaletteSize.y - halfHeight);
            translate.x = positionX;
            translate.y = positionY;
            _pointer.style.translate = translate;
            UpdateColorFromPosition(GetPointerPosition());
        }
    }

    private void PointerUp(PointerUpEvent evt)
    {
        _draggingPointer = false;
    }

    private void OnAlphaColorChanged(ChangeEvent<float> alpha)
    {
        SetAlphaFromSlider(alpha.newValue);
    }

    private void OnBlueColorChanged(ChangeEvent<float> blueColor)
    {
        SetColorFromBlueSlider(blueColor.newValue);
    }

    private void OnGreenColorChanged(ChangeEvent<float> greenColor)
    {
        SetColorFromGreenSlider(greenColor.newValue);
    }

    private void OnRedColorChanged(ChangeEvent<float> redColor)
    {
        SetColorFromRedSlider(redColor.newValue);
    }

    private void UpdateGradientTexture()
    {
        if (_gradientTexture == null)
        {
            Debug.LogError("[ColorPicker][UpdateGradientTexture] Texture is null");
            return;
        }

        for (var y = 0; y < _gradientTexture.height; y++)
        {
            for (var x = 0; x < _gradientTexture.width; x++)
            {
                var index = y * _gradientTexture.width + x;
                _palettePixelsColors[index] = Color.HSVToRGB(_hue, (float)x / _gradientTexture.height,
                    (float)y / _gradientTexture.width);
            }
        }

        _gradientTexture.SetPixels(_palettePixelsColors);
        _gradientTexture.Apply();

        _gradientArea.MarkDirtyRepaint();
    }

    private void SetColorFromRedSlider(float value)
    {
        var color = Color.HSVToRGB(_hue, _saturation, _value);
        color.r = value;
        Color.RGBToHSV(color, out _hue, out _saturation, out _value);
        OnColorChanged(false, true);
    }

    private void SetColorFromGreenSlider(float value)
    {
        var color = Color.HSVToRGB(_hue, _saturation, _value);
        color.g = value;
        Color.RGBToHSV(color, out _hue, out _saturation, out _value);
        OnColorChanged(false, true);
    }

    private void SetColorFromBlueSlider(float value)
    {
        var color = Color.HSVToRGB(_hue, _saturation, _value);
        color.b = value;
        Color.RGBToHSV(color, out _hue, out _saturation, out _value);
        OnColorChanged(false, true);
    }

    private void SetAlphaFromSlider(float value)
    {
        var color = Color.HSVToRGB(_hue, _saturation, _value);
        color.a = value;
        Color.RGBToHSV(color, out _hue, out _saturation, out _value);
        OnColorChanged(false, true);
    }

    private void UpdateColorFromPosition(Vector2 position)
    {
        var halfWidth = (resolvedStyle.width - _pointer.resolvedStyle.width) / 2;
        var halfHeight = (resolvedStyle.height - _pointer.resolvedStyle.height) / 2;

        if (float.IsNaN(halfWidth) || float.IsNaN(halfHeight))
        {
            Debug.LogWarning("[ColorPicker][UpdateColorFromPosition] UI hasn't been initialized yet!");
            return;
        }

        if (Mathf.Abs(halfWidth) < Mathf.Epsilon || Mathf.Abs(halfHeight) < Mathf.Epsilon)
        {
            Debug.LogWarning("[ColorPicker][UpdateColorFromPosition] At the edge of the color palette");
            return;
        }

        var normalizedPositionX = Mathf.Max(0f, Mathf.Min(position.x, halfWidth)) / halfWidth;
        var inversedNormalizedPositionY = 1 - Mathf.Max(0f, Mathf.Min(position.y, halfHeight)) / halfHeight;

        var pixels = _gradientTexture.GetPixels32(0);
        var textureWidth = _gradientTexture.width;
        var textureHeight = _gradientTexture.height;
        var normalizedPositionToPixelPositionX = (int)(normalizedPositionX * (textureWidth - 1));
        var normalizedPositionToPixelPositionY = (int)(inversedNormalizedPositionY * (textureHeight - 1));
        var pixelIndex = normalizedPositionToPixelPositionY * textureWidth + normalizedPositionToPixelPositionX;
        CurrentColor = pixels[pixelIndex];
        OnColorPicked?.Invoke(CurrentColor);
    }

    private Vector2 GetPointerPosition()
    {
        if (_pointer == null)
        {
            return Vector2.zero;
        }

        return new Vector2(_pointer.style.translate.value.x.value, _pointer.style.translate.value.y.value);
    }

    private void OnColorChanged(bool updateHue, bool updateGradient)
    {
        var color = Color.HSVToRGB(_hue, _saturation, _value);
        //hueSliderDragger.style.backgroundColor = Color.HSVToRGB(H, 1f, 1f);
        //gradientSliderDragger.style.backgroundColor = c;

        _redColorSlider.SetValueWithoutNotify(Round(color.r, 3));
        _greenColorSlider.SetValueWithoutNotify(Round(color.g, 3));
        _blueColorSlider.SetValueWithoutNotify(Round(color.b, 3));

        if (updateHue)
        {
            //TODO: update hue slider
            //hueSlider.SetValueWithoutNotify(H * 360f);
        }

        if (updateGradient)
        {
            UpdateGradientTexture();
            UpdateColorFromPosition(GetPointerPosition());
            //gradientSlider.SetValueWithoutNotify(new Vector2(S, V));
        }
    }

    private float Round(float value, int digits)
    {
        var mult = Mathf.Pow(10.0f, digits);
        return Mathf.Round(value * mult) / mult;
    }
}