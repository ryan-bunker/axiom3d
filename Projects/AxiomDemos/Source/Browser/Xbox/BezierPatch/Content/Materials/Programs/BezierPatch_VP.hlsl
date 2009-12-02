/***************************************************************************************
 * This shader was generated by a tool; any changes made to this file may be lost
 * Axiom Fixed Function Emulation Layer
 * Vertex Program Entry Point : VS
 * Fragment Program Entry Point : FP
 * Vertex Buffer Delcaration : Float3 Position0;
Float3 Normal0;
Float2 TexCoords0;

 * Fixed Function State : Axiom.RenderSystems.Xna.FixedFunctionEmulation.FixedFunctionState
 ***************************************************************************************/
struct VS_INPUT
{
	float4 Position0 : POSITION0;
	float3 Normal0 : NORMAL0;
	float2 Texcoord0 : TEXCOORD0;
};

float4x4  World;
float4x4  View;
float4x4  Projection;
float4x4  ViewIT;
float4x4  WorldViewIT;
shared float4x4  TextureMatrix0; //technique: None
float4 BaseLightAmbient;
float4 Light0_Ambient;
float4 Light0_Diffuse;
float4 Light0_Specular;
float3 Light0_Direction;
float4 MaterialAmbient=float4(1,1,1,1);
float4 MaterialDiffuse=float4(1,1,1,1);
float4 MaterialSpecular=(float4)0;

struct VS_OUTPUT
{
	float4 Pos : POSITION;
	float2 Texcoord0 : TEXCOORD0;
	float4 Color: COLOR0;
	float4 ColorSpec: COLOR1;
};

VS_OUTPUT main_vp( VS_INPUT input )
{
	VS_OUTPUT output = (VS_OUTPUT)0;
	float4 worldPos = mul( World, input.Position0);
	float4 cameraPos = mul( View, worldPos );
	output.Pos = mul( Projection, cameraPos );
	ViewIT = transpose(ViewIT);
	WorldViewIT = transpose(WorldViewIT);
	float3 Normal = input.Normal0;
	//debug info : texture stage 0 coordindex: 0
	{
		float4 texCordWithMatrix = float4(input.Texcoord0, 1, 1);
		texCordWithMatrix = mul(texCordWithMatrix, TextureMatrix0 );
		output.Texcoord0 = texCordWithMatrix.xy;
	}
	output.ColorSpec =MaterialSpecular;
	output.Color +=BaseLightAmbient;
	output.Color *=MaterialDiffuse;
	float3 N = mul((float3x3)WorldViewIT, Normal);
	float3 V = -normalize(cameraPos);
	#define fMaterialPower 16.f
{
  float3 L = mul((float3x3)ViewIT, -normalize(Light0_Direction));
  float NdotL = dot(N, L);
  float4 Color = Light0_Ambient;
  float4 ColorSpec = 0;
  if(NdotL > 0.f)
  {
    //compute diffuse color
    Color += NdotL * Light0_Diffuse;
    //add specular component
    float3 H = normalize(L + V);   //half vector
    ColorSpec = pow(max(0, dot(H, N)), fMaterialPower) * Light0_Specular;
    output.Color += Color;
    output.ColorSpec += ColorSpec;
  }
}
	return output;
}
