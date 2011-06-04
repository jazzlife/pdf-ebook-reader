using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PdfBookReader.Utils
{
    // Generic event args
    public class EArgs<T> : EventArgs
    {
        public readonly T Value;

        public EArgs(T value)
        {
            Value = value;
        }
    }

    // Shorthand for EventHandler<EArgs<TValue>>
    public delegate void EAHandler<TValue>(object sender, EArgs<TValue> e);

}
