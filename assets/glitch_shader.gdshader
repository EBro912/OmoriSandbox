shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;

varying vec2 local_position;

uint next_hash(uint value) {
	uint state = value * 747796405u + 2891336453u;
	uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
	return (word >> 22u) ^ word;
}

float next_random(inout uint seed) {
	seed = next_hash(seed);
	return float(seed >> 8u) / 16777216.0;
}

void vertex() {
	local_position = VERTEX;
}

void fragment() {
	uint seed = next_hash(uint(floor(TIME * 60.0)));
	int count = 10 + int(next_random(seed) * 5.0);
	float minimum = min(8.0 / 512.0, 0.9 / float(count));
	float limit = sqrt(1.0 / float(count));
	float sizes[14];
	for (int i = 0; i < 14; i++) {
		sizes[i] = 0.0;
	}
	float left = 1.0;
	for (int i = 0; i < 13; i++) {
		if (i < count - 1) {
			sizes[i] = max(limit * left * next_random(seed), minimum);
			left -= sizes[i];
		}
	}
	sizes[count - 1] = left;
	for (int t = 13; t > 0; t--) {
		if (t < count) {
			int k = int(next_random(seed) * float(t));
			float held = sizes[t];
			sizes[t] = sizes[k];
			sizes[k] = held;
		}
	}
	int slice = count - 1;
	float top = 0.0;
	for (int i = 0; i < 14; i++) {
		if (i < count && slice == count - 1 && SCREEN_UV.y >= top && SCREEN_UV.y < top + sizes[i]) {
			slice = i;
		}
		top += sizes[i];
	}
	float shift = 0.0;
	for (int i = 0; i < 14; i++) {
		if (i < count) {
			float amount = next_random(seed);
			float direction = next_random(seed) < 0.5 ? -1.0 : 1.0;
			if (i == slice) {
				shift = amount * direction;
			}
		}
	}
	float units_per_pixel = max(abs(dFdx(local_position.x)), 0.0001);
	vec2 uv = vec2(SCREEN_UV.x + shift * 100.0 * SCREEN_PIXEL_SIZE.x / units_per_pixel, SCREEN_UV.y);
	if (uv.x < 0.0 || uv.x > 1.0) {
		COLOR = vec4(0.0, 0.0, 0.0, 1.0);
	} else {
		COLOR = vec4(texture(screen_texture, uv).rgb, 1.0);
	}
}
