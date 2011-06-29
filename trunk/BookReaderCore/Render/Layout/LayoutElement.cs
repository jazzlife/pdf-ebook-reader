using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using AForge.Imaging;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using BookReader.Utils;

namespace BookReader.Render.Layout
{
    /// <summary>
    /// Layout info for a page element
    /// </summary>
    [DataContract]
    public class LayoutElement
    {
        RectangleF _unitBounds;

        /// <summary>
        /// Type of the element
        /// </summary>
        [DataMember]
        public LayoutElementType Type { get; set; }

        /// <summary>
        /// Unit bounds (in [0-1] unit interval), based on PageSize
        /// </summary>
        public RectangleF UnitBounds 
        {
            get { return _unitBounds; }
            set
            {
                ArgCheck.IsUnit(value.X, "x");
                ArgCheck.IsUnit(value.Y, "y");
                ArgCheck.IsUnit(value.Width, "width");
                ArgCheck.IsUnit(value.Height, "height");

                _unitBounds = value;
            }
        }

        /// <summary>
        /// Text within the elemnt (null if unknown)
        /// </summary>
        [DataMember]
        public String Text { get; set; } 

        /// <summary>
        /// Internal elements
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<LayoutElement> Children { get; set; }


        public LayoutElement(LayoutElementType type = LayoutElementType.Word, IEnumerable<LayoutElement> children = null) 
        {
            Type = type;
            Children = new List<LayoutElement>();
            if (children != null) 
            { 
                Children.AddRange(children);
                SetBoundsFromNodes(false);
                Text = children.Select(x => x.Text).Where(x => !String.IsNullOrEmpty(x)).ElementsToStringS(delim: " ");
            }
        }

        public static LayoutElement NewWord(Size pageSize, Rectangle bounds, String text = null)
        {
            RectangleF unitBounds = GetUnitBounds(pageSize, bounds);
            var e = new LayoutElement(LayoutElementType.Word, unitBounds, text);
            return e;
        }

        public static LayoutElement NewRow(IEnumerable<LayoutElement> nodes, 
            LayoutElementType type = LayoutElementType.Row)
        {
            RectangleF dummyBounds = new RectangleF(0,0,1,1);
            var e = new LayoutElement(type, dummyBounds, null, nodes);

            // set bounds
            if (!nodes.IsEmpty())
            {
                e.Text = nodes.Select(x => x.Text).Where(x => !String.IsNullOrEmpty(x)).ElementsToStringS(delim: " ");
                e.SetBoundsFromNodes(false);
            }

            return e;
        }

        public LayoutElement(LayoutElementType type, RectangleF unitBounds, String text = null, IEnumerable<LayoutElement> nodes = null)
        {
            ArgCheck.NotEmpty(unitBounds, "unitBounds empty");

            Type = type;
            UnitBounds = unitBounds;
            Text = text;

            if (nodes != null)
            {
                Children = new List<LayoutElement>();
                Children.AddRange(nodes);
            }
        }

        #region Serialization

        // For serialization - cut down on ridiculous amount of XML crap
        [DataMember(Name = "S")]
        string StringRep
        {
            get
            {
                return UnitBounds.X + "," + UnitBounds.Y + "," + UnitBounds.Width + "," + UnitBounds.Height;
            }
            set
            {
                String[] parts = value.Split('x', ' ', ',');
                int i = 0;
                UnitBounds = new RectangleF(float.Parse(parts[i++]), float.Parse(parts[i++]), float.Parse(parts[i++]), float.Parse(parts[i++]));
            }
        }

        #endregion



        /// <summary>
        /// Get bounds based on the new page size
        /// </summary>
        /// <param name="pageSize"></param>
        public Rectangle GetBounds(Size pageSize)
        {
            return new Rectangle(
                (UnitBounds.X * pageSize.Width).Round(),
                (UnitBounds.Y * pageSize.Height).Round(),
                (UnitBounds.Width * pageSize.Width).Round(),
                (UnitBounds.Height * pageSize.Height).Round());
        }

        public bool IsEmpty { get { return UnitBounds.IsEmpty; } }

        /// <summary>
        /// Bounds in unit interval [0, 1] coordinates 
        /// </summary>
        public static RectangleF GetUnitBounds(Size pageSize, Rectangle relBounds)
        {
            // Unit bounds checking [0-1] not done -- would
            // prevent out-of-page-bounds coordinates.
            return new RectangleF(
                (float)relBounds.X / pageSize.Width,
                (float)relBounds.Y / pageSize.Height,
                (float)relBounds.Width / pageSize.Width,
                (float)relBounds.Height / pageSize.Height);
        }

        /// <summary>
        /// Set bounds from child nodes
        /// </summary>
        /// <param name="recursive"></param>
        public void SetBoundsFromNodes(bool recursive)
        {
            // No adjustment
            if (Children == null || Children.Count < 1) { return; } 

            if (recursive)
            {
                Children.ForEach(x => x.SetBoundsFromNodes(true));
            }

            float left = Children.Min(x => x.UnitBounds.Left);
            float width = Children.Max(x => x.UnitBounds.Right) - left;

            float top = Children.Min(x => x.UnitBounds.Top);
            float height = Children.Max(x => x.UnitBounds.Bottom) - top;

            UnitBounds = new RectangleF(left, top, width, height);
        }


        public override bool Equals(object obj)
        {
            LayoutElement that = obj as LayoutElement;
            if (that == null) { return false; }

            if (this.Type != that.Type ||
                this.UnitBounds != that.UnitBounds) { return false; }

            if (this.Children == null && that.Children == null) { return true; }

            return this.Children.Equals(that.Children);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Type + " ");
            if (Text != null) { sb.Append(Text + " "); }
            sb.AppendFormat("{0:#.00},{1:#.00} {2:#.00}x{3:#.00}", UnitBounds.X, UnitBounds.Y, UnitBounds.Width, UnitBounds.Height);

            return sb.ToString();
        }

        #region debug

        [DebugOnly]
        public void Debug_DrawLayout(Graphics g, Size pageSize)
        {
            // Draw children
            if (Children != null)
            {
                foreach (var n in Children)
                {
                    n.Debug_DrawLayout(g, pageSize);
                }
            }

            // Draw self
            g.DrawRectangle(GetPen(Type), GetBounds(pageSize));
        }

        Pen GetPen(LayoutElementType Type)
        {
            switch (Type)
            {
                case LayoutElementType.Word:
                    return Pens.LightBlue;
                case LayoutElementType.Image:
                    return Pens.Violet;
                case LayoutElementType.TitleRow:
                    return Pens.Magenta;
                case LayoutElementType.Row:
                    return Pens.Red;
                case LayoutElementType.HeaderRow:
                    return Pens.DarkGreen;
                case LayoutElementType.FooterRow:
                    return Pens.Blue;
                case LayoutElementType.FootnoteRow:
                    return Pens.Green;
                case LayoutElementType.Page:
                    return Pens.LightBlue;
                case LayoutElementType.Column:
                    return Pens.BurlyWood;
                default:
                    return Pens.Yellow;
            }
        }

        #endregion
    }

    public enum LayoutElementType
    {
        Word = 0,
        Image,
        Row,
        TitleRow,
        HeaderRow,
        FooterRow,
        FootnoteRow,

        Column,
        Page, 
    }

}
