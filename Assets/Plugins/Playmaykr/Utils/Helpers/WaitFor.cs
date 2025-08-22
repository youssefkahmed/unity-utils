using System.Collections.Generic;
using UnityEngine;

namespace Playmaykr.Utils.Helpers
{
    public static class WaitFor
    {
        public static WaitForFixedUpdate FixedUpdate { get; } = new();
        public static WaitForEndOfFrame EndOfFrame { get; } = new();

        private static readonly Dictionary<float, WaitForSeconds> WaitForSecondsDict = new(100, new FloatComparer());

        public static WaitForSeconds Seconds(float seconds)
        {
            if (seconds < 1f / Application.targetFrameRate)
            {
                return null;
            }

            if (!WaitForSecondsDict.TryGetValue(seconds, out WaitForSeconds forSeconds))
            {
                forSeconds = new WaitForSeconds(seconds);
                WaitForSecondsDict[seconds] = forSeconds;
            }

            return forSeconds;
        }

        private class FloatComparer : IEqualityComparer<float>
        {
            public bool Equals(float x, float y) => Mathf.Abs(x - y) <= Mathf.Epsilon;
            public int GetHashCode(float obj) => obj.GetHashCode();
        }
    }
}