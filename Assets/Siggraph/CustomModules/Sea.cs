using System.Linq;
using Eidetic.URack.Base;
using Eidetic.URack.CustomTypes;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace Eidetic.URack.VFX
{
    public class Sea : VFXModule
    {
        public override void Start()
        {
            TemplateAsset = Resources.Load<VisualEffectAsset>("SeaGraph");
            base.Start();
        }
        [Input(0, 10, 4, 1), Knob]
        public float SpawnRate
        {
            set => VisualEffect.SetFloat("SpawnRateMultiplier", value);
        }

        [Input(0.01f, 100, 15, 0.5f), Knob]
        public float Lifetime
        {
            set => VisualEffect.SetFloat("Lifetime", value);
        }

        [Input(0, 5, 5, .5f), Knob]
        public float Size
        {
            set => VisualEffect.SetFloat("Size", value);
        }

        [Input(0, 2, 5, 1f), Knob]
        public float SimulationSpeed
        {
            set => VisualEffect.playRate = value;
        }

        [Input(0, .5f, 4, 0), Knob]
        public float Noise
        {
            set => VisualEffect.SetFloat("Noise", value);
        }

        [Input(0, 100, 5, 0), Knob]
        public float Turbulence
        {
            set => VisualEffect.SetFloat("Turbulence", value);
        }

        [Input(0, 20, 5, 1), Knob]
        public float TurbulenceScale
        {
            set
            {
                VisualEffect.SetFloat("TurbulenceScaleX", value);
                VisualEffect.SetFloat("TurbulenceScaleY", value);
                VisualEffect.SetFloat("TurbulenceScaleZ", value);
            }
        }

        [Input(-3, 3, 1, 0), Knob]
        public float EmitterX
        {
            set => VisualEffect.SetFloat("OriginX", value);
        }

        [Input(-3, 3, 1, 0), Knob]
        public float EmitterY
        {
            set => VisualEffect.SetFloat("OriginY", value);
        }

        [Input(-3, 3, 1, 0), Knob]
        public float EmitterZ
        {
            set => VisualEffect.SetFloat("OriginZ", value);
        }

        [Input(0.2f, 15, 15, 0.2f), Knob]
        public float EmitterScale
        {
            set => VisualEffect.SetFloat("EmitterScale", value);
        }

        [Input(-3, 3, 1, 0), Knob]
        public float AttractorX
        {
            set => VisualEffect.SetFloat("AttractorX", value);
        }

        [Input(-3, 3, 1, 0), Knob]
        public float AttractorY
        {
            set => VisualEffect.SetFloat("AttractorY", value);
        }

        [Input(-3, 3, 1, 0), Knob]
        public float AttractorZ
        {
            set => VisualEffect.SetFloat("AttractorZ", value);
        }

        [Input(0f, 5, 2, 1f), Knob]
        public float AttractorScale
        {
            set => VisualEffect.SetFloat("AttractorScale", value);
        }

        [Input(0.1f, 5, 2, 1f), Knob]
        public float AttractorSpeed
        {
            set => VisualEffect.SetFloat("AttractorSpeed", value);
        }

        [Input(0.1f, 50, 2, 1f), Knob]
        public float AttractorForce
        {
            set => VisualEffect.SetFloat("AttractorForce", value);
        }

        [Input(-10f, 10, 5, 0f), Knob]
        public float Gravity
        {
            set => VisualEffect.SetFloat("Gravity", value);
        }
    }
}