float4x4 view;
float4x4 proj;
float4x4 world;
float visibility;
float3 color;
float ambient;
float3 lightDir;
bool enableLight;
texture FOWTexture;
texture TerrainTexture;
texture UnitTexture;
texture CliffTexture;

struct VS_INPUT { float4 position : POSITION; float2 TextureCoords : TEXCOORD1; };
struct VS_OUTPUT { float4 position : POSITION; float4 color : COLOR0; float4 worldPos : TEXCOORD0; float2 TextureCoords : TEXCOORD1; };
struct TexVertexToPixel { float4 Position : POSITION; float4 Color : COLOR0; float3 Normal : TEXCOORD0; float2 TextureCoords : TEXCOORD1; float4 LightDirection : TEXCOORD2; float2 CliffCoords : TEXCOORD3; };
struct TexPixelToFrame { float4 Color : COLOR0; };

sampler unitSampler = sampler_state { Texture = <UnitTexture>; MipFilter = LINEAR; MinFilter = LINEAR; MagFilter = LINEAR; AddressU  = mirror; AddressV  = mirror; };
sampler fowSampler = sampler_state { Texture = <FOWTexture>; MipFilter = LINEAR; MinFilter = LINEAR; MagFilter = LINEAR; AddressU  = Clamp; AddressV  = Clamp; };
sampler fowTerrainSampler = sampler_state { Texture = <FOWTexture>; MagFilter = LINEAR; MinFilter = LINEAR; MipFilter = LINEAR; AddressU = mirror; AddressV = mirror; };
sampler textureSampler = sampler_state { Texture = <TerrainTexture>; MagFilter = LINEAR; MinFilter = LINEAR; MipFilter = LINEAR; AddressU = mirror; AddressV = mirror; };
sampler cliffSampler = sampler_state { Texture = <CliffTexture>; MagFilter = LINEAR; MinFilter = LINEAR; MipFilter = LINEAR; AddressU = mirror; AddressV = mirror; };



VS_OUTPUT UnitVS(VS_INPUT In)
{
    VS_OUTPUT Out = (VS_OUTPUT)0;
    float4x4 viewProj = mul(view, proj);
    float4x4 worldViewProj= mul(world, viewProj);
    Out.position = mul( In.position , worldViewProj);
    Out.worldPos = mul(In.position, world);
    Out.TextureCoords = In.TextureCoords;
    return Out;
}

float4 UnitPS(VS_OUTPUT In) : COLOR
{
    float4 data = tex2D(unitSampler, In.TextureCoords);
    return float4(normalize(data + ((1 - data.a) * float4(color, 1.0f))).rgb, visibility);
}

technique Unit
{
    pass P0
    {
        vertexShader = compile vs_2_0 UnitVS();
        pixelShader  = compile ps_2_0 UnitPS();
    }
}

VS_OUTPUT SelectionVS(VS_INPUT In)
{
    VS_OUTPUT Out = (VS_OUTPUT)0;
    float4x4 viewProj = mul(view, proj);
    float4x4 worldViewProj= mul(world, viewProj);
    Out.position = mul( In.position , worldViewProj);
    Out.worldPos = mul(In.position, world);
    return Out;
}

float4 SelectionPS(VS_OUTPUT In) : COLOR
{
    return float4(color, 0.3f);
}

technique Selection
{
    pass P0
    {
        vertexShader = compile vs_2_0 SelectionVS();
        pixelShader  = compile ps_2_0 SelectionPS();
    }
}


TexVertexToPixel TerrainVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0, float2 inCliffCoords: TEXCOORD1)
{	
	TexVertexToPixel Output = (TexVertexToPixel)0;
	float4x4 preViewProjection = mul (view, proj);
	float4x4 preWorldViewProjection = mul (world, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.Normal = mul(normalize(inNormal), world);
	
	Output.TextureCoords = inTexCoords;
	Output.CliffCoords = inCliffCoords;

	Output.LightDirection.xyz = -lightDir;
	Output.LightDirection.w = 1;	
    
	return Output;    
}

TexPixelToFrame TerrainPS(TexVertexToPixel PSIn) 
{
	TexPixelToFrame Output = (TexPixelToFrame)0;		
    
	float lightingFactor = 1;
	/*if (enableLight)
		lightingFactor = saturate(saturate(dot(PSIn.Normal, PSIn.LightDirection)) + ambient);*/
	
	float4 sampled = (float4)0;
	if (PSIn.CliffCoords.x >= 0)
	    sampled = tex2D(cliffSampler, PSIn.CliffCoords);
	else
		sampled = tex2D(textureSampler, PSIn.TextureCoords);
	
	Output.Color = sampled * lightingFactor * tex2D(fowTerrainSampler, PSIn.TextureCoords);
	

	return Output;
}

technique Terrain
{
    pass P0
    {
        vertexShader = compile vs_2_0 TerrainVS();
        pixelShader  = compile ps_2_0 TerrainPS();
    }
}
