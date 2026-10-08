shader_type canvas_item;
render_mode blend_add;

uniform vec4 tone = vec4(0.0);

void fragment() {
	float x = 1.0 - tone.a * 2.0 / 3.0;
	float y = (x - 1.0) * -0.5;
	vec3 c = COLOR.rgb;
	COLOR.rgb = vec3(x * c.r + y * c.g + y * c.b, y * c.r + x * c.g + y * c.b, y * c.r + y * c.g + x * c.b);
}
