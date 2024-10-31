using Godot;
using System;
using System.IO;
#if GODOT_WINDOWS
using Clowd.Clipboard.Bitmaps;
using Clowd.Clipboard;
#endif

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
	public Button btnRemove;

	[Export]
	public Control labelPaste;

	private bool emptyPanelHovering = false;
	private float targetEmptyPanelSize = 1.0f;
	private float targetPasteLabelAlpha = 0.0f;

	public override void _Ready() {
		edit.TextChanged += OnEdit;
		texEmptyPanel.MouseEntered += OnMouseEnter;
		texEmptyPanel.MouseExited += OnMouseLeave;
		texEmptyPanel.PivotOffset = texEmptyPanel.Size/2;
		texRect.Texture = null;
		btnRemove.Pressed += OnRemove;
		var modu = labelPaste.SelfModulate;
		modu.A = 0;
		labelPaste.SelfModulate = modu;
	}

	public override void _Process(double delta) {
		if (texEmptyPanel.Visible) {
			float scale = texEmptyPanel.Scale.X;
			if (scale != targetEmptyPanelSize) {
				scale = (targetEmptyPanelSize-scale)*(float)delta*5+scale;
				if (Mathf.Abs(scale-targetEmptyPanelSize) < 0.01f) scale = targetEmptyPanelSize;
				texEmptyPanel.Scale = new (scale, scale);
			}

			var modu = labelPaste.SelfModulate;
			float alpha = modu.A;
			if (alpha != targetPasteLabelAlpha) {
				alpha = (targetPasteLabelAlpha-alpha)*(float)delta*5+alpha;
				if (Mathf.Abs(alpha-targetPasteLabelAlpha) < 0.01f) alpha = targetPasteLabelAlpha;
				modu.A = alpha;
				labelPaste.SelfModulate = modu;
			}
		}
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseButton eMouseBtn) {
			if (emptyPanelHovering && eMouseBtn.Pressed && eMouseBtn.ButtonIndex == MouseButton.Left) {
				OnPaste();
			}
		}
	}


	public void Refresh(bool forceLoadPicture=false) {
		edit.Text = data.text;
		edit.PlaceholderText = $"第{data.week}周";
		var picPath = JobStickMain.GetSavePicPath(data);

		if (forceLoadPicture) texRect.Texture = null;
		if (texRect.Texture == null) {
			if (File.Exists(picPath)) {
				Image img = Image.LoadFromFile(picPath);
				if (img != null) texRect.Texture = ImageTexture.CreateFromImage(img);
			}
		}
		bool hasTex = texRect.Texture == null;
		texEmptyPanel.Visible = hasTex;
		emptyButtons.Visible = hasTex;
		btnRemove.Visible = !hasTex;

		Position = new (data.week*JobStickMain.instance.weekScale,0);
	}

	public void DropFile(string[] files) {
		var file = files[0];
		Image img = Image.LoadFromFile(file);
		if (img != null) {
			UploadImage(img);
			Refresh();
		}
	}

	private void OnEdit(string t) {
		if (data != null) {
			data.text = t;
			JobStickMain.instance.MarkDirty();
		}
	}

	public void UploadImage(Image img) {
		var picPath = JobStickMain.GetSavePicPath(data);
		if (File.Exists(picPath)) File.Delete(picPath);
		if (img == null) return;
		img.SavePng(JobStickMain.GetSavePicPath(data));
	}

	private void OnPaste() {
#if GODOT_WINDOWS
		var bitmap = ClipboardGdi.GetImage();
		if (bitmap != null) {
			UploadImage(null);
			bitmap.Save(JobStickMain.GetSavePicPath(data));// manually save
			Refresh();
		}
#endif
	}
	private void OnRemove() {
		var picPath = JobStickMain.GetSavePicPath(data);
		if (File.Exists(picPath)) File.Delete(picPath);
		Refresh(true);
	}

	private void OnMouseEnter() {
		JobStickMain.instance.dropTo = this;
		emptyPanelHovering = true;
		targetEmptyPanelSize = 1.1f;
		targetPasteLabelAlpha = 1;
	}
	private void OnMouseLeave() {
		JobStickMain.instance.dropTo = null;
		emptyPanelHovering = false;
		targetEmptyPanelSize = 1;
		targetPasteLabelAlpha = 0;
	}
}
