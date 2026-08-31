using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public Unrecord? lastFocused;
        public List<Unrecord> childunrecords;//unify usage of this list and getpreviouschild method by common Interface? (IChildable)
        public UnrecordCollection()
        {
            childunrecords = new List<Unrecord>();
        }

        public void addchild(string text = "")
        {
            Unrecord u = new Unrecord(null, text);
            Add(u);
            childunrecords.Add(u);
            return;
        }

        public void tabchild()
        {
            if (lastFocused != null) 
            {
                lastFocused.tab();
            }
        }

        public void untabchild()
        {
            if (lastFocused != null)
            {
                lastFocused.untab();
            }
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
        
        public Unrecord(Unrecord? parent, string text)
        {
            label = new Label();
            label.Text = "•";
            label.VerticalTextAlignment = TextAlignment.Center;
            label.FontSize = 26;

            Add(label);

            entry = new Entry();
            entry.Text = text;
            //entry.Completed += Entry_Completed;
            entry.Focused += Entry_Focused;
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
            foreach (Unrecord u in childunrecords)
            {
                u.unshift();
            }
        }

        public void tab()
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

        public void untab()
        {
            if (parentunrec == null)
            {
                return;
            }
            //updateview();



            Unrecord pu = parentunrec;
            if (pu.parentunrec == null)
            {
                UnrecordCollection uc = Parent as UnrecordCollection;
                int parentpos = uc.childunrecords.IndexOf(pu);
                if (uc.childunrecords.Count == parentpos + 1)
                {
                    uc.Remove(this);
                    uc.Add(this);
                    movechildend();
                }
                else
                {
                    Unrecord nextchild = uc.childunrecords[parentpos + 1];
                    int pos = uc.IndexOf(nextchild);
                    uc.Remove(this);
                    uc.Insert(pos-1, this);
                    movechild(nextchild);
                }

                pu.childunrecords.Remove(this);
                parentunrec = null;
                uc.childunrecords.Insert(parentpos+1, this);
                unshift();


            }
            else
            {
                UnrecordCollection uc = Parent as UnrecordCollection;
                Unrecord ppu = pu.parentunrec;
                int pparentpos = ppu.childunrecords.IndexOf(pu);
                if (ppu.childunrecords.Count == pparentpos + 1)
                {
                    uc.Remove(this);
                    uc.Add(this);
                    movechildend();
                }
                else
                {
                    Unrecord nextchild = ppu.childunrecords[pparentpos + 1];
                    int pos = uc.IndexOf(nextchild);
                    uc.Remove(this);
                    uc.Insert(pos - 1, this);
                    movechild(nextchild);
                }

                
                int parentpos = ppu.childunrecords.IndexOf(pu);
                pu.childunrecords.Remove(this);
                parentunrec = ppu;
                ppu.childunrecords.Insert(parentpos + 1, this);
                unshift();


            }
        }

        private void movechild(Unrecord flagrecord)
        {
            UnrecordCollection uc = Parent as UnrecordCollection;

            foreach (Unrecord child in childunrecords)
            {
                int pos = uc.IndexOf(flagrecord);
                uc.Remove(child);
                uc.Insert(pos-1, child);
                child.movechild(flagrecord);
            }
        }

        private void movechildend()
        {
            UnrecordCollection uc = Parent as UnrecordCollection;
            foreach (Unrecord child in childunrecords)
            {
                uc.Remove(child);
                uc.Add(child);
                child.movechildend();
            }
        }
        private void updateview()
        {
            UnrecordCollection uc = Parent as UnrecordCollection;
            Unrecord pu = parentunrec;
            int viewpos = uc.Children.IndexOf(pu.lastchild());
            uc.Remove(this);
            uc.Insert(viewpos + 1, this);
            int i = viewpos + 1 + 1;
            foreach (var item in childunrecords)
            {
                i = item.move(i);
            }
        }

        private int move(int pos)
        {
            UnrecordCollection uc = Parent as UnrecordCollection;
            uc.Remove(this);
            uc.Insert(pos, this);
            int i = pos + 1;
            foreach (var item in childunrecords)
            {
                i = item.move(i);
            }
            return i;
        }

        private Unrecord lastchild()
        {
            return childunrecords.Count == 0 ? this : childunrecords.Last().lastchild();
        }

        private void Entry_Focused(object? sender, EventArgs e)
        {
            (Parent as UnrecordCollection).lastFocused = this;
        }
    }
}
