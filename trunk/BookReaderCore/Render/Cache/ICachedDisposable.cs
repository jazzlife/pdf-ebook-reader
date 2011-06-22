using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookReader.Utils;

namespace BookReader.Render.Cache
{
    /// <summary>
    /// Wrapper for cached objects which are expensive to create.
    /// Cache will create and return this, with InUse set to True.
    /// Later, when InUse is set to False, it can dispose it.
    /// </summary>
    public interface ICachedDisposable
    {
        /// <summary>
        /// User should call Return when done with the object,
        /// as it would call Dispose on an IDisposable.
        /// </summary>
        void Return();

        /// <summary>
        /// True if object is in use, false if it was returned.
        /// </summary>
        bool InUse { get; }
    }

}
