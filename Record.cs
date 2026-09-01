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
        public Record? lastFocused;
        public List<Record> subrecords;//unify usage of this list and getpreviouschild method by common Interface? (IChildable)
        public RecordCollection()
        {
            subrecords = new List<Record>();
        }

        public void addsubrecord(string text = "")
        {
            Record u = new Record(null, text);
            Add(u);
            subrecords.Add(u);
            return;
        }

        public void tabrecord()
        {
            if (lastFocused != null) 
            {
                lastFocused.tab();
            }
        }

        public void untabsubrecord()
        {
            if (lastFocused != null)
            {
                lastFocused.untab();
            }
        }

        public Record? getprevioussubrecord(Record re)
        {
            int childposition = subrecords.IndexOf(re);
            //if index == -1 throw an exception
            if (childposition == 0)
            {
                return null;
            }
            else
            {
                return subrecords[childposition - 1];
            }
        }
    }

    public class Record : HorizontalStackLayout
    {
        private Label label;
        private Entry entry;
        private List<Record> subrecords;
        private Record? superrecord;
        
        public Record(Record? parent, string text)
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

            subrecords = new List<Record>();
            superrecord = parent;
        }

        private void shift()
        {
            Padding = new Thickness(Padding.Left + 10, Padding.Top, Padding.Right, Padding.Bottom);
            foreach (Record u in subrecords)
            {
                u.shift();
            }
        }

        private void unshift()
        {
            Padding = new Thickness(Padding.Left - 10, Padding.Top, Padding.Right, Padding.Bottom);
            foreach (Record u in subrecords)
            {
                u.unshift();
            }
        }

        public void tab()
        {
            if (superrecord == null)
            {
                RecordCollection unrec = Parent as RecordCollection;
                Record? u = unrec.getprevioussubrecord(this);
                if (u != null)
                {
                    unrec.subrecords.Remove(this);
                    u.subrecords.Add(this);
                    superrecord = u;
                    //batchcommit?
                    shift();
                }
            }
            else
            {
                //get previous child
                int childposition = superrecord.subrecords.IndexOf(this);
                //if index == -1 throw an exception
                if (childposition != 0)
                {
                    Record u = superrecord.subrecords[childposition - 1] as Record;
                    superrecord.subrecords.Remove(this);
                    u.subrecords.Add(this);
                    superrecord = u;
                    shift();
                }
            }
        }

        public void untab()
        {
            if (superrecord == null)
            {
                return;
            }
            //updateview();



            Record pu = superrecord;
            if (pu.superrecord == null)
            {
                RecordCollection uc = Parent as RecordCollection;
                int parentpos = uc.subrecords.IndexOf(pu);
                if (uc.subrecords.Count == parentpos + 1)
                {
                    uc.Remove(this);
                    uc.Add(this);
                    movechildend();
                }
                else
                {
                    Record nextchild = uc.subrecords[parentpos + 1];
                    int pos = uc.IndexOf(nextchild);
                    uc.Remove(this);
                    uc.Insert(pos-1, this);
                    movechild(nextchild);
                }

                pu.subrecords.Remove(this);
                superrecord = null;
                uc.subrecords.Insert(parentpos+1, this);
                unshift();


            }
            else
            {
                RecordCollection uc = Parent as RecordCollection;
                Record ppu = pu.superrecord;
                int pparentpos = ppu.subrecords.IndexOf(pu);
                if (ppu.subrecords.Count == pparentpos + 1)
                {
                    uc.Remove(this);
                    uc.Add(this);
                    movechildend();
                }
                else
                {
                    Record nextchild = ppu.subrecords[pparentpos + 1];
                    int pos = uc.IndexOf(nextchild);
                    uc.Remove(this);
                    uc.Insert(pos - 1, this);
                    movechild(nextchild);
                }

                
                int parentpos = ppu.subrecords.IndexOf(pu);
                pu.subrecords.Remove(this);
                superrecord = ppu;
                ppu.subrecords.Insert(parentpos + 1, this);
                unshift();


            }
        }

        private void movechild(Record flagrecord)
        {
            RecordCollection uc = Parent as RecordCollection;

            foreach (Record child in subrecords)
            {
                int pos = uc.IndexOf(flagrecord);
                uc.Remove(child);
                uc.Insert(pos-1, child);
                child.movechild(flagrecord);
            }
        }

        private void movechildend()
        {
            RecordCollection uc = Parent as RecordCollection;
            foreach (Record child in subrecords)
            {
                uc.Remove(child);
                uc.Add(child);
                child.movechildend();
            }
        }
        private void updateview()
        {
            RecordCollection uc = Parent as RecordCollection;
            Record pu = superrecord;
            int viewpos = uc.Children.IndexOf(pu.lastchild());
            uc.Remove(this);
            uc.Insert(viewpos + 1, this);
            int i = viewpos + 1 + 1;
            foreach (var item in subrecords)
            {
                i = item.move(i);
            }
        }

        private int move(int pos)
        {
            RecordCollection uc = Parent as RecordCollection;
            uc.Remove(this);
            uc.Insert(pos, this);
            int i = pos + 1;
            foreach (var item in subrecords)
            {
                i = item.move(i);
            }
            return i;
        }

        private Record lastchild()
        {
            return subrecords.Count == 0 ? this : subrecords.Last().lastchild();
        }

        private void Entry_Focused(object? sender, EventArgs e)
        {
            (Parent as RecordCollection).lastFocused = this;
        }
    }
}
