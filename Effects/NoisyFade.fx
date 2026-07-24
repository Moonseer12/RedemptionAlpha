sampler uImage0 : register(s0);
sampler noiseTexture : register(s1);

float2 uImageSize0;
float4 uSourceRect;
float2 uImageSize1;

float progress;
float2 noiseOffset;
float4 outlineColor;
float outlineWidth = 0.1f;
float colorBorder = 0.25f;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 tex = tex2D(uImage0, coords);
    float4 noise = tex2D(noiseTexture, (coords + noiseOffset) * uImageSize1);
    float4 color = tex;
    
    float opacity = saturate(noise.r - 1 + progress * 2);
    color *= opacity < colorBorder ? 0 : 1;
    
    float borderOpacity = abs(opacity - colorBorder) < outlineWidth ? 1 : 0;
    float4 outline = outlineColor * borderOpacity;
    color += outline * tex.a;

    return color * sampleColor;
}
technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}