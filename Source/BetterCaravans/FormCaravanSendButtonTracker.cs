using UnityEngine;
using Verse;

namespace BetterCaravans
{
    public static class FormCaravanSendButtonTracker
    {
        private static Rect lastSendRect;
        private static int lastSendFrame = -1;

        public static void RecordSendButtonRect(Rect rect)
        {
            lastSendRect = rect;
            lastSendFrame = Time.frameCount;
        }

        public static bool TryGetSendButtonRect(out Rect rect)
        {
            if (lastSendFrame == Time.frameCount)
            {
                rect = lastSendRect;
                return true;
            }

            rect = default;
            return false;
        }
    }
}
