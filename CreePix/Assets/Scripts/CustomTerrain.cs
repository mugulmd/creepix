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

    private Brush current_brush;

    [SerializeField]
    private Camera cam;

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
}
