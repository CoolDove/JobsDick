using Godot;
using System.Collections.Generic;

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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (line.Material is ShaderMaterial shm) {
			shm.SetShaderParameter("spacing", weekScale);
		}
		cursorWeek = (int)(cam.Position.X/weekScale);
		debugLabel.Text = $"{cursorWeek} 周";
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

	public void AddNode(int week) {
		StickNode node = new() {
			week = week,
		};
		nodes.Add(node);

		var mnode = pksStickNode.Instantiate<MStickNode>();
		mnode.data = node;
		nodesParent.AddChild(mnode);
		mnode.Refresh();
	}
}

public class StickNode {
	public string text;
	public int week;
	public string picture;
}
