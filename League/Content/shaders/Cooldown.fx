float4x4 World;
float4x4 View;
float4x4 Projection;
float Complete;

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
   
   if (input.Position.x == 0.0f && input.Position.z == 0.0f)  {
       output.Color = float4(0.0f, 0.0f, 0.0f, 1.0f);
   } else {
            float ca = acos(input.Position.x / 3.6f);
            float sa = asin(input.Position.z / 3.6f);
            
            int quad = 0;
            if (sa < 0 && ca > 0) quad = 2;
            if (ca < 0 && sa > 0) quad = 1;
            if (ca < 0 && sa < 0) quad = 3;
            
            float angle = 3.1415926f / 2.0f * quad + max(sa, ca);
            
            if (angle < Complete * 3.1415926f * 2.0f) {
                output.Color = float4(0.0f, 0.0f, 0.0f, 0.0f);
            } else {
                output.Color = float4(0.0f, 0.0f, 0.0f, 1.0f);
            }
    }

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return input.Color;
}

technique Cooldown
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_1_1 PixelShaderFunction();
    }
}
