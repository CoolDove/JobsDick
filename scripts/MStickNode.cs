using Godot;
using System;

public partial class MStickNode : Node2D
{
	public StickNode data;
	[Export]
	public LineEdit edit;
	[Export]
	public TextureRect texRect;

	public override void _Ready() {
		edit.TextChanged += OnEdit;
	}

	public void Refresh() {
		edit.Text = data.text;
		edit.PlaceholderText = $"第{data.week}周";

		Position = new (data.week*JobStickMain.instance.weekScale,0);
	}

	public void OnEdit(string t) {
		if (data != null) {
			data.text = t;
			JobStickMain.instance.MarkDirty();
		}
	}

}
