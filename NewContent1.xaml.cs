using static System.Net.Mime.MediaTypeNames;

namespace MauiApp2;

public partial class NewContent1 : ContentView
{
	private Entry? _lastFocused;
	public Entry? LastFocused
	{
		get => _lastFocused;
		set
		{
			_lastFocused = value;
			if (HierarchicalParent is not null)
			{
                HierarchicalParent.LastFocused = value;
            }
		}
	}
	public NewContent1(NewContent1? parent)
	{
		InitializeComponent();
        HierarchicalParent = parent;
        shift = (uint)(HierarchicalParent is null ? 0 : (HierarchicalParent?.shift + 10));
		
    }

	public NewContent1? HierarchicalParent;

 //   public new IList<IView> Children//records
	//{
	//	get => cvvsl.Children;
	//}

	public uint shift
	{
		get => (uint)Padding.Left;
		set => Padding = new Thickness(Padding.Left + value, Padding.Top, Padding.Right, Padding.Bottom);
	}

	public void addrecord(object sender, string text = "")
	{
		if (sender is not Entry)
		{
			return;
		}

		HorizontalStackLayout senderhsl = (sender as Entry).Parent as HorizontalStackLayout;

        Label label = new Label();
        label.Text = "•";
        label.VerticalTextAlignment = TextAlignment.Center;
        label.FontSize = 26;

        Entry entry = new Entry();
        entry.Completed += Entry_Completed;
		entry.Text = text;
		entry.Focused += Entry_Focused;

        HorizontalStackLayout newHsl = new HorizontalStackLayout();
        newHsl.Add(label);
        newHsl.Add(entry);

		cvvsl.Insert(cvvsl.IndexOf(senderhsl)+1, newHsl);
		return;
    }

    private void Entry_Completed(object sender, EventArgs e)
    {
		addrecord(sender);
    }

    private void Entry_Focused(object? sender, FocusEventArgs e)
	{
        LastFocused = sender as Entry;
	}
}