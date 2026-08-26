namespace MauiApp2;

public partial class NewContent1 : ContentView
{
	public NewContent1(NewContent1? parent)
	{
		InitializeComponent();
        HierarchicalParent = parent;
        shift = (uint)(HierarchicalParent is null ? 0 : (HierarchicalParent?.shift + 10));
    }

	public NewContent1? HierarchicalParent;

    public new IList<IView> Children//records
	{
		get => cvvsl.Children;
	}

	public uint shift
	{
		get => (uint)Padding.Left;
		set => Padding = new Thickness(Padding.Left + value, Padding.Top, Padding.Right, Padding.Bottom);
	}

	public void addchild()//add record
	{
		Children.Add(new NewContent1(this));
		return;
	}

    private void Entry_Completed(object sender, EventArgs e)
    {
		addchild();
    }
}