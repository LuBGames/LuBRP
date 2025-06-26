Varyings vert(Attributes input)
{
    VertexDescriptionInputs inputs = BuildVertexDescriptionInputs(input);
    VertexDescription description = VertexDescriptionFunction(inputs);
    
    Varyings output = PackVaryings(input, description);
    return output;
}

real4 frag(Varyings i) : SV_TARGET
{
    SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(i);
    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);

    #if _ALPHATEST_ON
    half alpha = surfaceDescription.Alpha;
    clip(alpha - surfaceDescription.AlphaClipThreshold);
    #elif _SURFACE_TYPE_TRANSPARENT
    half alpha = surfaceDescription.Alpha;
    #else
    half alpha = 1;
    #endif
    
    #ifdef _ALPHAPREMULTIPLY_ON
    surfaceDescription.BaseColor *= alpha;
    #endif

    Light light = GetMainLight();
    
    real shading = saturate(dot(light.direction, i.normalWS));
    #ifdef LUB_SHADOWS_INCLUDED
    shading = min(GetDirectionalShadowAttenuation(i), shading);
    #endif
    real3 diffColor = shading * surfaceDescription.BaseColor * light.color;

    half3 sh = SampleSH(i.normalWS);
    real3 ambientTerm = sh * surfaceDescription.BaseColor;

    real3 finishColor = diffColor + ambientTerm;

    return real4(finishColor, alpha);
}
