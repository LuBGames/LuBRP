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

    return half4(surfaceDescription.BaseColor, alpha);
}
