# UnityShader_Learning
---
## 漫反射光照模型（Diffuse）

### 逐顶点

![逐顶点](RenderingPic/diffuse/DiffuseVertex.jpg)

### 逐像素

![逐像素](RenderingPic/diffuse/DiffusePixel.jpg)

从两张效果图很明显可以看出，逐顶点光照其实是出现了很多锯齿的，而逐像素光照的效果更为平滑。这是因为逐顶点光照是在顶点着色器中进行光照模型的计算，因此计算的次数和模型的顶点个数是一致的，而逐像素光照是在片元着色器中进行计算，是以每个像素为单位进行的计算。显然顶点数远远小于像素数，所以逐顶点的计算量会更小，性能消耗更低，当然对应得到的效果就要差一些。逐像素的计算则相反。

上述两种处理方式都符合**兰伯特定律（Lambert）**。在计算中，为了防止模型被后面来的光源照亮，可以通过判断法线和光源方向的**点乘（dot）**结果是否小于0得知，但这会导致背光部分没有任何明暗变化，使得背光区域像平面一样，失去了模型的细节，如图：

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

Blinn-Phong模型相对Phong模型来说更符合实际情况，但仍然不能真正的反应真实世界中的情况。Blinn-Phong模型的技术细节在此不做解释。
