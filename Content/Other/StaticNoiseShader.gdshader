shader_type canvas_item;
uniform float u_amount = 0.1;

float get_noise(vec2 uv) {
    return fract(sin(dot(uv ,vec2(12.9898,78.233))) * 43758.5453);
}

void fragment() {
    float n = 2.0 * get_noise(UV + vec2(TIME, 0.0)) - 1.0;
    COLOR = texture(TEXTURE, UV) + n * u_amount;
}