shader_type canvas_item;

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    
    float luminance = dot(tex_color.rgb, vec3(0.2126, 0.7152, 0.0722));
    
    COLOR = vec4(vec3(luminance), tex_color.a * COLOR.a);
}
