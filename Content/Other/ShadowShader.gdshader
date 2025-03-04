shader_type canvas_item;
render_mode unshaded;

uniform vec4 color : source_color; 

uniform float max_dist : hint_range(0,1000) = 100;
uniform sampler2D gradientTexture;

vec4 get_gradient_color(float position) { 
    return texture(gradientTexture, vec2(position, 0.5)); 
}

void fragment() { 
	vec2 to_frag = vec2(0.5 - SCREEN_UV.x, 0.5 - SCREEN_UV.y);
	float to_point = length(to_frag);
	
	float ang_rad = atan(to_frag.x, to_frag.y);
	
    vec2 dir = vec2(sin(ang_rad),cos(ang_rad)); 
    vec2 at = screen_uv_to_sdf(SCREEN_UV); 
    float accum = 0.0;
    while(accum < max_dist * to_point) {
        float d = texture_sdf(at);
        accum+=d;
        if (d < 0.01) {
            break;
        }
        at += d * dir;
    }
    float alpha = 1.0-min(1.0,accum/max_dist / to_point);
    
    // the gradient controls the falloff of the shadow
    alpha = get_gradient_color(alpha).r;
    
    COLOR = vec4(color.rgb,alpha * color.a);
}