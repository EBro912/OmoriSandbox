shader_type canvas_item;

uniform float amount = 1.0;

void fragment() {
	COLOR.rgb = mix(COLOR.rgb, vec3(0.2 * (COLOR.r + COLOR.g + COLOR.b)), amount);
}
