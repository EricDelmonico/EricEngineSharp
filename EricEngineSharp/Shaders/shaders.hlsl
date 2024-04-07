/* vertex attributes go here to input to the vertex shader */
struct VsInput
{
    float3 position : POSITION;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float2 uv : TEXCOORD;
};

/* outputs from vertex shader go here. can be interpolated to pixel shader */
struct VsOutput
{
    float4 screenPosition : SV_POSITION; // required output of VS
    float2 uv : TEXCOORD;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float3 worldPosition : POSITION;
    float4 vertexColor : COLOR;
};

cbuffer ExternalData
{
    matrix model;
    matrix modelInvTranspose;
    matrix view;
    matrix projection;
};

VsOutput vs_main(VsInput input)
{
    VsOutput output = (VsOutput)0;
    
    matrix mvp = mul(projection, mul(view, model));
    output.screenPosition = mul(mvp, float4(input.position, 1.0f));
    
    output.worldPosition = mul(model, float4(input.position, 1.0f));
    
    output.normal = mul((float3x3) modelInvTranspose, input.normal);
    output.tangent = mul((float3x3) modelInvTranspose, input.tangent);
    
    output.uv = input.uv;
    
    output.vertexColor = float4(0.5 * (input.position + float3(1, 1, 1)), 1);

    return output;
}

float4 ps_main(VsOutput input) : SV_TARGET
{
    return input.vertexColor;
}