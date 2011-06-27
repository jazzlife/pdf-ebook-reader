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
        // For serialization - cut down on ridiculous amount of XML crap
        [DataMember(Name = "S")]
        string StringRep
        {
            get
            {
                return Bounds.X + "," + Bounds.Y + "," + Bounds.Width + "," + Bounds.Height;
            }
            set
            {
                String[] parts = value.Split('x', ' ', ',');
                int i=0;
                Bounds = new Rectangle(int.Parse(parts[i++]), int.Parse(parts[i++]), int.Parse(parts[i++]), int.Parse(parts[i++]));
            }
        }

        /// <summary>
        /// Text within the elemnt (null if unknown)
        /// </summary>
        [DataMember]
        public String Text { get; set; } 


        /// <summary>
        /// Type of the element
        /// </summary>
        [DataMember]
        public LayoutElementType Type { get; set; }

        /// <summary>
        /// Bounds based on PageSize
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// Internal nodes
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<LayoutElement> Nodes { get; set; }

        public LayoutElement() 
        {
            Nodes = new List<LayoutElement>();
        }

        public LayoutElement(Rectangle bounds, String text = null, LayoutElementType type = LayoutElementType.Word) 
        {
            Bounds = bounds;
            Text = text;
            Type = type;
            Nodes = new List<LayoutElement>();
        }

        /// <summary>
        /// Get bounds based on the new page size
        /// </summary>
        /// <param name="pageSize"></param>
        public Rectangle GetScaledBounds(Size pageSize)
        {
            RectangleF relBounds = GetUnitBounds(pageSize);

            return new Rectangle(
                (relBounds.X * pageSize.Width).Round(),
                (relBounds.Y * pageSize.Height).Round(),
                (relBounds.Width * pageSize.Width).Round(),
                (relBounds.Height * pageSize.Height).Round());
        }

        public bool IsEmpty { get { return Bounds.IsEmpty; } }

        /// <summary>
        /// Bounds in unit interval [0, 1] coordinates 
        /// </summary>
        public RectangleF GetUnitBounds(Size pageSize)
        {
            // Unit bounds checking [0-1] not done -- would
            // prevent out-of-page-bounds coordinates.
            return new RectangleF(
                (float)Bounds.X / pageSize.Width,
                (float)Bounds.Y / pageSize.Height,
                (float)Bounds.Width / pageSize.Width,
                (float)Bounds.Height / pageSize.Height);
        }

        /// <summary>
        /// Set bounds from internal nodes
        /// </summary>
        /// <param name="recursive"></param>
        public void SetBoundsFromNodes(bool recursive)
        {
            // No adjustment
            if (Nodes == null || Nodes.Count < 1) { return; } 

            if (recursive)
            {
                Nodes.ForEach(x => x.SetBoundsFromNodes(true));
            }

            int left = Nodes.Min(x => x.Bounds.Left);
            int width = Nodes.Max(x => x.Bounds.Right) - left;

            int top = Nodes.Min(x => x.Bounds.Top);
            int height = Nodes.Max(x => x.Bounds.Bottom) - top;

            Bounds = new Rectangle(left, top, width, height);
        }


        public override bool Equals(object obj)
        {
            LayoutElement that = obj as LayoutElement;
            if (that == null) { return false; }

            if (this.Type != that.Type ||
                this.Bounds != that.Bounds) { return false; }

            if (this.Nodes == null && that.Nodes == null) { return true; }

            return this.Nodes.Equals(that.Nodes);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Type + " ");
            if (Text != null) { sb.Append(Text + " "); }
            sb.Append(Bounds.X + "," + Bounds.Y + " " + Bounds.Width + "x" + Bounds.Height);

            return base.ToString();
        }

        #region debug

        [DebugOnly]
        public void Debug_DrawLayout(Graphics g)
        {
            // Draw children
            if (Nodes != null)
            {
                g.DrawRectangle(GetPen(Type), Bounds);
            }
        }

        Pen GetPen(LayoutElementType Type)
        {
            switch (Type)
            {
                case LayoutElementType.Word:
                    return Pens.LightCoral;
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
