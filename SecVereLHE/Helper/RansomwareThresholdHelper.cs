using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecVerseLHE.Helper
{
    internal class RansomwareThresholdHelper
    {
        private readonly int _baseImmediateBlockThreshold;
        private readonly int _baseObservationThreshold;
        private readonly int _baseSustainedBlockThreshold;
        public RansomwareThresholdHelper(int baseImmediateBlockThreshold, int baseObservationThreshold, int baseSustainedBlockThreshold)
        {
            _baseImmediateBlockThreshold = Math.Max(1, baseImmediateBlockThreshold);
            _baseObservationThreshold = Math.Max(1, baseObservationThreshold);
            _baseSustainedBlockThreshold = Math.Max(1, baseSustainedBlockThreshold);
        }

        internal Thresholds GetThresholds(int recentCount, int observationWindowMs)
        {
            var rate = CalculateRate(recentCount, observationWindowMs);
            var multiplier = CalculateMultiplier(rate);

            var immediate = ScaleThreshold(_baseImmediateBlockThreshold, multiplier);
            var observation = ScaleThreshold(_baseObservationThreshold, multiplier);
            var sustained = ScaleThreshold(_baseSustainedBlockThreshold, multiplier);

            return new Thresholds(
                Math.Max(observation, immediate),
                observation,
                Math.Max(observation, sustained));
        }

        private static double CalculateRate(int recentCount, int observationWindowMs)
        {
            if (observationWindowMs <= 0)
                return recentCount;

            var seconds = Math.Max(1.0, observationWindowMs / 1000.0);
            return recentCount / seconds;
        }

        private static double CalculateMultiplier(double rate)
        {
            if (rate >= 20)
                return 0.5;
            if (rate >= 10)
                return 0.7;
            if (rate >= 5)
                return 0.85;
            if (rate <= 1)
                return 1.3;
            if (rate <= 2)
                return 1.15;

            return 1.0;
        }

        private static int ScaleThreshold(int baseValue, double multiplier)
        {
            var scaled = (int)Math.Round(baseValue * multiplier, MidpointRounding.AwayFromZero);
            return Math.Max(1, scaled);
        }

        internal readonly struct Thresholds
        {
            public int ImmediateBlockThreshold { get; }
            public int ObservationThreshold { get; }
            public int SustainedBlockThreshold { get; }

            public Thresholds(int immediateBlockThreshold, int observationThreshold, int sustainedBlockThreshold)
            {
                ImmediateBlockThreshold = immediateBlockThreshold;
                ObservationThreshold = observationThreshold;
                SustainedBlockThreshold = sustainedBlockThreshold;
            }
        }
    }
}