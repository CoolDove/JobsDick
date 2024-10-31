shader_type canvas_item;

uniform float spacing : hint_range(1, 1000);
uniform float width : hint_range(1, 1000);
uniform vec4 seg_color : source_color;

varying float line_length;
varying vec4 vcolor;

void vertex() {
	line_length = VERTEX.x;
	vcolor = COLOR;
}

void fragment() {
	float f = fract(line_length/spacing);
	f = 1.0-step(0, f*spacing-width);
	COLOR = mix(vcolor, seg_color, f);
}