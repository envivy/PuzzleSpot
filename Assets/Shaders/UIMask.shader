Shader "Custom/UIMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskColor ("Mask Color", Color) = (0,0,0,1) // 改为黑色
        _MaskShape ("Mask Shape", Float) = 0.0 // 0 for rectangle, 1 for circle  
        _Center ("Center", Vector) = (1250, 1250, 0, 0)  
        _Radius ("Radius", Range(0, 2500)) = 1250
        _Smoothness ("Smoothness", Range(0, 0.1)) = 0.03  
        _HoleSize("HoleSize", Vector) = (1250, 1250, 0, 0)
        _Transparency ("Transparency", Range(0,1)) = 0.9  
    }
    
    SubShader
    {
        // No culling or depth  
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]  
        Blend SrcAlpha OneMinusSrcAlpha 
  
        Pass  
        {  
            CGPROGRAM  
            #pragma vertex vert  
            #pragma fragment frag  
              
            #include "UnityCG.cginc"  
  
            struct appdata  
            {  
                float4 vertex : POSITION;  
                float2 uv : TEXCOORD0;  
                fixed4 color : COLOR;  
            };  
  
            struct v2f  
            {  
                float2 uv : TEXCOORD0;  
                float4 vertex : SV_POSITION;  
                fixed4 color : COLOR;  
            };  
  
            sampler2D _MainTex;  
            float4 _MainTex_ST;
            fixed4 _MaskColor;
            float _MaskShape; 
            float2 _Center;  
            float _Radius;           
            float _Smoothness;           
            float2 _HoleSize;  
            float _Transparency;  
  
            v2f vert (appdata v)  
            {  
                v2f o;  
                o.vertex = UnityObjectToClipPos(v.vertex);  
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);  
                o.color = v.color;  
                return o;     
            }  
  
            fixed4 frag (v2f i) : SV_Target  
            {  

                float2 uv = i.uv;
                fixed4 texCol = tex2D(_MainTex, uv);
                fixed4 col = _MaskColor; // 使用定义的遮罩颜色
                col.a = texCol.a * _Transparency; // 保持原有的透明度控制
                
                // Check if the pixel should be masked based on the shape  
                bool isMasked = false;  

                col.a *= _Transparency; // 调整透明度 
                             
                if (_MaskShape == 0.0) // Rectangle mask  
                {  
                    float2 normalizedCenter = _Center * 0.0004 + 0.5;  
                    float2 normalizedHoleSize = _HoleSize * 0.0004;
                   // 计算挖孔矩形的UV坐标  
                    float2 holeUVMin = float2(normalizedCenter.x - normalizedHoleSize.x / 2, normalizedCenter.y - normalizedHoleSize.y / 2);  
                    float2 holeUVMax = float2(normalizedCenter.x + normalizedHoleSize.x / 2, normalizedCenter.y + normalizedHoleSize.y / 2);  
                  
                    // 检查当前片元是否在挖孔矩形内  
                    if (i.uv.x >= holeUVMin.x && i.uv.x <= holeUVMax.x && i.uv.y >= holeUVMin.y && i.uv.y <= holeUVMax.y)  
                    {  
                        // 如果在挖孔内，则设置为透明或其他颜色  
                        col.a = 0; 
                    }
                }  
                else if (_MaskShape == 1.0) // Circle mask  
                {
                    float2 normalizedCenter = _Center * 0.0004 + 0.5;                                
                    float normalizedRadius = _Radius * 0.0004;  
                    float2 toCenter = uv - normalizedCenter;  
                    float dist = length(toCenter);  
                    // Calculate the alpha value without anti-aliasing  
                    float edge = smoothstep(normalizedRadius - _Smoothness, normalizedRadius + _Smoothness, dist);                                                            
                    col.a *= edge;  
                }
                return col;
            }  
            ENDCG 
        }
    }
    FallBack "Diffuse"
}
