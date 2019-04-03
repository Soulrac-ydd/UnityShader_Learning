# UnityShader_Learning
---
## 漫反射光照模型（Diffuse）

### 逐顶点

![逐顶点](RenderingPic/diffuse/DiffuseVertex.jpg)

### 逐像素

![逐像素](RenderingPic/diffuse/DiffusePixel.jpg)

从两张效果图很明显可以看出，逐顶点光照其实是出现了很多锯齿的，而逐像素光照的效果更为平滑。这是因为逐顶点光照是在顶点着色器中进行光照模型的计算，因此计算的次数和模型的顶点个数是一致的，而逐像素光照是在片元着色器中进行计算，是以每个像素为单位进行的计算。显然顶点数远远小于像素数，所以逐顶点的计算量会更小，性能消耗更低，当然对应得到的效果就要差一些。逐像素的计算则相反。

上述两种处理方式都符合**兰伯特定律（Lambert）**。在计算中，为了防止模型被后面来的光源照亮，可以通过判断法线和光源方向的**点乘（Dot）** 结果是否小于0得知，但这会导致背光部分没有任何明暗变化，使得背光区域像平面一样，失去了模型的细节，如图：

### 逐顶点

![逐顶点](RenderingPic/diffuse/DiffuseVertexBack.jpg)

### 逐像素

![逐像素](RenderingPic/diffuse/DiffusePixelBack.jpg)

为了改进上述缺点，可以采用**半兰伯特光照模型（Half Lambert）**：

### 半兰伯特光照模型

![HalfLambert](RenderingPic/diffuse/HalfLambert.jpg)

![HalfLambert](RenderingPic/diffuse/HalfLambertBack.jpg)

半兰伯特的技术细节在此不作说明，但可以知道的是，半兰伯特是在原兰伯特光照模型的基础上进行的修改，是不具有任何物理依据的，其仅仅是一个trick。

---

## 高光/镜面反射光照模型（Specular）

### 逐顶点

![逐顶点](RenderingPic/specular/SpecularVertex.jpg)

### 逐像素

![逐像素](RenderingPic/specular/SpecularPixel.jpg)

从效果图中可以明显看出，逐顶点的方式中高光部分是不平滑的，这是因为在顶点着色器中对每个顶点计算光照时，会对各个顶点进行**线性插值**以得到像素光照，而**高光反射计算部分是非线性的**，因此会出现视觉问题。逐像素的方式则可以得到更加平滑的高光效果。

上述两种方式以**Phong光照模型**来进行高光反射的计算，下面展示一种在Phong模型的基础上简单修改的模型：

### Blinn-Phong光照模型

![Blinn-Phong](RenderingPic/specular/BlinnPhong.jpg)

可以看出，Blinn-Phong光照模型的高光部分更大，更亮一些。

Blinn-Phong模型相对Phong模型来说更符合实际情况，但仍然不能真正的反应真实世界中的情况。Blinn-Phong模型的技术细节在此不做解释。

## 凹凸映射（bump mapping）中的法线贴图（normal map）

最近初学纹理相关部分的知识，有许多概念理解不是很透彻，查找多方资料后自己对一些概念有了一定的初步理解，在此记录下来，方便日后回顾与更正。

### 基本概念————空间变换

> 如果需要知道A空间到B空间的变换矩阵，只需用B中A的三个坐标轴的方向、按X、Y、Z轴的顺序**按列排序**构建一个矩阵即可得到。

### 法线贴图中存的是什么且如何存储

> 顾名思义，既然是贴图，那存的就是含有ARGB通道的图，但这不是一般的图像，一般来说，这个图像的RGB分量存的是切线空间下对应模型顶点各个法线的方向。当然法线是不能直接存储在贴图中的，所以这里就需要一个映射：
>> ![](MathFormula/1.png)
>
> 因此换句话说，法线贴图中存的就是把法线经过映射后得到的像素值，一般也说是**纹素（texel）**值，即纹理的基本单元。
>
> 前面提到，一般情况下，法线贴图的RGB分量对应的是切线空间下法线方向xyz值，但在Unity中，Unity会对法线贴图的存储进行优化，同时根据平台的不同选择不同的 优化方式，当然这里的前提是把Texture Type选择为Normal Type。
>
> 当我们对法线纹理进行采样后，此时我们采样得到的结果是已经被Unity优化过后的结果，这个时候就需要我们对结果进行**UnpackNormal**操作以获得真正的纹素值，具体的UnpackNormal函数内部实现如下：
>
```shaderlab
inline fixed3 UnpackNormalDXT5nm (fixed4 packednormal)
{
    fixed3 normal;
    normal.xy = packednormal.wy * 2 - 1;
    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
    return normal;
}
// Unpack normal as DXT5nm (1, y, 1, x) or BC5 (x, y, 0, 1)
// Note neutral texture like "bump" is (0, 0, 1, 1) to work with both plain RGB normal and DXT5nm/BC5
fixed3 UnpackNormalmapRGorAG(fixed4 packednormal)
{
    // This do the trick
   packednormal.x *= packednormal.w;

    fixed3 normal;
    normal.xy = packednormal.xy * 2 - 1;
    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
    return normal;
}
inline fixed3 UnpackNormal(fixed4 packednormal)
{
#if defined(UNITY_NO_DXT5nm)
    return packednormal.xyz * 2 - 1;
#else
    return UnpackNormalmapRGorAG(packednormal);
#endif
}
```
> 可以看出Unity对法线纹理优化后，贴图的各纹素RBG分量就不对应法线的xyz分量了，因此我们就需要解码。以编码格式DXT5nm为例，纹素的的A通道（即w分量）对应法线的x分量，G通道（即y分量）对应法线的y分量，而R和B通道则被舍弃，然后对结果进行一次反映射就能得到原先法线的xy分量。
> 由于法线是单位向量，我们便可以通过法线的xy分量值的计算的到z分量值，因此我们可以仅存储xy方向从而推导得到z方向，这就进一步缩小了法线纹理所占内存。
