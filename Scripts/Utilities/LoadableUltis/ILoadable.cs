namespace Base.LoadModule
{
    using System;

    public interface ILoadable
    {
        public void Setup();

        public Action     WorkLoad  { get; set; }
        public Func<bool> IsLoaded  { get; set; }
        public Func<bool> IsTimeout { get; set; }

        public float WorkEstimate { get; set; }

        public float GetProgress();
    }
}