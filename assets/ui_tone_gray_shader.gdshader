shader_type canvas_item;

uniform vec4 tone = vec4(0.0);

void fragment() {
	vec4 tex_color = texture(TEXTURE, UV);
	float luminance = dot(tex_color.rgb, vec3(0.30, 0.59, 0.11));
	vec3 gray = vec3(max(mix(1.0, luminance, tex_color.a) - 80.0 / 255.0, 0.0));
	COLOR = vec4(clamp(gray + tone.rgb, 0.0, 1.0), COLOR.a);
}
