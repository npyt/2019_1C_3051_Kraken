/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

float screen_dx = 1024;
float screen_dy = 768;

float3 DiffuseLightDirection = float3(0, 230, -230);
float4 DiffuseColor = float4(1, 1, 1, 1);
float DiffuseIntensity = 1.0;

float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.1;

float Shininess = 100;
float4 SpecularColor = float4(1, 1, 1, 1);    
float SpecularIntensity = 1;
float3 ViewVector = float3(0, 1, 0);



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

float camaraX = 0;
float camaraY = 0;
float camaraZ = 0;

/**************************************************************************************/
/* RenderScene */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 Texcoord : TEXCOORD0;
	float4 Normal : NORMAL0;
};

//Output del Vertex Shader
struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float2 RealPos : TEXCOORD1;
    float4 Color : COLOR0;
	float4 Normal : NORMAL0;
};

//Vertex Shader
VS_OUTPUT vs_main(VS_INPUT Input)
{
    VS_OUTPUT Output;
    Output.RealPos = Input.Position;

	//Proyectar posicion
    Output.Position = mul(Input.Position, matWorldViewProj);
   
    float4 normal = normalize(mul(Input.Normal, matInverseTransposeWorld));
    float lightIntensity = dot(normal, DiffuseLightDirection);

	//Propago las coordenadas de textura
    Output.Texcoord = Input.Texcoord;

	//Propago el color x vertice
    Output.Color = saturate(DiffuseColor * DiffuseIntensity * lightIntensity);

	Output.Normal = normal;

    return (Output);
}

//Pixel Shader
float4 ps_main(VS_OUTPUT Input) : COLOR0
{
	ViewVector = float3(camaraX, camaraY, camaraZ);

	float3 light = normalize(DiffuseLightDirection);
    float3 normal = normalize(Input.Normal);
    float3 r = normalize(2 * dot(light, normal) * normal - light);
    float3 v = normalize(mul(normalize(ViewVector), matWorld));

    float dotProduct = dot(r, v);
    float4 specular = SpecularIntensity * SpecularColor * max(pow(dotProduct, Shininess), 0) * length(Input.Color);

    return saturate(tex2D(diffuseMap, Input.Texcoord) * Input.Color + AmbientColor * AmbientIntensity + specular);
}


// ------------------------------------------------------------------
technique RenderScene
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 ps_main();
    }
}
