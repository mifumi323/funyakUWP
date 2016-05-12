using System;

namespace MifuminSoft.funyak.View.Utility
{
    public class FpsCounter
    {
        long frame = 0;
        long count = 0;
        long prevTime = 0;
        double fps = 0.0;

        /// <summary>経過フレーム数</summary>
        public long Frame { get { return frame; } }
        /// <summary>FPS</summary>
        public double Fps { get { return fps; } }

        public void Step()
        {
            frame++;
            // FPS計測
            if (count == 0)
            {
                count = 60;
                long time = DateTime.Now.Ticks;
                fps = 10000000.0 * count / (time - prevTime);
                prevTime = time;
            }
            count--;
        }
    }
}
