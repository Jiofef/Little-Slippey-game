shader_type canvas_item;
render_mode unshaded;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

uniform sampler2D shadow_to_light;
uniform float brightness : hint_range(-10, 10) = 1.5;

uniform float shadow_max_dist : hint_range(0,1000) = 100;
uniform sampler2D gradientTexture;

vec4 get_gradient_color(float position) { 
    return texture(gradientTexture, vec2(position, 0.5)); 
}
vec4 get_shadow_to_light_gradient_color(float position) { 
    return texture(shadow_to_light, vec2(position, 0.5)); 
}

void fragment() { 
	vec2 to_frag = vec2(0.5 - SCREEN_UV.x, 0.5 - SCREEN_UV.y);
	float to_point = length(to_frag);
	
	float ang_rad = atan(to_frag.x, to_frag.y);
	
    vec2 dir = vec2(sin(ang_rad),cos(ang_rad)); 
    vec2 at = screen_uv_to_sdf(SCREEN_UV); 
    float accum = 0.0;
    while(accum < shadow_max_dist * to_point) {
        float d = texture_sdf(at);
        accum+=d;
        if (d < 0.01) {
            break;
        }
        at += d * dir;
    }
    float alpha = min(1.0, accum/shadow_max_dist / to_point);

    // the gradient controls the falloff of the shadow
    alpha = get_gradient_color(alpha).r;
	
	vec4 pxc = textureLod(screen_texture, SCREEN_UV, 0.0);
	
    vec4 c = pxc;
	c = get_shadow_to_light_gradient_color(alpha);
	
	c = mix(vec4(0.0, 0.0, 0.0, c.a - alpha * c.a), c, alpha * brightness);
	

    COLOR = c;
}