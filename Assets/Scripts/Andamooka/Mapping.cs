using Eidetic.Utility;

namespace Eidetic.Andamooka
{

    [System.Serializable]
    public abstract class Mapping<T>
    {
        public T Input;
        public float MinimumValue = 0;
        public float MaximumValue = 1;
        public float Output
        {
            get
            {
                return GetInputValue().Map(MinimumValue, MaximumValue);
            }
        }
        public abstract float GetInputValue();
        public bool Active
        {
            get
            {
                return MinimumValue != 1 && MaximumValue != 1;
            }
        }
    }

}