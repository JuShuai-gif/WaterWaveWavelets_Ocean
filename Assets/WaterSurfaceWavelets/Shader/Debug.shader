Shader "Unlit/Debug"
{
	Properties{
			// ͨ�����Ա��
			[Header(HEADER)]
			_MainTex0("Base (RGB)", 2D) = "white" {}
			// ��ά����
			
			// �ڲ������������
			// [HideInInspector]

			// ����������ͼ������
			// Tilling(��ͼ�ظ���)
			// Offset(��ͼƫ��ֵ)
			[NoScaleOffset]_MainTex1("2D Texture",2D) = "white" {}
			
			// ��ʾ�����ͼ�����Ƿ�����ͼ
			[Normal]_MainTex2("������ͼ",2D) = "white" {}
			// 2D����Ĭ��ֵ��white:����ɫ   black:����ɫ  gray:��ɫͼ  bump:����ͼ

			_MainTex3("3D����",3d) = ""{}

			// CubeMap ͨ����������Ϊ���з�����������ķ���Դ
			_MainTex4("Cube Tex",CUBE) = ""{}

			// ��ͨ����ɫֵ 
			_Color0("����Color",Color) = (1,1,1,1)
			
			// ����ͨ����ɫֵ����һ��ǿ��
			[HDR]_Color("����Color",Color) = (1,1,1,1)
			
			// ����ֵ
			_Int("����Int",Int) = 1
			// ����ֵ
			_Float0("����Float",Float) = 0.5
			// ���㻬����
			_Float1("����Float",Range(0,1)) = 0.5
			// �����ʵĸ��㻬����
			[PowerSlider(3)]_Float2("PowerSlider Float",Range(0,1)) = 0.5
			// ���ͻ�����
			[IntRange]_Float3("IntRange Float",Range(0,1)) = 1
			// ����
			[Toggle]_Float("Toggle Float",Range(0,1)) = 1
			// ö��
			[Enum(UnityEngine.Rendering.CullMode)]_Float("Enum",Float) = 1
			// ����
			_Vector("Vector",Vector) = (0,0,0,0)
	}
	SubShader{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			fixed4 _Color;
			
			// ������ɫ��һ��������ִֻ������
			// Ƭ����ɫ��һ�������ο���ִ�м���������ǧ�Σ������������ռ�������й�
			
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
