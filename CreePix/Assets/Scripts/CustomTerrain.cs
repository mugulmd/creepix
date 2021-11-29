using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTerrain : MonoBehaviour
{
    private Terrain terrain;
    private Collider terrain_collider;
    private TerrainData terrain_data;
    private int heightmap_width;
    private int heightmap_height;
    private float[,] heightmap_data;

    [Range(1, 100)]
    public int brush_radius = 10;

    public GameObject object_prefab = null;
    public float min_scale = 0.8f;
    public float max_scale = 1.2f;

    private Brush current_brush;

    [SerializeField]
    private Camera cam;

    public static System.Random rnd = new System.Random();

    // Start is called before the first frame update
    void Start()
    {
        if (!terrain)
            terrain = Terrain.activeTerrain;
        terrain_collider = terrain.GetComponent<Collider>();
        terrain_data = terrain.terrainData;
        heightmap_width = terrain_data.heightmapResolution;
        heightmap_height = terrain_data.heightmapResolution;
        heightmap_data = terrain_data.GetHeights(0, 0, heightmap_width, heightmap_height);
        current_brush = null;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 hit_loc = Vector3.zero;
        RaycastHit hit;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (terrain_collider.Raycast(ray, out hit, Mathf.Infinity))
        {
            hit_loc = hit.point;
            if (Input.GetMouseButton(0))
            {
                if (current_brush)
                    current_brush.callDraw(hit_loc.x, hit_loc.z);
            }
        }
    }

    // Get and set active brushes
    public void setBrush(Brush brush)
    {
        current_brush = brush;
    }
    public Brush getBrush()
    {
        return current_brush;
    }

    // Convert from world space to grid space
    public Vector3 world2grid(Vector3 grid)
    {
        return new Vector3(grid.x / terrain_data.heightmapScale.x,
                           grid.y,
                           grid.z / terrain_data.heightmapScale.z);
    }
    public Vector3 world2grid(float x, float y, float z)
    {
        return world2grid(new Vector3(x, y, z));
    }
    public Vector3 world2grid(float x, float z)
    {
        return world2grid(x, 0.0f, z);
    }

    // Get surface position
    public Vector3 get3(int x, int z)
    {
        return new Vector3(x, get(x, z), z);
    }
    public Vector3 get3(float x, float z)
    {
        return new Vector3(x, get(x, z), z);
    }
    public Vector3 getInterp3(float x, float z)
    {
        return new Vector3(x, getInterp(x, z), z);
    }

    // Get grid height for a node
    public float get(int x, int z)
    {
        x = (x + heightmap_width) % heightmap_width;
        z = (z + heightmap_height) % heightmap_height;
        return heightmap_data[z, x] * terrain_data.heightmapScale.y;
    }
    public float get(float x, float z)
    {
        return get((int)x, (int)z);
    }

    public float getInterp(float x, float z)
    {
        return terrain_data.GetInterpolatedHeight(x / heightmap_width,
                                                  z / heightmap_height);
    }
    public float getSteepness(float x, float z)
    {
        return terrain_data.GetSteepness(x / heightmap_width,
                                         z / heightmap_height);
    }
    public Vector3 getNormal(float x, float z)
    {
        return terrain_data.GetInterpolatedNormal(x / heightmap_width,
                                                  z / heightmap_height);
    }

    // Set the grid height for a node
    public void set(int x, int z, float val)
    {
        x = (x + heightmap_width) % heightmap_width;
        z = (z + heightmap_height) % heightmap_height;
        heightmap_data[z, x] = val / terrain_data.heightmapScale.y;
    }
    public void set(float x, float z, float val)
    {
        set((int)x, (int)z, val);
    }

    // Register changes made to the terrain
    public void save()
    {
        terrain_data.SetHeights(0, 0, heightmap_data);
    }

    // Terrain dimensions
    public int getWidth()
    {
        return heightmap_width;
    }
    public int getHeight()
    {
        return heightmap_height;
    }

    // Return index in tree prototypes array
    // Create a new tree prototype if necessary
    public int registerPrefab(GameObject go)
    {
        for (int i = 0; i < terrain_data.treePrototypes.Length; i++)
        {
            if (terrain_data.treePrototypes[i].prefab == go)
                return i;
        }

        TreePrototype proto = new TreePrototype();
        proto.bendFactor = 0.0f;
        proto.prefab = object_prefab;
        List<TreePrototype> protos = new List<TreePrototype>(terrain_data.treePrototypes);
        protos.Add(proto);
        terrain_data.treePrototypes = protos.ToArray();
        return protos.Count - 1;
    }

    // Spawn a new object (tree)
    public void spawnObject(Vector3 loc, float scale, int proto_idx)
    {
        TreeInstance obj = new TreeInstance();
        loc = new Vector3(loc.x / heightmap_width,
                          loc.y / terrain_data.heightmapScale.y,
                          loc.z / heightmap_height);
        obj.position = loc;
        obj.prototypeIndex = proto_idx;
        obj.lightmapColor = Color.white;
        obj.heightScale = scale;
        obj.widthScale = scale;
        obj.rotation = (float)rnd.NextDouble() * 2 * Mathf.PI;
        obj.color = Color.white;
        terrain.AddTreeInstance(obj);
    }

    // Object (tree) manipulation
    public int getObjectCount()
    {
        return terrain_data.treeInstanceCount;
    }
    public TreeInstance getObject(int index)
    {
        return terrain_data.GetTreeInstance(index);
    }
    // Returns an object (tree) location in grid space
    public Vector3 getObjectLoc(int index)
    {
        return getObjectLoc(terrain_data.GetTreeInstance(index));
    }
    public Vector3 getObjectLoc(TreeInstance obj)
    {
        return new Vector3(obj.position.x * heightmap_width,
                           obj.position.y * terrain_data.heightmapScale.y,
                           obj.position.z * heightmap_height);
    }
}
