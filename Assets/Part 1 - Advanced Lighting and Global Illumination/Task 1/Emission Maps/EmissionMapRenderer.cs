using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* This class implements a emission mapping into our forward
 * renderer code. 
 * 
 * Your job will be to implement the missing chunks of code
 * required to make the emission mapping work. You should take
 * some time to look over the code to try and understand it
 * before you do this.
 *
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class EmissionMapRenderer : MonoBehaviour
{
    // Reference to the camera which our scene will be rendered from
    public Camera renderingCamera;

    // The Raw Image UI Element which we will place our frame buffer texture in
    public RawImage frameBufferTextureUIContainer;

    // The size of the rendered scene texture we will create
    public Vector2Int renderedTextureSize = new Vector2Int(300, 300);

    // We will store our frame buffer in an array of colours
    Color[] frameBuffer;

    // We will store our depth buffer in an array of floats
    float[] depthBuffer;

    // The frame buffer Texture2D
    Texture2D frameBufferTexture;

    // Set the clear colour to blue. Alternately we can clear it
    // with an alpha of 0 to make the background transparent
    public Color TextureClearColour = Color.blue;
    //public Color TextureClearColour = new Color(1, 1, 1, 0);

    // The light sources in the scene
    public Light[] lightSources;

    // Start is called before the first frame update
    void Start()
	{
        // Create our frame buffer and texture
        frameBuffer = new Color[renderedTextureSize.x * renderedTextureSize.y];
        frameBufferTexture = new Texture2D(renderedTextureSize.x, renderedTextureSize.y);

        // Set the UI container's texture to be our frame buffer texture
        frameBufferTextureUIContainer.texture = frameBufferTexture;

        // Create our depth buffer
        depthBuffer = new float[renderedTextureSize.x * renderedTextureSize.y];
    }

	// Update is called once per frame
	void LateUpdate()
	{
        // Clear our rendered texture
        ClearFrameAndDepthBuffer(TextureClearColour, float.MaxValue);

        // Get a list of meshes in the scene to render
        MeshFilter[] meshFiltersToRender = FindObjectsOfType<MeshFilter>();
        
        
        foreach (MeshFilter meshFilterToRender in meshFiltersToRender)
        {
            // Store a copy of the original mesh vertices from the meshFilterToRender
            Vector3[] originalMeshVertices = meshFilterToRender.sharedMesh.vertices;

            // Store a copy of the original mesh vertex normals from the meshFilterToRender
            Vector3[] originalMeshNormals = meshFilterToRender.sharedMesh.normals;

            // Store a copy of the original mesh triangles from the meshFilterToRender
            int[] originalMeshTriangles = meshFilterToRender.sharedMesh.triangles;

            // Store a copy of the original mesh UVs from the meshFilterToRender
            Vector2[] originalMeshUVs = meshFilterToRender.sharedMesh.uv;

            // Get the material to use for our texture and emission mapping,
            // include the values for the diffuse colour, specular colour and shininess,
            // as well as the material's diffuse and emission textures and emission
            // colour
            Material material = meshFilterToRender.GetComponent<Renderer>().sharedMaterial;
            Color Material_DiffuseColor = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
            Color Material_SpecularColor = material.HasProperty("_SpecColor") ? material.GetColor("_SpecColor") : Color.white;
            float Material_Shininess = material.HasProperty("_Glossiness") ? material.GetFloat("_Glossiness") + 2 : 1;
            Texture2D Material_DiffuseTexture = material.GetTexture("_MainTex") as Texture2D;
            Texture2D Material_EmissionMap = material.GetTexture("_EmissionMap") as Texture2D;
            Color Material_EmissionColour = material.GetColor("_EmissionColor");

            // Calculate the model to world matrix for our mesh
            Matrix4x4 modelToWorldMatrix = CalculateModelToWorldMatrix(meshFilterToRender.transform);

            // Calculate the model to view matrix for our mesh based on the camera
            Matrix4x4 modelToViewMatrix = CalculateModelToViewMatrix(modelToWorldMatrix, renderingCamera);

            // Calculate the perspective projection matrix for our camera
            Matrix4x4 projectionMatrix = CalculatePerspectiveProjectionMatrix(renderingCamera);

            // Project our mesh vertices from model space to world space
            Vector3[] worldVertices = TransformModelVertsToWorldVerts(originalMeshVertices, modelToWorldMatrix);

            // Project our mesh normals from model space to world space
            Vector3[] worldNormals = TransformModelNormalsToWorldNormals(originalMeshNormals, modelToWorldMatrix);

            // Project our mesh vertices from model space to homogeneous clip space
            Vector3[] projectedVertices = ProjectModelSpaceVertices(originalMeshVertices, modelToViewMatrix, projectionMatrix);

            // Render our geometry
            RenderGeometry(worldVertices, projectedVertices, worldNormals, originalMeshUVs, originalMeshTriangles,
                Material_DiffuseColor, Material_SpecularColor, Material_Shininess,
                Material_DiffuseTexture, Material_EmissionMap, Material_EmissionColour,
                renderingCamera.transform.position);

        }

        // Copy the buffers into the textures
        CopyFrameBufferToTexture();
    }

    // This function clears our rendered texture
    void ClearFrameAndDepthBuffer(Color clearColour, float depthClearValue)
    {
        // Loop through each frame buffer pixel and set it's color to the clear colour
        for (int i = 0; i < frameBuffer.Length; i++) frameBuffer[i] = clearColour;

        // Loop through each depth buffer pixel and set it's value to the clear value
        for (int i = 0; i < depthBuffer.Length; i++) depthBuffer[i] = depthClearValue;
    }

    // This function copies our frame buffer to the texture to display
    void CopyFrameBufferToTexture()
    {
        // The frame buffer can just be copied directly to the texture
        frameBufferTexture.SetPixels(frameBuffer);
        frameBufferTexture.Apply();
    }

    // This function calculates the model to world matrix for a transform
    Matrix4x4 CalculateModelToWorldMatrix(Transform initialTransform)
    {

        // This is where we will store the model to world matrix as we traverse
        // the hierarchy. Initialise it to the identity matrix
        Matrix4x4 modelToWorldMatrix = Matrix4x4.identity;

        // Start at the meshFilterToRenders transform
        Transform currentTransform = initialTransform;

        // As long as the current transform isn't null
        while (currentTransform != null)
        {
            // Multiply the existing modelToWorldMatrix by the current transform's
            // local position, local rotation and local scale (stored in matrix form)
            modelToWorldMatrix = Matrix4x4.TRS(currentTransform.localPosition, currentTransform.localRotation, currentTransform.localScale) * modelToWorldMatrix;

            // Update the current transform to it's parent - once we reach
            // root node, this will set current transform to null, exiting out
            // of our loop
            currentTransform = currentTransform.parent;
        }

        // return the calculated matrix
        return modelToWorldMatrix;
    }


    // This function calculates the Model to View Matrix (or ModelViewMatrix)
    Matrix4x4 CalculateModelToViewMatrix(Matrix4x4 modelToWorldMatrix, Camera camera)
    {
        // Note that camera space matches OpenGL convention: camera's forward
        // is the negative Z axis. This is different from Unity's convention,
        // where forward is the positive Z axis.
        Matrix4x4 unityWorldToCameraMatrix = Matrix4x4.Scale(new Vector3(1, 1, 1))
            * camera.worldToCameraMatrix;

        // The model view matrix is just the World to Camera matrix multiplied
        // by the model to world matrix
        return unityWorldToCameraMatrix * modelToWorldMatrix;
    }


    // This function calculates the perspective projection matrix for a camera
    Matrix4x4 CalculatePerspectiveProjectionMatrix(Camera camera)
    {
        // initialize our projection matrix to all zeros
        Matrix4x4 projectionMatrix = Matrix4x4.zero;

        // Get the camera vertical field of view in radians
        float rad_fovY = camera.fieldOfView * Mathf.Deg2Rad;

        // Manually calculate our projection matrix using the values:
        // a = vertical field of view, ar = aspect ratio
        // nz = near plane, fz = far plane

        // M_0,0 = 1 / ( ar * tan (a / 2) )
        projectionMatrix.m00 = 1f / (camera.aspect * Mathf.Tan(rad_fovY / 2));
        // M_1,1 = 1 / ( tan (a / 2) )
        projectionMatrix.m11 = 1f / (Mathf.Tan(rad_fovY / 2));
        // M_2,2 = -( (-nz - fz) / (nz - fz) )
        projectionMatrix.m22 = -((-camera.nearClipPlane - camera.farClipPlane) / (camera.nearClipPlane - camera.farClipPlane));
        // M_2,3 = (2 * fz * nz) / (nz - fz)
        projectionMatrix.m23 = (2 * camera.farClipPlane * camera.nearClipPlane) / (camera.nearClipPlane - camera.farClipPlane);
        // M_3,2 = -1
        projectionMatrix.m32 = -1;

        // Return the calculated projection matrix
        return projectionMatrix;
    }

    // This function transforms our model vertices into world space
    Vector3[] TransformModelVertsToWorldVerts(Vector3[] modelVerts, Matrix4x4 modelToWorldMatrix)
    {
        // Allocate space to store our world space vectors
        Vector3[] worldVerts = new Vector3[modelVerts.Length];
        // Loop through each vertex
        for (int i = 0; i < worldVerts.Length; i++)
            // Convert from model space to world space 
            worldVerts[i] = modelToWorldMatrix.MultiplyPoint3x4(modelVerts[i]);

        // return the world space vectors
        return worldVerts;
    }

    // This function transforms our model vertices into world space
    Vector3[] TransformModelNormalsToWorldNormals(Vector3[] modelNormals, Matrix4x4 modelToWorldMatrix)
    {
        // Allocate space to store our world space vectors
        Vector3[] worldNormals = new Vector3[modelNormals.Length];
        // Loop through each vertex
        for (int i = 0; i < worldNormals.Length; i++)
            // Convert from model space to world space 
            worldNormals[i] = modelToWorldMatrix.MultiplyVector(modelNormals[i]);

        // return the world space vectors
        return worldNormals;
    }

    // This function projects vertices in model space into homogeneous clip space
    Vector3[] ProjectModelSpaceVertices(Vector3[] modelSpaceVertices, Matrix4x4 modelToViewMatrix, Matrix4x4 projectionMatrix)
    {

        // Calculate the model to homogeneous clip space matrix
        // by multiplying the projection matrix by the model to view matrix
        Matrix4x4 modelToHomogeneousMatrix = projectionMatrix * modelToViewMatrix;

        // Create an array to store our projected vertices
        Vector3[] projectedVertices = new Vector3[modelSpaceVertices.Length];
        // Loop through each vertex
        for (int i = 0; i < modelSpaceVertices.Length; i++)
        {
            // Convert it from a 3D vertex to a 4D homogenous vertex (with w = 1)
            Vector4 homogeneousVertex = new Vector4(modelSpaceVertices[i].x, modelSpaceVertices[i].y, modelSpaceVertices[i].z, 1);

            // Project the 4D vertex to Homogeneous Space
            Vector4 projectedHomogeneousVertex = modelToHomogeneousMatrix * homogeneousVertex;

            // Convert the projected vertex from 4D to 3D by dividing the
            // x, y and z values by the w value
            projectedVertices[i] = new Vector3(projectedHomogeneousVertex.x / projectedHomogeneousVertex.w,
                projectedHomogeneousVertex.y / projectedHomogeneousVertex.w,
                projectedHomogeneousVertex.z / projectedHomogeneousVertex.w);
        }

        // Return the array of projected vertices
        return projectedVertices;
    }

    // This function renders out projected vertices as a point cloud using a specific colour
    void RenderGeometry(Vector3[] worldSpaceVertices, Vector3[] projectedVertices,
        Vector3[] worldSpaceNormals, Vector2[] uvs, int[] meshTriangles,
        Color material_DiffuseColour, Color material_SpecularColour, float material_Shininess,
        Texture2D material_DiffuseTexture, Texture2D Material_EmissionMap, Color Material_EmissionColour,
        Vector3 cameraWorldPosition
        )
    {
        // Loop through each of the triplets of triangle indices in our mesh
        for (int i = 0; i < meshTriangles.Length; i += 3)
        {
            // Get the projected homogeneous clip space positions for the
            // vertices in this triangle
            Vector3 proj_v1 = projectedVertices[meshTriangles[i]];
            Vector3 proj_v2 = projectedVertices[meshTriangles[i + 1]];
            Vector3 proj_v3 = projectedVertices[meshTriangles[i + 2]];

            // If at least one of the vertices is in the homogeneous clip bounds
            // Render the triangle
            if ((proj_v1.x >= -1 && proj_v1.x <= 1 && proj_v1.y >= -1 && proj_v1.y <= 1) ||
                (proj_v2.x >= -1 && proj_v2.x <= 1 && proj_v2.y >= -1 && proj_v2.y <= 1) ||
                (proj_v3.x >= -1 && proj_v3.x <= 1 && proj_v3.y >= -1 && proj_v3.y <= 1))
            {

                // Calculate the normal of the projected triangle from the cross
                // product of two of its edges
                Vector3 proj_triangleNormal = Vector3.Cross(proj_v2 - proj_v1, proj_v3 - proj_v1);

                // Calculate the centre of the projected triangle
                Vector3 proj_triangleCentre = (proj_v1 + proj_v2 + proj_v3) / 3;

                // Check the dot project of the projected triangle normal and
                // the camera to triangle centre vector - if the dot product is
                // <=0, the normal and vector point at each other, and the triangle
                // must be facing the camera, so we should render it. If the dot
                // product is >0, the are facing the same direction, therefore
                // the triangle is facing away from the camera - don't render it
                if (Vector3.Dot(proj_triangleNormal, proj_triangleCentre - cameraWorldPosition) <= 0)
                {
                    // Get the world space positions for the vertices in this triangle
                    Vector3 w_v1 = worldSpaceVertices[meshTriangles[i]];
                    Vector3 w_v2 = worldSpaceVertices[meshTriangles[i + 1]];
                    Vector3 w_v3 = worldSpaceVertices[meshTriangles[i + 2]];

                    // Get the world space normals for the vertices in this triangle
                    Vector3 w_n1 = worldSpaceNormals[meshTriangles[i]];
                    Vector3 w_n2 = worldSpaceNormals[meshTriangles[i + 1]];
                    Vector3 w_n3 = worldSpaceNormals[meshTriangles[i + 2]];

                    // Get the UVs for the vertices in this triangle
                    Vector2 uv1 = uvs[meshTriangles[i]];
                    Vector2 uv2 = uvs[meshTriangles[i + 1]];
                    Vector2 uv3 = uvs[meshTriangles[i + 2]];

                    // Normalize our projected vertices so that they are in the range
                    // Between 0 and 1 (instead of -1 and 1)
                    Vector2 normalized_v1 = new Vector2((proj_v1.x + 1) / 2f, (proj_v1.y + 1) / 2f);
                    Vector2 normalized_v2 = new Vector2((proj_v2.x + 1) / 2f, (proj_v2.y + 1) / 2f);
                    Vector2 normalized_v3 = new Vector2((proj_v3.x + 1) / 2f, (proj_v3.y + 1) / 2f);

                    // Multiply our normalized vertex positions by the texture size
                    // to get their position in texture space (or if we were rendering
                    // to the screen - screen space)
                    Vector3 texturespace_v1 = new Vector3(normalized_v1.x * (float)frameBufferTexture.width, normalized_v1.y * (float)frameBufferTexture.height, proj_v1.z);
                    Vector3 texturespace_v2 = new Vector3(normalized_v2.x * (float)frameBufferTexture.width, normalized_v2.y * (float)frameBufferTexture.height, proj_v2.z);
                    Vector3 texturespace_v3 = new Vector3(normalized_v3.x * (float)frameBufferTexture.width, normalized_v3.y * (float)frameBufferTexture.height, proj_v3.z);

                    // Draw the triangle interpolated between the three vertices,
                    // using the values calculated for these vertices based
                    // on the triangle indices
                    DrawInterpolatedTriangle(frameBufferTexture,
                        texturespace_v1, texturespace_v2, texturespace_v3,
                        w_v1, w_v2, w_v3,
                        w_n1, w_n2, w_n3,
                        uv1, uv2, uv3,
                        material_DiffuseColour, material_SpecularColour, material_Shininess,
                        material_DiffuseTexture, Material_EmissionMap, Material_EmissionColour,
                        cameraWorldPosition);
                }
            }
        }
    }

    // This function draws a triangle to a texture renderTexture
    void DrawInterpolatedTriangle(Texture2D renderTexture,
        Vector3 t_v1, Vector3 t_v2, Vector3 t_v3, //Texture Space Vertex Positions
        Vector3 w_v1, Vector3 w_v2, Vector3 w_v3, //World Vertex Positions
        Vector3 w_n1, Vector3 w_n2, Vector3 w_n3, //World Normal Vectors
        Vector2 uv1, Vector2 uv2, Vector2 uv3, //UV Coords
        Color material_DiffuseColour, Color material_SpecularColour, float material_Shininess,
        Texture2D material_DiffuseTexture, Texture2D Material_EmissionMap, Color Material_EmissionColour,
        Vector3 cameraWorldPosition)
    {
        // Calculate the bounding rectangle of the triangle based on the
        // three vertices
        RectInt triangleBoundingRect = new RectInt();
        triangleBoundingRect.xMin = (int)Mathf.Min(t_v1.x, Mathf.Min(t_v2.x, t_v3.x));
        triangleBoundingRect.xMax = (int)Mathf.Max(t_v1.x, Mathf.Max(t_v2.x, t_v3.x));
        triangleBoundingRect.yMin = (int)Mathf.Min(t_v1.y, Mathf.Min(t_v2.y, t_v3.y));
        triangleBoundingRect.yMax = (int)Mathf.Max(t_v1.y, Mathf.Max(t_v2.y, t_v3.y));

        // Cull the bounding rect to the size of the texture we're rendering to
        triangleBoundingRect.xMin = (int)Mathf.Max(triangleBoundingRect.xMin, 0);
        triangleBoundingRect.xMax = (int)Mathf.Min(triangleBoundingRect.xMax + 1, renderTexture.width);
        triangleBoundingRect.yMin = (int)Mathf.Max(triangleBoundingRect.yMin, 0);
        triangleBoundingRect.yMax = (int)Mathf.Min(triangleBoundingRect.yMax + 1, renderTexture.height);

        // Loop through every pixel in the bounding rect
        for (int y = triangleBoundingRect.yMin; y < triangleBoundingRect.yMax; y++)
        {
            for (int x = triangleBoundingRect.xMin; x < triangleBoundingRect.xMax; x++)
            {
                // Convert our integer x and y positions into a floating point
                // Vector 2 for floating point multiplication and ease use
                Vector2 p = new Vector2(x, y);

                // Calculate the weights weight_v1, weight_v2 and weight_v3 for the barycentric
                // coordinates based on the positions of the three vertices
                float denom = (t_v2.y - t_v3.y) * (t_v1.x - t_v3.x) + (t_v3.x - t_v2.x) * (t_v1.y - t_v3.y);
                float weight_v1 = ((t_v2.y - t_v3.y) * (p.x - t_v3.x) + (t_v3.x - t_v2.x) * (p.y - t_v3.y)) / denom;
                float weight_v2 = ((t_v3.y - t_v1.y) * (p.x - t_v3.x) + (t_v1.x - t_v3.x) * (p.y - t_v3.y)) / denom;
                float weight_v3 = 1 - weight_v1 - weight_v2;

                // If weight_v1, weight_v2 and weight_v3 are >= 0, we are
                // inside the triangle (or on an edge, but either way,
                // render the pixel)
                if (weight_v1 >= 0 && weight_v2 >= 0 && weight_v3 >= 0)
                {
                    // Calculate the position in our buffer based on our x and y values
                    int bufferPosition = x + (y * renderTexture.width);

                    // Calculate the depth value of this pixel
                    float depthValue = t_v1.z * weight_v1 + t_v2.z * weight_v2 + t_v3.z * weight_v3;

                    // If the depth value is less than what is currently in the
                    // depth buffer for this pixel
                    if (depthValue < depthBuffer[bufferPosition])
                    {
                        // Calculate the world position for this pixel
                        Vector3 pixelWorldPos = w_v1 * weight_v1 + w_v2 * weight_v2 + w_v3 * weight_v3;

                        // Calculate the world normal for this pixel
                        Vector3 pixelWorldNormal = w_n1 * weight_v1 + w_n2 * weight_v2 + w_n3 * weight_v3;

                        //TODO: Calculate the UV coordinate for this pixel
                        Vector2 uv = uv1 * weight_v1 + uv2 * weight_v2 + uv3 * weight_v3; // ????

                        // TODO: Set the diffuse colour to the correct color
                        Color diffuseColour = material_DiffuseColour;

                        // If there is a diffuse texture on the material
                        if (material_DiffuseTexture != null)
                        {
                            // TODO: Calculate the correct diffuse colour for this pixel
                            // Hint - the correct diffuse colour will be the colour stored
                            // in the diffuse map at the pixels UV coordinates, multipled
                            // by the material's diffuse colour
                            diffuseColour = material_DiffuseColour * (material_DiffuseTexture.GetPixelBilinear(uv.x, uv.y)); // ????
                        }

                        // TODO: Set the emission colour to the correct color
                        Color emissionColour = Material_EmissionColour; // ????

                        // If there is an emission map texture on the material
                        if (Material_EmissionMap != null)
                        {
                            // TODO: Calculate the correct emission colour for this pixel
                            // Hint - the correct emission colour will be the brightness stored
                            // in the emission map at the pixels UV coordinates, multipled
                            // by the material's emission colour. As the brightness of an emission 
                            // map pixel is a float that multiples with the emission colour -
                            // we can use .grayscale on a colour to get a single floating point
                            // value that corresponds to it's brightness
                            emissionColour = Material_EmissionMap.GetPixelBilinear(uv.x, uv.y) * Material_EmissionColour.grayscale;  // ????
                        }

                        // Calculate the final pixel colour
                        Color pixelColour = CalculatePixelColour(pixelWorldPos, pixelWorldNormal, diffuseColour, material_SpecularColour,
                            material_Shininess, cameraWorldPosition, lightSources);

                        //TODO: Add the emission colour to the calculated pixel colour
                        pixelColour += emissionColour;

                        // Update the frame buffer with this pixel colour
                        frameBuffer[bufferPosition] = pixelColour;

                        // Update the depth buffer with this depth value
                        depthBuffer[bufferPosition] = depthValue;
                    }

                }
            }
        }
    }

    // This function calculates the colours of a pixel, based
    // on the pixel world space normals, camera world pos, light source,
    // and material diffuse colour, specular colour and shininess
    Color CalculatePixelColour(
        Vector3 pixelWorldPosition, Vector3 pixelWorldNormal,
        Color diffuseColour, Color specularColour, float shininess,
        Vector3 cameraWorldPos, Light[] lightSources
        )
    {
        // Set the diffuse and specular values for this pixel to
        // black initially
        Color diffuseSum = Color.black;
        Color specularSum = Color.black;

        // Loop through each light source
        foreach (Light lightSource in lightSources)
        {

            // Calculate the light position and colour based on the light properties
            // LightW is set to 0 if the light is directional, and 1 otherwise
            float lightW; Vector3 lightPos; Color lightColour;
            float attenuation = 0;
            if (lightSource.type == LightType.Directional)
            {
                lightPos = -lightSource.transform.forward; lightW = 0;
                lightColour = lightSource.color * lightSource.intensity;
                attenuation = 1.0f;
            }
            else
            {
                lightPos = lightSource.transform.position; lightW = 1;
                // For non direcitonal lights, we'll figure out the light
                // Colour per pixel, based on the distance from the light
                // To the pixel
                lightColour = Color.black;
            }

            // Normalize the normal direction
            //float3 normalDirection = normalize(i.normal);
            Vector3 normalDirection = pixelWorldNormal.normalized;

            // Calculate the normalized view direction (the camera position -
            // the pixel world position) 
            //float3 viewDirection = normalize(_WorldSpaceCameraPos - i.posWorld.xyz);
            Vector3 viewDirection = (cameraWorldPos - pixelWorldPosition).normalized;

            // if our light source is not directional
            if (lightSource.type != LightType.Directional)
            {
                // Calculate the distance from the light to the pixel, and 1/distance
                //float3 vert2LightSource = _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
                Vector3 vert2LightSource = lightPos - pixelWorldPosition;
                //float oneOverDistance = 1.0 / length(vert2LightSource);
                //float attenuation = lerp(1.0, oneOverDistance, _WorldSpaceLightPos0.w); //Optimization for spot lights. This isn't needed if you're just getting started.
                attenuation = 1.0f / vert2LightSource.magnitude;

                // Calculate the colour based on the distance and lightsource range
                if (vert2LightSource.magnitude < lightSource.range)
                    lightColour = (lightSource.color * lightSource.intensity);
            }

            // Calculate light direction
            //float3 lightDirection = _WorldSpaceLightPos0.xyz - i.posWorld.xyz * _WorldSpaceLightPos0.w;
            Vector3 lightDirection = lightPos - pixelWorldPosition * lightW;

            // Calculate Diffuse Lighting
            //float3 diffuseReflection = attenuation * _LightColor0.rgb * _Color.rgb * max(0.0, dot(normalDirection, lightDirection)); //Diffuse component
            Color diffuseReflection = attenuation * lightColour * diffuseColour * Mathf.Max(0, Vector3.Dot(normalDirection, lightDirection));

            /*float3 specularReflection;
            if (dot(i.normal, lightDirection) < 0.0) //Light on the wrong side - no specular
            {
                specularReflection = float3(0.0, 0.0, 0.0);
            }
            else
            {
                //Specular component
                specularReflection = attenuation * _LightColor0.rgb * _SpecColor.rgb * pow(max(0.0, dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
            }*/

            // Calculate Specular reflection if the normal is pointing in the
            // Lights direction
            Color specularReflection;
            if (Vector3.Dot(normalDirection, lightDirection) < 0.0)
            {
                specularReflection = Color.black;
            }
            else
            {
                // Split out the specular colour and shininess from the specularbuffer
                specularReflection = attenuation * lightColour * specularColour * Mathf.Pow(Mathf.Max(0.0f, Vector3.Dot(Vector3.Reflect(-lightDirection, normalDirection), viewDirection)), shininess);
            }

            diffuseSum += diffuseReflection;
            specularSum += specularReflection;
        }

        // Calculate Ambient Lighting
        //float3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb; //Ambient component
        Color ambientLighting = RenderSettings.ambientLight * diffuseColour;

        // The final colour for this pixel is the sum of the ambient, diffuse and specular
        //float3 color = (ambientLighting + diffuseReflection) * tex2D(_Tex, i.uv) + specularReflection; //Texture is not applient on specularReflection
        return (ambientLighting + diffuseSum) + specularSum;
    }
}
