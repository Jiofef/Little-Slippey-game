shader_type canvas_item;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec2 resolution = 1.0 / SCREEN_PIXEL_SIZE;
	vec2 uv = UV;
	
	vec2 current_pixel = SCREEN_UV * resolution;
	float dist = distance(current_pixel, resolution / 2.0);
	vec3 final_color = vec3(0.0);
	final_color.r = sin(current_pixel.x * TIME + current_pixel.y / 5.0 * dist) * cos(tan(TIME * 0.3));
	final_color.g = final_color.r / sin(current_pixel.y * TIME / 2.0) / 2.0;
	
	COLOR.rgb = final_color;
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
