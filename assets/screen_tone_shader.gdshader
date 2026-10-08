shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;
uniform vec4 tone = vec4(0.0);

void fragment() {
	vec3 c = texture(screen_texture, SCREEN_UV).rgb;
	float x = 1.0 - tone.a * 2.0 / 3.0;
	float y = (x - 1.0) * -0.5;
	vec3 saturated = vec3(x * c.r + y * c.g + y * c.b, y * c.r + x * c.g + y * c.b, y * c.r + y * c.g + x * c.b);
	COLOR = vec4(clamp(saturated + tone.rgb, 0.0, 1.0), 1.0);
}
