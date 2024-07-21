using System;
using Elements.Core;

namespace Obsidian
{
    public static class RandomXExtensions
    {
        private static RandomXGenerator r = new RandomXGenerator();

        public static floatQ Range(floatQ min, floatQ max)
        {
            lock (r)
            {
                return new floatQ(
                    r.Range(min.x, max.x),
                    r.Range(min.y, max.y),
                    r.Range(min.z, max.z),
                    r.Range(min.w, max.w)
                );
            }
        }
    }
}
