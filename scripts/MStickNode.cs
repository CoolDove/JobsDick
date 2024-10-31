using Godot;
using System;
using System.IO;

public partial class MStickNode : Node2D
{
	public StickNode data;
	[Export]
	public LineEdit edit;
	[Export]
	public TextureRect texRect;
	[Export]
	public Panel texEmptyPanel;
	[Export]
	public Control emptyButtons;

	[Export]
	public Button btnPaste;

	private bool emptyPanelHovering = false;
	private float targetEmptyPanelSize = 1.0f;

	public override void _Ready() {
		edit.TextChanged += OnEdit;
		texEmptyPanel.MouseEntered += OnMouseEnter;
		texEmptyPanel.MouseExited += OnMouseLeave;
		texEmptyPanel.PivotOffset = texEmptyPanel.Size/2;
		texRect.Texture = null;
	}

	public override void _Process(double delta) {
		if (texEmptyPanel.Visible) {
			float scale = texEmptyPanel.Scale.X;
			if (scale != targetEmptyPanelSize) {
				scale = (targetEmptyPanelSize-scale)*(float)delta*5+scale;
				if (Mathf.Abs(scale-targetEmptyPanelSize) < 0.01f) scale = targetEmptyPanelSize;
				texEmptyPanel.Scale = new (scale, scale);
			}
		}
	}
	public void Refresh() {
		edit.Text = data.text;
		edit.PlaceholderText = $"第{data.week}周";
		var picPath = JobStickMain.GetSavePicPath(data);

		if (texRect.Texture == null) {
			if (File.Exists(picPath)) {
				Image img = Image.LoadFromFile(picPath);
				if (img != null) texRect.Texture = ImageTexture.CreateFromImage(img);
			}
		}
		bool hasTex = texRect.Texture == null;
		texEmptyPanel.Visible = hasTex;
		emptyButtons.Visible = hasTex;

		Position = new (data.week*JobStickMain.instance.weekScale,0);
	}

	public void DropFile(string[] files) {
		var file = files[0];
		Image img = Image.LoadFromFile(file);
		if (img != null) {
			img.SavePng(JobStickMain.GetSavePicPath(data));
			Refresh();
		}
	}

	private void OnEdit(string t) {
		if (data != null) {
			data.text = t;
			JobStickMain.instance.MarkDirty();
		}
	}

	private void OnMouseEnter() {
		JobStickMain.instance.dropTo = this;
		emptyPanelHovering = true;
		targetEmptyPanelSize = 1.1f;
	}
	private void OnMouseLeave() {
		JobStickMain.instance.dropTo = null;
		emptyPanelHovering = false;
		targetEmptyPanelSize = 1;
	}
}
