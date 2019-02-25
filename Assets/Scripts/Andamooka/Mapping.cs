using Eidetic.Utility;

namespace Eidetic.Andamooka
{
    [System.Serializable]
    public abstract class Mapping<T>
    {
        public T Input;
        public float MinimumValue = 0;
        public float MaximumValue = 1;
        public abstract float Output {get;}
        public abstract float GetInputValue();
        public bool Active
        {
            get
            {
                return MinimumValue != 1 && MaximumValue != 1;
            }
        }
    }
    public static partial class ExtensionMethods{
        /// <summary>
        /// Map a float from 0-1 to the range within a mapping object.
        /// </summary>
        /// <param name="input">The input float to map</param>
        /// <param name="mapping">Mapping object</param>
        public static float Map<T>(this float input, Mapping<T> mapping)
        {
            return input.Map(mapping.MinimumValue, mapping.MaximumValue);
        }
    }
}