using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

public partial class JobStickMain : Node
{
	public static JobStickMain instance;

	[Export]
	public Camera2D cam;

	[Export]
	public PackedScene pksStickNode;

	[Export]
	public Line2D line;
	
	[Export]
	public Control ui;

	[Export]
	public Label debugLabel;

	public Node2D nodesParent;

	public int cursorWeek;

	public float weekScale = 200;

	[Export]
	public Button btnAddNode;

	public List<StickNode> nodes = new();


	private bool _dirty;
	private bool _forceSave;
	private double saveCheckInterval=1;

	public override void _Ready() {
		instance = this;
		line.Points = new Vector2[]{
			Vector2.Zero,
			new (int.MaxValue, 0),
		};
		nodesParent = new();
		nodesParent.Name = "StickNodesRoot";
		AddChild(nodesParent);

		btnAddNode.Pressed += ()=> {
			bool ok = true;
			foreach (var n in nodes) {
				if (n.week == cursorWeek) {
					ok = false;
					break;
				}
			}
			if (ok) {
				AddNode(cursorWeek);
			}
		};

		// ** read
		var path = CheckSavePath();
		if (File.Exists(path)) {
			var jsonStr = File.ReadAllText(path);
			var doc = JsonConvert.DeserializeObject<JobStickDoc>(jsonStr);
			foreach (var n in doc.nodes) {
				AddNodeIntern(n);
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (line.Material is ShaderMaterial shm) {
			shm.SetShaderParameter("spacing", weekScale);
		}
		cursorWeek = (int)(cam.Position.X/weekScale);
		debugLabel.Text = $"{cursorWeek} 周";

		saveCheckInterval -= delta;
		if (saveCheckInterval<=0 || _forceSave) {
			if (_dirty || _forceSave) {
				Save();
				_dirty = false;
				_forceSave = false;
			}
			saveCheckInterval = 1;
		}
	}

	public void MarkDirty(bool force=false) {
		_dirty = true;
		_forceSave |= force;
		saveCheckInterval = 1;
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseButton eMbutton) {
			float zoom = 0;
			if (eMbutton.ButtonIndex == MouseButton.WheelUp) {
				zoom = 0.1f;
			} else if (eMbutton.ButtonIndex == MouseButton.WheelDown) {
				zoom = -0.1f;
			}
			float to = cam.Zoom.X + zoom;
			to = Mathf.Clamp(to, 0.3f, 2.0f);
			cam.Zoom = new (to, to);
		} else if (@event is InputEventMouseMotion eMmotion) {
			if (Input.IsMouseButtonPressed(MouseButton.Middle)) {
				cam.Position -= new Vector2(eMmotion.Relative.X / cam.Zoom.X, 0);
				cam.Position = new (Mathf.Max(0, cam.Position.X), 0);
			}
		}
	}

	private void AddNodeIntern(StickNode node, bool dirty=false) {
		nodes.Add(node);

		var mnode = pksStickNode.Instantiate<MStickNode>();
		mnode.data = node;
		nodesParent.AddChild(mnode);
		mnode.Refresh();
		if (dirty) MarkDirty();
	}
	public void AddNode(int week) {
		StickNode node = new() {
			week = week,
		};
		nodes.Add(node);

		var mnode = pksStickNode.Instantiate<MStickNode>();
		mnode.data = node;
		nodesParent.AddChild(mnode);
		mnode.Refresh();
		MarkDirty();
	}

	public void Save() {
		JobStickDoc doc = new();
		doc.nodes = nodes;
		var jsonStr = JsonConvert.SerializeObject(doc, Formatting.Indented);
		File.WriteAllText(CheckSavePath(), jsonStr);
		GD.Print("saved");
	}

	private string CheckSavePath() {
		var path = System.Environment.GetEnvironmentVariable("LOCALAPPDATA");
		path = Path.Join(path, "JobStick");
		if (!Directory.Exists(path)) {
			Directory.CreateDirectory(path);
		}
		return Path.Join(path, "nodes.json");
	}
}

public class StickNode {
	public string text;
	public int week;
	public bool unlocked;
	public bool got;
}
