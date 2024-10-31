using Godot;
using System;

public partial class MStickNode : Node2D
{
	public StickNode data;
	[Export]
	public LineEdit edit;
	[Export]
	public TextureRect texRect;

	public void Refresh() {
		edit.Text = data.text;
		edit.PlaceholderText = $"第{data.week}周";

		Position = new (data.week*JobStickMain.instance.weekScale,0);
	}

}
