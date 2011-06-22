using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BookReader.Utils
{
    // Generic event args
    public class EvArgs<T> : EventArgs
    {
        public readonly T Value;

        public EvArgs(T value)
        {
            Value = value;
        }
    }

    public static class EvArgs
    {
        public static EvArgs<T> Create<T>(T arg) { return new EvArgs<T>(arg); }
    }

    // Shorthand for EventHandler<EvArgs<TValue>>
    public delegate void EvHandler<T>(object sender, EvArgs<T> e);

}
