struct VS_IN {

	float4 pos : POSITION;
	float4 col : COLOR;
	float4 tex : TEXCOORD0;
};

struct PS_IN {

	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float4 tex : TEXCOORD0;
	float4 posNM : TEXCOORD1;
};


float4x4 worldViewProj;
Texture2D Tex2 : register(t0);
sampler Sampler : register(s0);

sampler2D tester : register(s0);


float4 BlurTest(float2 uv : TEXCOORD0, float2 dir, float4 col : COLOR) : COLOR0 {
	
	float offset[] = {
		0.0, 1.0, 2.0, 3.0, 4.0
	};

	float weight[] = {
		0.2270270270, 0.1945945946, 0.1216216216,
		0.0540540541, 0.0162162162	
	};

	float4 ppColour = col;
	ppColour += Tex2.Sample(Sampler, uv) * weight[0];

	float4 FragmentColor = Tex2.Sample(Sampler, uv / 1024) * weight[0];

	float hstep = dir.x;
	float vstep = dir.y;

	for (int i = 1; i < 5; i++) {

		FragmentColor +=
			Tex2.Sample(Sampler, uv.xy + float2(hstep, offset[i]) / 1024) * weight[i];

		FragmentColor +=
			Tex2.Sample(Sampler, uv.xy - float2(vstep, offset[i]) / 1024) * weight[i];			
	}

	ppColour += FragmentColor;
	
	return ppColour;
}

float4 GaussianBlur(float2 uv : TEXCOORD0, float4 col : COLOR) : SV_Target
{
	float4 Color = col;
	Color.rgb = 1.0f;

	if ((uv.x <= 0.05f || uv.x >= 0.95f) && uv.y >= 0.0f)
	{
		Color.a = 1.0f - uv.y;
	}
	if ((uv.y <= 0.05f || uv.y >= 0.95f) && uv.x >= 0.0f)
	{
		Color.a = 1.0f - uv.x;
	}

	return Color;
}



PS_IN VS(VS_IN input) {

	PS_IN output = (PS_IN)0;

	output.pos = input.pos;
	output.tex = input.tex;
	output.col = input.col;
	output.posNM = input.pos;

	output.pos = mul(input.pos, worldViewProj);

	return output;
}


float4 PS(PS_IN input) : SV_Target{

	//Horizontal pass
	input.col = BlurTest(input.tex, float2(1.0f, 0.0f), input.col);
	//input.col = GaussianBlur(input.tex, input.col);

	//Vertical pass
	input.col = BlurTest(input.tex, float2(0.0f, 1.0f), input.col);

	return input.col;
}

