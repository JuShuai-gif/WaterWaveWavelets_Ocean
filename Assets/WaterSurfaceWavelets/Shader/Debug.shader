Shader "Unlit/Debug"
{
	Properties{
			// 通用属性标记
			[Header(HEADER)]
			_MainTex0("Base (RGB)", 2D) = "white" {}
			// 二维纹理
			
			// 在材质面板中隐藏
			// [HideInInspector]

			// 设置纹理贴图的属性
			// Tilling(贴图重复度)
			// Offset(贴图偏移值)
			[NoScaleOffset]_MainTex1("2D Texture",2D) = "white" {}
			
			// 警示这个贴图必须是法线贴图
			[Normal]_MainTex2("法线贴图",2D) = "white" {}
			// 2D纹理默认值：white:纯白色   black:纯黑色  gray:灰色图  bump:法线图

			_MainTex3("3D纹理",3d) = ""{}

			// CubeMap 通常被用来作为具有反射属性物体的反射源
			_MainTex4("Cube Tex",CUBE) = ""{}

			// 普通的颜色值 
			_Color0("我是Color",Color) = (1,1,1,1)
			
			// 比普通的颜色值，多一个强度
			[HDR]_Color("我是Color",Color) = (1,1,1,1)
			
			// 整型值
			_Int("我是Int",Int) = 1
			// 浮点值
			_Float0("我是Float",Float) = 0.5
			// 浮点滑动条
			_Float1("我是Float",Range(0,1)) = 0.5
			// 带曲率的浮点滑动条
			[PowerSlider(3)]_Float2("PowerSlider Float",Range(0,1)) = 0.5
			// 整型滑动条
			[IntRange]_Float3("IntRange Float",Range(0,1)) = 1
			// 开关
			[Toggle]_Float("Toggle Float",Range(0,1)) = 1
			// 枚举
			[Enum(UnityEngine.Rendering.CullMode)]_Float("Enum",Float) = 1
			// 向量
			_Vector("Vector",Vector) = (0,0,0,0)
	}
	SubShader{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			fixed4 _Color;
			
			// 顶点着色器一个三角形只执行三次
			// 片段着色器一个三角形可能执行几百甚至上千次，这个三角形所占像素数有关
			
			struct appdata {
				float4 vertex:POSITION;
				float2 uv:TEXCOORD;
			};

			struct v2f {
				float4 pos:SV_POSITION;
				float2 uv:TEXCOORD;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 checker(float2 uv) 
			{
				float2 repeatUV = uv * 10;
				float2 c = floor(repeatUV) / 2;
				float checker = frac(c.x + c.y) * 2;
				return float4(checker,checker, checker, checker);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//return fixed4(i.uv,0,1);
				//return _Color;
				fixed4 col = checker(i.uv);
			return col;
			}
			ENDCG
		}
	}
}
