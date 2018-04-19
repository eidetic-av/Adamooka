using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SirenDisc : MonoBehaviour
{
    List<GameObject> Discs = new List<GameObject>();
    List<Color> Colors = new List<Color>();

    public Material Material;
    public Material Blockout;

    public SirenDisc WithMaterial(Material material)
    {
        Material = material;
        return this;
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Initialise(GameObject onGameObject, int discCount, int colorSpread, float baseSize, float spacing, Vector3 startPosition, List<Color> colors, Material material, Material baseMaterial = null)
    {
        Clear();
        Colors = colors.ToList();
        var active = onGameObject.activeInHierarchy;
        onGameObject.SetActive(true);
        for (int d = 0; d < discCount; d++)
        {
            var radius = baseSize + (spacing * d);
            var thickness = 0.05f - (0.05f*(d / (float)discCount));
            var position = new Vector3(startPosition.x, startPosition.y, startPosition.z);
            position.z = startPosition.z - (thickness / 2f);
            var hue = Mathf.Repeat(d * (1 / (float)colorSpread), 1f);
            //var startColor = Color.HSVToRGB(hue, .8f, .5f);
            int colorIndex = d % Colors.Count;
            var startColor = Colors[colorIndex];

            if (d == 0 && baseMaterial != null)
                Discs.Add(CreateDisc(onGameObject, position, radius, thickness, startColor, baseMaterial));
            else
                Discs.Add(CreateDisc(onGameObject, position, radius, thickness, startColor, material));
        }
        onGameObject.SetActive(active);
        SetColors(colors.ToList());
    }

    public void Clear()
    {
        if (Discs.Count != 0)
        {
            foreach (var disc in Discs)
            {
                Destroy(disc);
            }
        }
        Discs.Clear();
        Colors.Clear();
    }

    GameObject CreateDisc(GameObject onGameObject, Vector3 startPosition, float radius, float thickness, Color startColor, Material material)
    {
        var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        //disc.hideFlags = HideFlags.HideInHierarchy;
        disc.transform.SetParent(onGameObject.GetComponentInParent<Transform>());
        disc.transform.localScale = new Vector3(radius, thickness, radius);
        disc.transform.localPosition = startPosition;
        disc.transform.localEulerAngles = new Vector3(90, 0, 0);
        var renderer = disc.GetComponent<Renderer>();
        if (material != null)
        {
            renderer.material = material;
            renderer.material.SetColor("_Color", startColor);
        }
        return disc;
    }

    public void SetColors(List<Color> colors)
    {
        if (colors.Count == 0) return;
        Colors = colors;
        for (int i = 0; i < Discs.Count; i++)
        {
            int colorIndex = i % colors.Count;
            var color = colors[colorIndex];
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);
            var renderer = Discs[i].GetComponent<Renderer>();
            renderer.material = new Material(Material);
            var alpha = 1;
            if ((v == 0 && Blockout != null) || i == 0)
            {
                renderer.material = Blockout;
            }
            color.a = alpha;
            renderer.material.SetColor("_Color", color);
        }
    }

    public void ReverseColors()
    {
        var newColors = Colors.ToList();
        var newFirst = newColors.Last();
        newColors.Insert(0, newFirst);
        newColors.RemoveAt(newColors.Count - 1);
        SetColors(newColors);
    }

    public void AdvanceColors()
    {
        var newColors = Colors.ToList();
        var newLast = newColors.First();
        newColors.Remove(newColors.First());
        newColors.Add(newLast);
        SetColors(newColors);
    }
}
