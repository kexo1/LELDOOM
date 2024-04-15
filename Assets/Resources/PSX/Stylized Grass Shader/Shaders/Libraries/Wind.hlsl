//Stylized Grass Shader
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

//Properties
TEXTURE2D(_WindMap); SAMPLER(sampler_WindMap);
float4 _GlobalWindParams;
//X: Strength
//W: (int bool) Wind zone present
float _WindStrength;
float4 _GlobalWindDirection;

struct WindSettings
{
	float mask;
	float ambientStrength;
	float speed;
	float time;
	float3 direction;
	float swinging;

	float randObject;
	float randVertex;
	float randObjectStrength;

	float3 gustDirection;

	float gustStrength;
	float gustFrequency;
};

WindSettings PopulateWindSettings(in float strength, float speed, float4 direction, float swinging, float mask, float randObject, float randVertex, float randObjectStrength, float gustStrength, float gustFrequency)
{
	WindSettings s = (WindSettings)0;

	//Apply WindZone strength
	if (_GlobalWindParams.x > 0) 
	{
		strength *= _GlobalWindParams.y;
		gustStrength *= _GlobalWindParams.z;

		direction.xyz = _GlobalWindDirection.xyz;
		s.gustDirection = _GlobalWindDirection.xyz;
		s.time = _GlobalWindParams.x;
	}
	else
	{
		s.gustDirection = direction.xyz;
		s.time = _TimeParameters.x;
	}

	s.ambientStrength = strength;
	s.speed = speed;
	s.direction.xyz = direction.xyz;
	s.swinging = swinging;
	s.mask = mask;
	s.randObject = randObject;
	s.randVertex = randVertex;
	s.randObjectStrength = randObjectStrength;
	s.gustStrength = gustStrength;
	s.gustFrequency = gustFrequency;

	return s;
}

//World-align UV moving in wind direction
float2 GetGustingUV(float3 positionWS, in float speed, in float freq, in float3 dir)
{
	return (positionWS.xz * freq * 0.01) - (speed * freq * 0.01) * dir.xz;
}

//World-align UV moving in wind direction
float2 GetGustingUV(float3 positionWS, WindSettings s)
{
	return GetGustingUV(positionWS, s.time * s.speed, s.gustFrequency, s.gustDirection);
}

#if defined(SHADER_STAGE_VERTEX) || defined(SHADER_STAGE_DOMAIN)
#define SAMPLE_GUST_MAP(texName, sampler, uv) SAMPLE_TEXTURE2D_LOD(texName, sampler, uv, 0)
#else
#define SAMPLE_GUST_MAP(texName, sampler, uv) SAMPLE_TEXTURE2D(texName, sampler, uv)
#endif

float SampleGustMap(float3 positionWS, WindSettings s)
{
	float2 gustUV = GetGustingUV(positionWS, s);

	float gust = SAMPLE_GUST_MAP(_WindMap, sampler_WindMap, gustUV).r;

	gust *= s.gustStrength * s.mask;

	return gust;
}

float4 GetWindOffset(in float3 positionOS, in float3 positionWS, float rand, WindSettings s)
{
	float4 offset = 0;

#if !defined(DISABLE_WIND)
	//Random offset per vertex
	float f = length(positionOS.xz) * s.randVertex;
	float strength = s.ambientStrength * 0.5 * lerp(1, rand, s.randObjectStrength);
	
	//Combine
	float2 sine = sin(s.speed * (s.time + (rand * s.randObject) + f));
	//Remap from -1/1 to 0/1
	sine = lerp(sine * 0.5 + 0.5, sine, s.swinging);

	//Apply gusting
	float2 gust = SampleGustMap(positionWS, s).xx;

	//Scale sine
	sine = sine * s.mask * strength;

	//Mask by direction vector + gusting push
	offset.xz = (sine + gust) * s.direction.xz;
	offset.y = s.mask;

	//Summed offset strength
	float windWeight = length(offset.xz) + 0.0001;
	//Slightly negate the triangle-shape curve
	windWeight = pow(windWeight, 1.5);
	offset.y *= windWeight;

	//Wind strength in alpha
	offset.a = windWeight;
#endif

	return offset;
}

void GetWindOffset_float(in float3 positionOS, in float3 positionWS, float rand, in float strength, float speed, float3 direction, float swinging, float mask, float randObject, float randVertex, float randObjectStrength, float gustStrength, float gustFrequency, out float3 offset)
{
	WindSettings s = PopulateWindSettings(strength, speed, direction.xyzz, swinging, mask, randObject, randVertex, randObjectStrength, gustStrength, gustFrequency);

	offset = GetWindOffset(positionOS, positionWS, rand, s).xyz;

	//Negate component so the entire vector can just be additively applied
	offset.y = -offset.y;
}