// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using Godot;
using System;

public class Hud : Node2D {

	private InputEventScreenTouch currTEvent;
	private System.Timers.Timer currTimer;
	private VBoxContainer elemHolder = new VBoxContainer();
	private Button startBt = new Button() { Text = "Start", ToggleMode = true };
	private DateTime futureTime = DateTime.Now;
	private Label timeRemainingLb = new Label();
	private Label clicksLb = new Label();

	private TextureRect vignette = new TextureRect() {
		Expand = true,
		Texture = new GradientTexture() {
			Gradient = new Gradient() { Colors = new[] { Colors.Transparent } }
		},
		Material = new ShaderMaterial() {
			Shader = new Shader() {
				Code = @"
					shader_type canvas_item;
					void fragment() {
						float iRad = 0.3;
						float oRad = 1.0;
						float opac = 0.5;
						vec2 uv = SCREEN_UV;
					    vec2 cent = uv - vec2(0.5);
					    vec4 tex = textureLod(SCREEN_TEXTURE, SCREEN_UV, 0.0);
					    vec4 col = vec4(1.0);
					    col.rgb *= 1.0 - smoothstep(iRad, oRad, length(cent));
					    col *= tex;
					    col = mix(tex, col, opac);
					    COLOR = col;
					}"
			}
		}
	};

	public override void _Ready() {
		InitVignette();

		elemHolder.RectMinSize = GetViewportRect().Size;
		elemHolder.MarginTop = 20;

		elemHolder.AddChild(startBt);
		startBt.Connect("toggled", this, nameof(OnStartButtonToggled));

		StyleElement(startBt);
		StyleElement(timeRemainingLb);
		timeRemainingLb.Align = Label.AlignEnum.Center;
		var tRem = TimeSpan.FromMinutes(1);
		timeRemainingLb.Text = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", tRem.Hours, tRem.Minutes, tRem.Seconds, tRem.Milliseconds / 10);
		elemHolder.AddChild(timeRemainingLb);

		StyleElement(clicksLb);
		clicksLb.Align = Label.AlignEnum.Center;
		clicksLb.Text = 0.ToString();
		elemHolder.AddChild(clicksLb);

		AddChild(elemHolder);

	}

	private void StyleElement(Control control) {
		control.SizeFlagsHorizontal = (int)Control.SizeFlags.ShrinkCenter;
		control.RectMinSize = new Vector2(elemHolder.RectMinSize.x * 0.7f, 40);
		control.AddFontOverride("font", new DynamicFont() { FontData = GD.Load<DynamicFontData>("res://assets/default/Tuffy_Bold.ttf"), Size = 40 });
	}

	private void OnStartButtonToggled(bool pressed) {

		if (pressed) {
			startBt.Text = "Stop";
			currTimer = new System.Timers.Timer() { AutoReset = false, Interval = 60000, Enabled = true };
			currTimer.Elapsed += (z, zz) => {
				currTimer.Close();
				currTimer.Dispose();
				currTimer = null;
				startBt.Text = "Start";
				startBt.Pressed = false;
			};
			clicksLb.Text = 0.ToString();
			futureTime = DateTime.Now.AddMilliseconds(currTimer.Interval);

		} else {
			startBt.Text = "Start";
			if (currTimer != null) {
				currTimer.Stop();
				currTimer.Close();
				currTimer.Dispose();
			}
			currTimer = null;
		}
	}

	public override void _Draw() {
		if (currTEvent != null && currTimer != null) {
			DrawCircle(currTEvent.Position, 50, Color.FromHsv(GD.Randf(), 1, 1));
		}

		DrawBorder(this);
	}

	public override void _Process(float delta) {

		if (currTimer != null) {
			var tRem = futureTime - DateTime.Now;
			timeRemainingLb.Text = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", tRem.Hours, tRem.Minutes, tRem.Seconds, tRem.Milliseconds / 10);
		}
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventScreenTouch te && te.Pressed) {
			if (currTimer != null) {
				clicksLb.Text = (int.Parse(clicksLb.Text) + 1).ToString();
			}

			currTEvent = te;
			Update();
		}
	}

	private void InitVignette() {
		vignette.RectMinSize = GetViewportRect().Size;
		AddChild(vignette);
		if (Lib.Node.VignetteEnabled) {
			vignette.Show();
		} else {
			vignette.Hide();
		}
	}

	public static void DrawBorder(CanvasItem canvas) {
		if (Lib.Node.BoderEnabled) {
			var vps = canvas.GetViewportRect().Size;
			int thickness = 4;
			var color = new Color(Lib.Node.BorderColorHtmlCode);
			canvas.DrawLine(new Vector2(0, 1), new Vector2(vps.x, 1), color, thickness);
			canvas.DrawLine(new Vector2(1, 0), new Vector2(1, vps.y), color, thickness);
			canvas.DrawLine(new Vector2(vps.x - 1, vps.y), new Vector2(vps.x - 1, 1), color, thickness);
			canvas.DrawLine(new Vector2(vps.x, vps.y - 1), new Vector2(1, vps.y - 1), color, thickness);
		}
	}
}
