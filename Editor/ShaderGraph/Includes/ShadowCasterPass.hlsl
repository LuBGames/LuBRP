#ifndef SG_SHADOW_PASS_INCLUDED
#define SG_SHADOW_PASS_INCLUDED

Varyings vert(Attributes input)
{
    VertexDescriptionInputs inputs = BuildVertexDescriptionInputs(input);
    VertexDescription description = VertexDescriptionFunction(inputs);
    
    Varyings output = PackVaryings(input, description);
    return output;
}

half4 frag(Varyings i) : SV_TARGET
{
    SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(i);
    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);

    #if defined(_ALPHATEST_ON)
        clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
    #endif

    return 0;
}

#endif
