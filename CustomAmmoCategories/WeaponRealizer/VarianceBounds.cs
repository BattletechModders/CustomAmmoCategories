namespace WeaponRealizer
{
    internal struct VarianceBounds
    {
        public readonly float Min;
        public readonly float Max;
        public readonly float StandardDeviation;

        public VarianceBounds(float min, float max, float standardDeviation)
        {
            Min = min;
            Max = max;
            StandardDeviation = standardDeviation;
        }
    }
}