using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NLog;
using System.Reflection;
using System.Collections;

namespace PdfBookReader.Utils
{
    /// <summary>
    /// Disposable object wrapper. Every disposable object outside 
    /// of a using statement must be wrapped with this. 
    /// 
    /// Purpose:
    /// (a) prevent access to disposed object's methods causing mysterious exceptions
    /// (b) debug memory leaks
    /// (c) make code creating and handling disposable object more obvious 
    /// 
    /// This class is not disposable to make it easy to find calls to both 
    /// DW.Wrap and dwObj.Dispose() in Visual Studio, and also to avoid
    /// having it within a using statement.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DW<T> 
        where T : class, IDisposable
    {
        T _item;
        int _hashCode;

#if DEBUG
        public readonly StackTrace CreationSite;

        // Tracking numbers per-type
        static List<DW<T>> _activeWrappers = new List<DW<T>>();

        public static int Created;
        public static int Disposed;

        /// <summary>
        /// Count of objects in active use (not disposed yet) for type T
        /// </summary>
        public static int Active { get { return Created - Disposed; } }

        /// <summary>
        /// List of all active wrapper objects for type T
        /// </summary>
        public static ICollection<DW<T>> GetActiveWrappers() { return _activeWrappers.AsReadOnly(); }

        static DW()
        {
            DW.AddWrappedType(typeof(T));
        }

#endif
        public DW(T item)
        {
            ArgCheck.NotNull(item, "item");
            _item = item;
            _hashCode = item.GetHashCode();

#if DEBUG
            CreationSite = new StackTrace(1);
            Created++;
            DW.Created++;
            _activeWrappers.Add(this);
#endif
        }

        /// <summary>
        /// Wrapped disposable object. 
        /// Never assign this property to a variable or return from a method/getter. 
        /// Stay safe, keep it wrapped.
        /// </summary>
        public T o 
        {
            get 
            {
                if (_item == null) 
                {
#if DEBUG
                    DW.Log.Error("Accesing disposed object created at: \r\n {0}", CreationSite);
                    throw new ObjectDisposedException("item " + this + " from: " + CreationSite);
#else
                    throw new ObjectDisposedException("item " + this); 
#endif
                }
                return _item;
            } 
        }

        /// <summary>
        /// Returns information about all wrapped objects of this class
        /// </summary>
        /// <returns></returns>
        public static string GetDebugInfo()
        {
#if DEBUG
            return String.Format("{1} DW<{0}>s active, {2} created, {3} disposed", 
                typeof(T).Name, Active, Created, Disposed);
#else 
            return "Not available (only tracked in DEBUG mode)";
#endif 
        }

        /// <summary>
        /// Returns true if underlying object has been disposed.
        /// </summary>
        public bool IsDisposed { get { return _item == null; } }

        /// <summary>
        /// Dispose the wrapped object.
        /// </summary>
        public void DisposeItem()
        {
            if (_item == null) { return; }

            T oldItem = _item;
            _item = null;
            oldItem.Dispose();

#if DEBUG
            Disposed++;
            DW.Disposed++;
            _activeWrappers.Remove(this);
#endif
        }

        public override string ToString()
        {
            if (_item == null) { return "DW<" + typeof(T).Name + ">_Disposed"; }
            return "DW<T>: " + _item;
        }

        #region hashtable support
        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            DW<T> that = (DW<T>)obj;
            if (that == null) { return false; }

            if (that._item == null && this._item == null) 
            { 
                // Subtle bug here, but can't do any better, since we don't know
                // what 'that' used to contain
                return this._hashCode == that._hashCode; 
            }

            if (this._item == null) { return false; }
            return this._item.Equals(that._item);
        }
        #endregion
    }


    /// <summary>
    /// Information about all wrappers, no matter which type they wrap.
    /// DW.Wrap(new SomeObj()) helper for creating DW objects.
    /// </summary>
    public static class DW
    {
        /// <summary>
        /// Wrap an object: 
        /// DW&lt;Whatever&gt; foo = DW.Wrap(new Whatever(...));
        /// 
        /// Object should only be wrapped onece, pass around DW after that.
        /// 
        /// Equivalent to
        /// DW&lt;Whatever&gt; foo = new DW&lt;Whatever&gt;(new Whatever(...));
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static DW<T> Wrap<T>(T item)
            where T : class, IDisposable
        {
            return new DW<T>(item); 
        }

#if DEBUG
        // Must use object since DW is global
        static List<Type> _wrappedTypes = new List<Type>();

        // Tracking numbers
        public static int Created;
        public static int Disposed;

        /// <summary>
        /// Count of objects in active use (not disposed yet) for all wrapped types
        /// </summary>
        public static int Active { get { return Created - Disposed; } }

        public static readonly Logger Log = LogManager.GetLogger("Disposable");

        public static void AddWrappedType(Type t) { _wrappedTypes.Add(t); }


        /// <summary>
        /// List of all active wrappers for any type. 
        /// </summary>
        public static List<object> GetAllActiveWrappers()
        { 
            List<object> allItems = new List<object>();
            foreach (Type t in _wrappedTypes)
            {
                Type dt = typeof(DW<>).MakeGenericType(t);
                IList typeItems = (IList)dt.GetMethod("GetActiveWrappers").Invoke(null, new object[0]);
                    
                foreach(object item in typeItems)
                {
                    allItems.Add(item);
                }
            }
            return allItems; 
        }
#endif

        /// <summary>
        /// Returns information about all wrapped objects of this class
        /// </summary>
        /// <returns></returns>
        public static string GetDebugInfo()
        {
#if DEBUG
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Wrapped Types: " + _wrappedTypes.Count);
            foreach (Type t in _wrappedTypes)
            {
                Type dt = typeof(DW<>).MakeGenericType(t);
                String info = (String)dt.GetMethod("GetDebugInfo").Invoke(null, new object[0]);
                sb.AppendLine(info);
            }
            return sb.ToString();
#else 
            return "Not available (only tracked in DEBUG mode)";
#endif 
        }


    }

}
