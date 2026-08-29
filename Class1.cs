using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MauiApp2
{
    public class RecordCollection : VerticalStackLayout
    {
        public RecordCollection()
        { 
        }

        public void addchild()
        {
            Add(new RecordEnhancer());
            return;
        }

        public RecordEnhancer? getpreviouschild(RecordEnhancer re)
        {
            int childposition = IndexOf(re);
            //if index == -1 throw an exception
            if (childposition == 0)
            {
                return null;
            }
            else
            {
                return Children[childposition - 1] as RecordEnhancer;
            }
        }
    }

    public class RecordEnhancer : VerticalStackLayout
    {
        private Record record;
        private VerticalStackLayout? childvsl;

        public RecordEnhancer()
        {
            record = new Record();

            Add(record);

            childvsl = null;
        }

        public void addchildre(RecordEnhancer re)
        {
            if (childvsl == null)
            {
                childvsl = new VerticalStackLayout();
                childvsl.Padding = new Thickness(childvsl.Padding.Left + 10, childvsl.Padding.Top, childvsl.Padding.Right, childvsl.Padding.Bottom);
            }

            childvsl.Add(re);
        }
    }

    public class Record : HorizontalStackLayout
    {
        private Label label;
        private Entry entry;
        public Record() 
        { 
            label = new Label();
            label.Text = "•";
            label.VerticalTextAlignment = TextAlignment.Center;
            label.FontSize = 26;

            Add(label);

            entry = new Entry();
            entry.Text = "";
            //entry.Completed += Entry_Completed;
            entry.Completed += Entry_Tabbed;
            Add(entry);
        }

        private void Entry_Completed(object? sender, EventArgs e)
        {
            ((Parent as RecordEnhancer).Parent as RecordCollection).addchild();
        }

        private void Entry_Tabbed(object? sender, EventArgs e)
        {
            RecordEnhancer reparent = Parent as RecordEnhancer;
            RecordCollection rcparent = reparent.Parent as RecordCollection;

            RecordEnhancer? newre = rcparent.getpreviouschild(reparent);

            if (newre != null) 
            {
                newre.addchildre(reparent);
            }
        }
    }

    public class UnrecordCollection : VerticalStackLayout
    {
        public List<Unrecord> childunrecords;//unify usage of this list and getpreviouschild method by common Interface? (IChildable)
        public UnrecordCollection()
        {
            childunrecords = new List<Unrecord>();
        }

        public void addchild()
        {
            Unrecord u = new Unrecord(null);
            Add(u);
            childunrecords.Add(u);
            return;
        }

        public Unrecord? getpreviouschild(Unrecord re)
        {
            int childposition = childunrecords.IndexOf(re);
            //if index == -1 throw an exception
            if (childposition == 0)
            {
                return null;
            }
            else
            {
                return childunrecords[childposition - 1];
            }
        }
    }

    public class Unrecord : HorizontalStackLayout
    {
        private Label label;
        private Entry entry;
        private List<Unrecord> childunrecords;
        private Unrecord? parentunrec;
        
        public Unrecord(Unrecord? parent)
        {
            label = new Label();
            label.Text = "•";
            label.VerticalTextAlignment = TextAlignment.Center;
            label.FontSize = 26;

            Add(label);

            entry = new Entry();
            entry.Text = "";
            //entry.Completed += Entry_Completed;
            entry.Completed += Entry_Tabbed;
            Add(entry);

            childunrecords = new List<Unrecord>();
            parentunrec = parent;
        }

        private void shift()
        {
            Padding = new Thickness(Padding.Left + 10, Padding.Top, Padding.Right, Padding.Bottom);
            foreach (Unrecord u in childunrecords)
            {
                u.shift();
            }
        }

        private void unshift()
        {
            Padding = new Thickness(Padding.Left - 10, Padding.Top, Padding.Right, Padding.Bottom);
        }

        private void Entry_Tabbed(object? sender, EventArgs e)
        {
            if (parentunrec == null)
            {
                UnrecordCollection unrec = Parent as UnrecordCollection;
                Unrecord? u = unrec.getpreviouschild(this);
                if (u != null)
                {
                    unrec.childunrecords.Remove(this);
                    u.childunrecords.Add(this);
                    parentunrec = u;
                    //batchcommit?
                    shift();
                }
            }
            else
            {
                //get previous child
                int childposition = parentunrec.childunrecords.IndexOf(this);
                //if index == -1 throw an exception
                if (childposition != 0)
                {
                    Unrecord u = parentunrec.childunrecords[childposition - 1] as Unrecord;
                    parentunrec.childunrecords.Remove(this);
                    u.childunrecords.Add(this);
                    parentunrec = u;
                    shift();
                }
            }
        }
    }
}
