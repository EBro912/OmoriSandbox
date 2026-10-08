shader_type canvas_item;

uniform vec4 tone = vec4(0.0);

void fragment() {
	vec4 tex_color = texture(TEXTURE, UV);
	float luminance = dot(tex_color.rgb, vec3(0.2126, 0.7152, 0.0722));
	COLOR = vec4(clamp(vec3(luminance) + tone.rgb, 0.0, 1.0), COLOR.a);
}
