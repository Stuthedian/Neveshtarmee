namespace MauiApp2;

public partial class NewContent1 : ContentView
{
	public NewContent1()
	{
		InitializeComponent();
	}

	public IList<IView> children//records
	{
		get => cvvsl.Children;
	}
	public uint shift
	{
		get => shift;
		set => shift = (Parent is NewContent1) ? (Parent as NewContent1).shift+10 : 0;
	}

	public void addchild()//add record
	{
		children.Add(new NewContent1());
		return;
	}
}