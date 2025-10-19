$input a_position
$output v_texcoord0


void main()
{
	v_texcoord0 = a_position.xy * vec2(0.5, -0.5) + 0.5;
	
	gl_Position = vec4(a_position.xy, 0.0, 1.0);
}