shader_type canvas_item;

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    
    float luminance = dot(tex_color.rgb, vec3(0.30, 0.59, 0.11));
    
    COLOR = vec4(vec3(max(mix(1.0, luminance, tex_color.a) - 80.0 / 255.0, 0.0)), COLOR.a);
}
