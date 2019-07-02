//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

//Textura para DiffuseMap
texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
	Texture = (texDiffuseMap);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

float screen_dx;					// tamaño de la pantalla en pixels
float screen_dy;
float time;


//Input del Vertex Shader
struct VS_INPUT
{
	float4 Position : POSITION0;
	float3 Normal :   NORMAL0;
	float4 Color : COLOR;
	float2 Texcoord : TEXCOORD0;
};

texture g_RenderTarget;
sampler RenderTarget =
sampler_state
{
	Texture = <g_RenderTarget>;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Output del Vertex Shader
struct VS_OUTPUT
{
	float4 Position :        POSITION0;
	float2 Texcoord :        TEXCOORD0;
    float3 Normal :          TEXCOORD1; // Normales
};

//Vertex Shader
VS_OUTPUT vs_main(VS_INPUT Input)
{
	VS_OUTPUT Output;
    Output.Position = mul(Input.Position, matWorldViewProj);
    Output.Normal = Input.Normal;
    Output.Texcoord = Input.Texcoord;
	return(Output);
}

//Pixel Shader
float4 ps_main(float2 Texcoord: TEXCOORD0, float3 N : TEXCOORD1,
	float3 Pos : TEXCOORD2) : COLOR0
{
	//Obtener el texel de textura
	float4 fvBaseColor = tex2D(diffuseMap, Texcoord);
	return fvBaseColor;
}

technique DefaultTechnique
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader = compile ps_3_0 ps_main();
	}
}

void VSCopy(float4 vPos : POSITION, float2 vTex : TEXCOORD0, out float4 oPos : POSITION, out float2 oScreenPos : TEXCOORD0)
{
	oPos = vPos;
	oScreenPos = vTex;
	oPos.w = 1;
}
float r=5;
float c=10;
float4 PSPostProcess(in float2 Tex : TEXCOORD0, in float2 vpos : VPOS) : COLOR0
{
	float x = floor((Tex.x * screen_dx)/r)*r/screen_dx;
	float y = floor((Tex.y * screen_dy)/r)*r/screen_dy;

	return tex2D(RenderTarget, float2(x,y));
}

technique PostProcess
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 VSCopy();
		PixelShader = compile ps_3_0 PSPostProcess();
	}
}

float fish_kU = 0.25f;
float fish_kV = 0.25f;

bool grid = false;

float4 PSPincusion(in float2 Tex : TEXCOORD0, in float2 vpos : VPOS) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float dist = distance(center, Tex);
    Tex -= center;
    float percent = 1.0 - ((0.5 - dist) / 0.5) * fish_kU;
    Tex *= percent;
    Tex += center;
    float4 rta;
    int pos_x = round(Tex.x * screen_dx);
    int pos_y = round(Tex.y * screen_dy);
    if (grid && (pos_x % 50 == 1 || pos_y % 50 == 1))
        rta = float4(1, 1, 1, 1);
    else
        rta = tex2D(RenderTarget, Tex);
    return rta;

}

technique Pincusion
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 PSPincusion();
    }

}