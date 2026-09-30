using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AdventureWorldGenerator : MonoBehaviour
{
    [Header("World")]
    public int seed = 1151;
    [Range(129, 513)] public int heightmapResolution = 257;
    [Range(40, 300)] public int treeCount = 150;
    public float worldSize = 240f;
    public float maximumHillHeight = 32f;

    Material groundMaterial;
    Material foliageMaterial;
    Material trunkMaterial;
    Material waterMaterial;
    Material objectiveMaterial;
    Terrain generatedTerrain;
    const float LakeX = 50f;
    const float LakeZ = 10f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateAtStartup()
    {
        if (FindFirstObjectByType<AdventureWorldGenerator>() == null)
        {
            new GameObject("Adventure World Generator").AddComponent<AdventureWorldGenerator>();
        }
    }

    void Start()
    {
        BuildWorld();
    }

    void BuildWorld()
    {
        RemoveStarterObjects();
        CreateMaterials();
        CreateTerrain();
        CreateLake();
        CreateForest();
        CreateObjective();
        CreatePlayer();
        ConfigureLighting();
    }

    void RemoveStarterObjects()
    {
        Destroy(GameObject.Find("Sphere"));
        Destroy(GameObject.Find("Plane"));
        var oldCamera = GameObject.Find("Main Camera");
        if (oldCamera != null)
        {
            Destroy(oldCamera);
        }
    }

    void CreateMaterials()
    {
        groundMaterial = MakeMaterial(new Color(0.24f, 0.39f, 0.19f));
        foliageMaterial = MakeMaterial(new Color(0.13f, 0.31f, 0.15f));
        trunkMaterial = MakeMaterial(new Color(0.25f, 0.15f, 0.08f));
        waterMaterial = MakeMaterial(new Color(0.12f, 0.55f, 0.62f, 0.88f));
        objectiveMaterial = MakeMaterial(new Color(1f, 0.62f, 0.13f));
        waterMaterial.SetFloat("_Surface", 1f);
        waterMaterial.SetFloat("_Blend", 0f);
        waterMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        waterMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        waterMaterial.SetFloat("_ZWrite", 0f);
        waterMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        waterMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        waterMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
    }

    static Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("HDRP/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var material = new Material(shader);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.72f);
        return material;
    }

    void CreateTerrain()
    {
        var data = new TerrainData();
        int resolution = Mathf.ClosestPowerOfTwo(Mathf.Clamp(heightmapResolution - 1, 128, 512)) + 1;
        data.heightmapResolution = resolution;
        data.size = new Vector3(worldSize, maximumHillHeight, worldSize);
        data.SetHeights(0, 0, MakeHeightmap(resolution));
#if UNITY_EDITOR
        var grassLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(
            "Assets/TerrainDemoScene_URP/Terrain/Layers/Grass_A.terrainlayer");
        if (grassLayer != null)
        {
            int alphamapResolution = Mathf.Clamp(resolution - 1, 16, 2048);
            data.alphamapResolution = alphamapResolution;
            data.terrainLayers = new[] { grassLayer };
            var paint = new float[alphamapResolution, alphamapResolution, 1];
            for (int z = 0; z < alphamapResolution; z++)
            for (int x = 0; x < alphamapResolution; x++) paint[z, x, 0] = 1f;
            data.SetAlphamaps(0, 0, paint);
        }
#endif
        GameObject terrainObject = Terrain.CreateTerrainGameObject(data);
        terrainObject.name = "Generated Hills and Forest Floor";
        terrainObject.transform.position = new Vector3(-worldSize * 0.5f, 0f, -worldSize * 0.5f);
        generatedTerrain = terrainObject.GetComponent<Terrain>();
        generatedTerrain.heightmapPixelError = 5f;
        generatedTerrain.basemapDistance = 300f;
        generatedTerrain.drawTreesAndFoliage = true;
    }

    float[,] MakeHeightmap(int resolution)
    {
        var heights = new float[resolution, resolution];
        var random = new System.Random(seed);
        float offsetX = (float)random.NextDouble() * 10000f;
        float offsetZ = (float)random.NextDouble() * 10000f;
        for (int z = 0; z < resolution; z++)
        for (int x = 0; x < resolution; x++)
        {
            float nx = x / (float)(resolution - 1);
            float nz = z / (float)(resolution - 1);
            float broad = Mathf.PerlinNoise(offsetX + nx * 3.4f, offsetZ + nz * 3.4f);
            float detail = Mathf.PerlinNoise(offsetX + nx * 11f, offsetZ + nz * 11f);
            float height = 0.035f + broad * 0.09f + Mathf.Max(0f, detail - 0.48f) * 0.16f;
            height += Hill(nx, nz, 0.2f, 0.78f, 0.18f);
            height += Hill(nx, nz, 0.83f, 0.78f, 0.24f);
            height += Hill(nx, nz, 0.83f, 0.2f, 0.2f);

            float lakeX = (LakeX + worldSize * 0.5f) / worldSize;
            float lakeZ = (LakeZ + worldSize * 0.5f) / worldSize;
            float lakeDistance = Mathf.Sqrt(Mathf.Pow((nx - lakeX) / 0.1f, 2f) + Mathf.Pow((nz - lakeZ) / 0.085f, 2f));
            float basin = Mathf.Clamp01(1.15f - lakeDistance);
            height = Mathf.Lerp(height, 0.035f + (1f - basin) * 0.025f, basin);
            heights[z, x] = Mathf.Clamp01(height);
        }
        return heights;
    }

    static float Hill(float x, float z, float centerX, float centerZ, float strength)
    {
        float dx = (x - centerX) / 0.16f;
        float dz = (z - centerZ) / 0.19f;
        return strength * Mathf.Exp(-(dx * dx + dz * dz) * 1.6f);
    }

    void CreateLake()
    {
        float groundHeight = generatedTerrain.SampleHeight(new Vector3(LakeX, 0f, LakeZ));
        Vector3 lakePosition = new Vector3(LakeX, Mathf.Max(groundHeight + 1.1f, 2.8f), LakeZ);
        GameObject lake = null;
#if UNITY_EDITOR
        var waterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/WaterBlock_50m Variant.prefab");
        if (waterPrefab != null)
        {
            lake = Instantiate(waterPrefab, lakePosition, Quaternion.identity);
            foreach (var lakeCollider in lake.GetComponentsInChildren<Collider>())
            {
                Destroy(lakeCollider);
            }
        }
#endif
        if (lake == null)
        {
            lake = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lake.transform.localScale = new Vector3(70f, 0.05f, 52f);
            lake.GetComponent<Renderer>().sharedMaterial = waterMaterial;
            Destroy(lake.GetComponent<Collider>());
        }
        lake.name = "Procedural Lake";
        lake.transform.position = lakePosition;
    }

    void CreateForest()
    {
        var random = new System.Random(seed ^ 0x51F15E);
        int created = 0;
        int attempts = treeCount * 18;
        for (int i = 0; i < attempts && created < treeCount; i++)
        {
            float x = Mathf.Lerp(-worldSize * 0.46f, worldSize * 0.46f, (float)random.NextDouble());
            float z = Mathf.Lerp(-worldSize * 0.46f, worldSize * 0.46f, (float)random.NextDouble());
            if (Vector2.Distance(new Vector2(x, z), Vector2.zero) < 14f) continue;
            if (Vector2.Distance(new Vector2(x, z), new Vector2(0f, -18f)) < 11f) continue;
            if (Mathf.Pow((x - LakeX) / 37f, 2f) + Mathf.Pow((z - LakeZ) / 28f, 2f) < 1f) continue;

            Vector3 basePosition = new Vector3(x, 0f, z);
            basePosition.y = generatedTerrain.SampleHeight(basePosition);
            float height = Mathf.Lerp(3.5f, 6.5f, (float)random.NextDouble());
            CreateTree(basePosition, height, (float)random.NextDouble());
            created++;
        }
    }

    void CreateTree(Vector3 basePosition, float height, float variation)
    {
        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Tree Trunk";
        trunk.transform.position = basePosition + Vector3.up * (height * 0.38f);
        trunk.transform.localScale = new Vector3(0.48f, height * 0.38f, 0.48f);
        trunk.GetComponent<Renderer>().sharedMaterial = trunkMaterial;

        var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.name = "Tree Canopy";
        crown.transform.position = basePosition + Vector3.up * (height * 0.88f);
        float canopy = Mathf.Lerp(2.8f, 4.2f, variation);
        crown.transform.localScale = new Vector3(canopy, canopy * 1.1f, canopy);
        crown.GetComponent<Renderer>().sharedMaterial = foliageMaterial;
        Destroy(crown.GetComponent<Collider>());
    }

    void CreateObjective()
    {
        Vector3 position = new Vector3(0f, 0f, 0f);
        position.y = generatedTerrain.SampleHeight(position) + 1f;
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Forest Center Cube";
        cube.transform.position = position;
        cube.transform.localScale = Vector3.one * 1.6f;
        cube.GetComponent<Renderer>().sharedMaterial = objectiveMaterial;
        cube.GetComponent<BoxCollider>().isTrigger = true;
        cube.AddComponent<ExplodingCube>();
    }

    void CreatePlayer()
    {
        Vector3 position = new Vector3(0f, 0f, -18f);
        position.y = generatedTerrain.SampleHeight(position) + 1f;
        var player = new GameObject("Explorer");
        player.transform.position = position;
        var controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.38f;
        controller.center = new Vector3(0f, 0.9f, 0f);
        controller.stepOffset = 0.35f;
        var character = player.AddComponent<MainChar>();

        var cameraObject = new GameObject("Explorer Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(player.transform, false);
        cameraObject.transform.localPosition = new Vector3(0f, 1.58f, 0f);
        var camera = cameraObject.AddComponent<Camera>();
        camera.nearClipPlane = 0.03f;
        camera.farClipPlane = 400f;
        cameraObject.AddComponent<AudioListener>();
        character.controller = controller;
        character.cameraTransform = cameraObject.transform;
    }

    void ConfigureLighting()
    {
        RenderSettings.ambientLight = new Color(0.48f, 0.56f, 0.47f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.57f, 0.68f, 0.62f);
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.002f;
        var sun = FindFirstObjectByType<Light>();
        if (sun != null && sun.type == LightType.Directional)
        {
            sun.transform.rotation = Quaternion.Euler(42f, -32f, 0f);
            sun.intensity = 1.2f;
        }
    }
}
