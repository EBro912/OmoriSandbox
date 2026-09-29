shader_type canvas_item;

instance uniform vec4 blend_color = vec4(0.0);
instance uniform float defeat_tone = 0.0;

varying vec4 modulate;

void vertex() {
	modulate = COLOR;
}

void fragment() {
	vec4 tex = texture(TEXTURE, UV);
	vec3 plain = blend_color.a > 0.0 ? tex.rgb * tex.a : tex.rgb;
	vec3 toned = max(mix(vec3(1.0), vec3(dot(tex.rgb, vec3(0.30, 0.59, 0.11))), tex.a) - 17.0 / 255.0, 0.0);
	vec3 rgb = mix(plain, toned, defeat_tone);
	rgb = mix(rgb, blend_color.rgb, blend_color.a);
	COLOR = vec4(rgb, tex.a) * modulate;
}
