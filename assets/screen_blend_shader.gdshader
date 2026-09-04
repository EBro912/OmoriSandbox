shader_type canvas_item;
render_mode blend_mul;

// imitates a Screen blend mode by fragmenting after an additive shader has already been applied
void fragment() {
	vec3 weighted = COLOR.rgb * COLOR.a;
	COLOR = vec4(1.0 - weighted, 1.0);
}
