using System.Collections.Generic;
using Vintagestory.API.Common;

namespace VintageStoryCodeMod1.Utils
{
    public class MainThreadTimer
    {
        private static List<MainThreadTimer> Timers = new List<MainThreadTimer>();

        public static MainThreadTimer Dispatch(ICoreAPI api, System.Func<float, bool> toRun, int updateRate = 33)
        {
            var timer = new MainThreadTimer()
            {
                api = api,
                toRun = toRun,
                updateRate = updateRate
            };

            Timers.Add(timer);
            
            timer.Start();

            return timer;
        }

        private ICoreAPI api;
        private long runId = 0;
        private int updateRate = 100;
        private System.Func<float, bool> toRun;

        public void Start()
        {
            if (runId != 0)
                return;
            runId = api.Event.RegisterGameTickListener(Tick, updateRate);
        }

        public void Stop()
        {
            if (runId == 0)
                return;
            api.Event.UnregisterGameTickListener(runId);
            runId = 0;
        }
        
        private void Tick(float deltaTime)
        {
            if (toRun.Invoke(deltaTime) == false)
            {
                Stop();
                Timers.Remove(this);
            }
        }
    }
}