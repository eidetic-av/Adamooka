//
// KinoMirror - Mirroring and kaleidoscope effect
//
// Copyright (C) 2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

// Modifications made by Eidetic 2018
//

using UnityEngine;

namespace Kino
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Kino Image Effects/Mirror")]
    public class Mirror : MonoBehaviour
    {
        #region Public Properties
        
        int _repeat;
        public int Repeat;
        
        float _offset;
        public float Offset;
        
        float _roll;
        public float Roll;

        public bool AnimateRoll = false;
        public float AnimateRollRate = 0.5f;
        
        bool _symmetry;
        public bool Symmetry;

        #endregion

        #region Private Properties

        [SerializeField] Shader _shader;
        Material _material;

        #endregion

        #region MonoBehaviour Functions

        void Update()
        {
            if (AnimateRoll)
            {
                _roll += (Time.deltaTime * AnimateRollRate);
            } else
            {
                _roll = Roll;
            }
            _repeat = Repeat;
            _offset = Offset;
            _symmetry = Symmetry;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_material == null)
            {
                _material = new Material(_shader);
                _material.hideFlags = HideFlags.DontSave;
            }

            var div = Mathf.PI * 2 / Mathf.Max(1, _repeat);

            _material.SetFloat("_Divisor", div);
            _material.SetFloat("_Offset", _offset * Mathf.Deg2Rad);
            _material.SetFloat("_Roll", _roll * Mathf.Deg2Rad);

            if (_symmetry)
                _material.EnableKeyword("SYMMETRY_ON");
            else
                _material.DisableKeyword("SYMMETRY_ON");

            Graphics.Blit(source, destination, _material);
        }

        #endregion
    }
}
