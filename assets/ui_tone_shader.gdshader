shader_type canvas_item;

uniform vec4 tone = vec4(0.0);

void fragment() {
	float x = 1.0 - tone.a * 2.0 / 3.0;
	float y = (x - 1.0) * -0.5;
	vec3 c = COLOR.rgb;
	vec3 saturated = vec3(x * c.r + y * c.g + y * c.b, y * c.r + x * c.g + y * c.b, y * c.r + y * c.g + x * c.b);
	COLOR.rgb = clamp(saturated + tone.rgb, 0.0, 1.0);
}
