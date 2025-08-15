using System;

namespace Playmaykr.Utils.Timers
{
    public abstract class Timer
    {
        public Action OnTimerStart = delegate {};
        public Action OnTimerStop = delegate {};
        
        public bool IsRunning { get; protected set; }
        public float Progress => Time / initialTime;
        
        protected float initialTime;
        protected float Time { get; set; }

        protected Timer(float value)
        {
            initialTime = value;
            IsRunning = false;
        }

        public void Start()
        {
            Time = initialTime;
            if (!IsRunning)
            {
                IsRunning = true;
                OnTimerStart?.Invoke();
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                OnTimerStop?.Invoke();
            }
        }

        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;

        public abstract void Tick(float delta);
    }

    public class CooldownTimer : Timer
    {
        public CooldownTimer(float value) : base(value) { }

        public override void Tick(float delta)
        {
            if (IsRunning && Time > 0f)
            {
                Time -= delta;
            }

            if (IsRunning && Time <= 0f)
            {
                Stop();
            }
        }

        public bool IsFinished => Time <= 0;
        public void Reset() => Time = initialTime;
        
        public void Reset(float newTime)
        {
            Time = newTime;
            Reset();
        }
    }
    
    public class StopwatchTimer : Timer
    {
        public StopwatchTimer() : base(0) { }

        public override void Tick(float delta)
        {
            if (IsRunning)
            {
                Time += delta;
            }
        }

        public void Reset() => Time = 0;
        public float GetTime() => Time;
    }
}
