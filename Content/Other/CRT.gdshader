shader_type canvas_item;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;

vec2 warp(vec2 uv){
	vec2 delta = uv - 0.5;
	float delta2 = dot(delta.xy, delta.xy);
	float delta4 = delta2 * delta2;
	float delta_offset = delta4 * 0.2;
	
	return uv + delta * delta_offset;
}

float border (vec2 uv){
	float radius = min(0.2, 0.08);
	radius = max(min(min(abs(radius * 2.0), abs(1.0)), abs(1.0)), 1e-5);
	vec2 abs_uv = abs(uv * 2.0 - 1.0) - vec2(1.0, 1.0) + radius;
	float dist = length(max(vec2(0.0), abs_uv)) / radius;
	float square = smoothstep(0.96, 1.0, dist);
	return clamp(1.0 - square, 0.0, 1.0);
}

float vignette(vec2 uv){
	uv *= 1.0 - uv.xy;
	float vignette = uv.x * uv.y * 15.0;
	return pow(vignette, 0.125);
}

void fragment()
{
	vec2 uv = warp(SCREEN_UV);
	vec2 text_uv = uv;
	vec2 roll_uv = vec2(0.0);
	float time = TIME;
	
	float roll_line = 0.0;
	roll_line = smoothstep(0.3, 0.9, sin(uv.y * 15.0 - (time * 8.0) ) );
	roll_line *= roll_line * smoothstep(0.3, 0.9, sin(uv.y * 15.0 * 1.8 - (time * 8.0 * 1.8) ) );
	roll_uv = vec2(( roll_line * 0.001 * (1.-UV.x)), 0.0);
	
	vec4 text;
	text.r = texture(SCREEN_TEXTURE, text_uv + roll_uv * 0.8).r;
	text.g = texture(SCREEN_TEXTURE, text_uv + roll_uv * 1.2).g;
	text.b = texture(SCREEN_TEXTURE, text_uv + roll_uv).b;
	text.a = 1.0;
	
	uv = warp(UV);

	float scanlines = 0.5;
	scanlines = smoothstep(0.0, 0.5, abs(sin(uv.y * (480.0 * 3.14159265))));
	text.rgb = mix(text.rgb, text.rgb * vec3(scanlines), 0.1);
	
	
	text.rgb *= border(uv);
	text.rgb *= vignette(uv);
	
	COLOR = text;
}