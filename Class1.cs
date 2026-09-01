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
    //public class RecordCollection : VerticalStackLayout
    //{
    //    public RecordCollection()
    //    { 
    //    }

    //    public void addchild()
    //    {
    //        Add(new RecordEnhancer());
    //        return;
    //    }

    //    public RecordEnhancer? getpreviouschild(RecordEnhancer re)
    //    {
    //        int childposition = IndexOf(re);
    //        //if index == -1 throw an exception
    //        if (childposition == 0)
    //        {
    //            return null;
    //        }
    //        else
    //        {
    //            return Children[childposition - 1] as RecordEnhancer;
    //        }
    //    }
    //}

    //public class RecordEnhancer : VerticalStackLayout
    //{
    //    private Record record;
    //    private VerticalStackLayout? childvsl;

    //    public RecordEnhancer()
    //    {
    //        record = new Record();

    //        Add(record);

    //        childvsl = null;
    //    }

    //    public void addchildre(RecordEnhancer re)
    //    {
    //        if (childvsl == null)
    //        {
    //            childvsl = new VerticalStackLayout();
    //            childvsl.Padding = new Thickness(childvsl.Padding.Left + 10, childvsl.Padding.Top, childvsl.Padding.Right, childvsl.Padding.Bottom);
    //        }

    //        childvsl.Add(re);
    //    }
    //}

    //public class Record : HorizontalStackLayout
    //{
    //    private Label label;
    //    private Entry entry;
    //    public Record() 
    //    { 
    //        label = new Label();
    //        label.Text = "•";
    //        label.VerticalTextAlignment = TextAlignment.Center;
    //        label.FontSize = 26;

    //        Add(label);

    //        entry = new Entry();
    //        entry.Text = "";
    //        //entry.Completed += Entry_Completed;
    //        entry.Completed += Entry_Tabbed;
    //        Add(entry);
    //    }

    //    private void Entry_Completed(object? sender, EventArgs e)
    //    {
    //        ((Parent as RecordEnhancer).Parent as RecordCollection).addchild();
    //    }

    //    private void Entry_Tabbed(object? sender, EventArgs e)
    //    {
    //        RecordEnhancer reparent = Parent as RecordEnhancer;
    //        RecordCollection rcparent = reparent.Parent as RecordCollection;

    //        RecordEnhancer? newre = rcparent.getpreviouschild(reparent);

    //        if (newre != null) 
    //        {
    //            newre.addchildre(reparent);
    //        }
    //    }
    //}

    //should I actually inherit or maybe class composition would be better idea?
    //if unrecordcollection is not inherited from vsl we wouldn't able to insert
    //it in scrollview

    //rename to recordviewer?
    public class UnrecordCollection : VerticalStackLayout, ISuperable
    {
        public Record? lastFocused;
        public ISuperable? superrecord { get; set; }
        public List<Record> subrecords {  get; set; }
        public UnrecordCollection()
        {
            subrecords = new List<Record>();
            superrecord = null;
        }

        public void addchild(string text = "")
        {
            Record u = new Record(this, text);
            Add(u);
            subrecords.Add(u);
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

        public Record? getpreviousrecord(Record re)
        {
            int argpos = subrecords.IndexOf(re);
            //if index == -1 throw an exception
            if (argpos == 0)
            {
                return null;
            }
            else
            {
                return subrecords[argpos - 1];
            }
        }

        public Record? getnextrecord(Record re) 
        {
            int argpos = subrecords.IndexOf(re);
            //if index == -1 throw an exception
            if (argpos == subrecords.Count - 1)
            {
                return null;
            }
            else
            {
                return subrecords[argpos + 1];
            }
        }
    }

    public class Record : HorizontalStackLayout, ISuperable
    {
        private Label label;
        private Entry entry;
        //public List<Record> subrecords;
        public ISuperable? superrecord { get; set; }//superrecord shouldn't point only to record, it should point anything that implements subrecords interface

        public List<Record> subrecords { get; set; }

        public Record(ISuperable super, string text)
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
            superrecord = super;
        }

        //public Record? getpreviousrecord(Record re)
        //{
        //    int argpos = subrecords.IndexOf(re);
        //    //if index == -1 throw an exception
        //    if (argpos == 0)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        return subrecords[argpos - 1];
        //    }
        //}

        //public Record? getnextrecord(Record re)
        //{
        //    int argpos = subrecords.IndexOf(re);
        //    //if index == -1 throw an exception
        //    if (argpos == subrecords.Count - 1)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        return subrecords[argpos + 1];
        //    }
        //}

        private void shift()
        {
            Padding = new Thickness(Padding.Left + 10, Padding.Top, Padding.Right, Padding.Bottom);
            foreach (Record r in subrecords)
            {
                r.shift();
            }
        }

        private void unshift()
        {
            Padding = new Thickness(Padding.Left - 10, Padding.Top, Padding.Right, Padding.Bottom);
            foreach (Record r in subrecords)
            {
                r.unshift();
            }
        }

        public void tab()
        {
            //Record? prevrec = null;
            //List<Record>? oldlist = null;
            //if (superrecord == null)
            //{
            //    UnrecordCollection unrec = Parent as UnrecordCollection;
            //    prevrec = unrec.getpreviousrecord(this);
            //    oldlist = unrec.subrecords;
            //}
            //else
            //{
            //    prevrec = superrecord.getpreviousrecord(this);
            //    oldlist = superrecord.subrecords;
            //}

            ISuperable? prevrec = superrecord.getpreviousrecord(this);
            List<ISuperable>? oldlist = superrecord.subrecords;

            if (prevrec != null && oldlist != null)
            {
                oldlist.Remove(this);
                prevrec.subrecords.Add(this);
                superrecord = prevrec;

                //batchcommit?
                shift();
            }

            return;
        }

        public void untab()
        {
            if (superrecord == null || superrecord.superrecord == null)
            {
                return;
            }


            //UnrecordCollection uc = Parent as UnrecordCollection;
            //if (nextsuper == null)
            //{
            //    uc.Remove(this);
            //    uc.Add(this);
            //    movechildend();
            //}
            //else//dead branch because  nextsuper is always null for some reason
            //{
            //    int pos = uc.IndexOf(nextsuper);
            //    uc.Remove(this);
            //    uc.Insert(pos - 1, this);
            //    movechild(nextsuper);
            //}

            ISuperable sr = superrecord, ssr = superrecord.superrecord;
            //Record? ssr = null;
            //List<Record>? ssrsubrecords = null;
            //if (sr.superrecord == null)
            //{
            //    ssr = null;
            //    nextsuper = uc.getnextrecord(sr);
            //    ssrsubrecords = uc.subrecords;
            //}
            //else
            //{
            //    ssr = sr.superrecord;
            //    nextsuper = ssr.getnextrecord(sr);            
            //    ssrsubrecords = ssr.subrecords;               
            //}         

            //if (ssrsubrecords != null)
            //{
            //    int parentpos = ssrsubrecords.IndexOf(sr);
            //    sr.subrecords.Remove(this);
            //    superrecord = ssr;
            //    ssrsubrecords.Insert(parentpos + 1, this);
            //}

            UnrecordCollection uc = Parent as UnrecordCollection;
            ISuperable? nextsuper = null;
            nextsuper = ssr.getnextrecord(sr);
            if (nextsuper == null)
            {
                uc.Remove(this);
                uc.Add(this);
                movechildend();
            }
            else//dead branch because  nextsuper is always null for some reason
            {
                int pos = uc.IndexOf(nextsuper);
                uc.Remove(this);
                uc.Insert(pos - 1, this);
                movechild(nextsuper);
            }



            int parentpos = ssr.subrecords.IndexOf(sr);
            sr.subrecords.Remove(this);
            superrecord = sr;
            ssr.subrecords.Insert(parentpos + 1, this);


            unshift();
        }

        private void movechild(Record flagrecord)
        {
            UnrecordCollection uc = Parent as UnrecordCollection;

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
            UnrecordCollection uc = Parent as UnrecordCollection;
            foreach (Record child in subrecords)
            {
                uc.Remove(child);
                uc.Add(child);
                child.movechildend();
            }
        }

        private int move(int pos)
        {
            UnrecordCollection uc = Parent as UnrecordCollection;
            uc.Remove(this);
            uc.Insert(pos, this);
            int i = pos + 1;
            foreach (var item in subrecords)
            {
                i = item.move(i);
            }
            return i;
        }

        private void Entry_Focused(object? sender, EventArgs e)
        {
            (Parent as UnrecordCollection).lastFocused = this;
        }
    }

    public interface ISuperable
    {
        ISuperable? superrecord { get; set; }
        List<ISuperable> subrecords { get; set; }

        public ISuperable? getpreviousrecord(ISuperable re)
        {
            int argpos = subrecords.IndexOf(re);
            //if index == -1 throw an exception
            if (argpos == 0)
            {
                return null;
            }
            else
            {
                return subrecords[argpos - 1];
            }
        }

        public ISuperable? getnextrecord(ISuperable re)
        {
            int argpos = subrecords.IndexOf(re);
            //if index == -1 throw an exception
            if (argpos == subrecords.Count - 1)
            {
                return null;
            }
            else
            {
                return subrecords[argpos + 1];
            }
        }
    }
}
