using System.CodeDom.Compiler;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum EDirection{
    North,
    South,
    East,
    West
}

public class RoomGenerator : MonoBehaviour
{

    [Header("Room Parameters")]
    [SerializeField] EDirection inDirection;
    [SerializeField] EDirection outDirection;
    [SerializeField] Vector3 startPosition;
    //[SerializeField] int outDoorsCount = 3;
    [SerializeField] float plainWallChance = 0.5f;

    [Header("Walls")]
    [SerializeField] Transform[] cornerParents;
    [SerializeField] Transform[] NorthWallParents;
    [SerializeField] Transform[] SouthWallParents;
    [SerializeField] Transform[] EastWallParents;
    [SerializeField] Transform[] WestWallParents;


    [Header("Prefab objects")]
    [SerializeField] WallPrefabs wallPrefabs;
    [SerializeField] EnviromentPrefabs groundPrefabs;

    private readonly EDirection[] allDirections =
    {
        EDirection.North,
        EDirection.South,
        EDirection.East,
        EDirection.West
    };



    private void Start()
    {
        GenerateRoom(startPosition);
    }

    public void GenerateRoom(Vector3 position, EDirection inDir = EDirection.South, EDirection outDir = EDirection.North)
    {
        startPosition = position;

        if (wallPrefabs == null) return;

        inDirection = inDir;
        outDirection = outDir;

        GenerateCorners();

        GenerateInWalls();
        GenerateOutWalls();

        foreach (EDirection direction in allDirections)
        {
            if (direction != inDir && direction != outDir)
            {
                GenerateSideWalls(GetWallParents(direction));
            }
        }
    }

    void GenerateCorners()
    {
        foreach (var cornerParent in cornerParents)
        {
            GameObject corner = Instantiate(wallPrefabs.GetRandomCornerWall(), cornerParent);
            corner.transform.localPosition = Vector3.zero;
        }
    }

    Transform[] GetWallParents(EDirection direction)
    {
        switch (direction)
        {
            case EDirection.North:
                return NorthWallParents;
            case EDirection.South:
                return SouthWallParents;
            case EDirection.East:
                return EastWallParents;
            case EDirection.West:
                return WestWallParents;
            default: return null;
        }

    }

    void GenerateInWalls()
    {
        Transform[] inWalls = GetWallParents(inDirection);

        for (int i = 0; i < inWalls.Length; i++)
        {
            if (i == inWalls.Length / 2)
            {
                GenerateDoorwayFor(inWalls[i]);
                continue;
            }

            GenerateWallFor(inWalls[i]);
        }


    }

    void GenerateOutWalls()
    {
        Transform[] outWalls = GetWallParents(outDirection);

        int maxDoorCount = (outWalls.Length + 1) / 2;
        int outDoorsCount = Random.Range(1, maxDoorCount + 1);

        int doorPattern = GetDoorPattern(
            outWalls.Length,
            outDoorsCount);

        for (int i = 0; i < outWalls.Length; i++)
        {
            bool isDoor = (doorPattern & (1 << i)) != 0;

            if (isDoor)
            {
                GenerateDoorwayFor(outWalls[i]);
            }
            else
            {
                GenerateWallFor(outWalls[i]);
            }
        }
    }

    void GenerateSideWalls(Transform[] walls)
    {
        foreach (Transform wall in walls)
        {
            GenerateWallFor(wall);
        }
    }

    void GenerateDoorwayFor(Transform parent)
    {
        GameObject wall = Instantiate(wallPrefabs.GetRandomDoorwayWall(), parent.transform);
        wall.transform.localPosition = Vector3.zero;
        //wall.GetComponent<WallDecorator>().GenerateDecorations();
    }

    void GenerateWallFor(Transform parent)
    {
        float chance = Random.value;
        GameObject wallPref;

        if (chance < plainWallChance)
        {
            wallPref = wallPrefabs.GetRandomPlainWall();
        }
        else
        {
            wallPref = wallPrefabs.GetRandomWindowedWall();
        }

        GameObject wall = Instantiate(wallPref, parent.transform);
        wall.transform.localPosition = Vector3.zero;
        wall.GetComponent<WallDecorator>().GenerateDecorations();
    }


    private int GetDoorPattern(int wallCount, int doorCount)
    {
        if (wallCount == 5)
        {
            return doorCount switch
            {
                3 => 0b10101,
                2 => 0b01010,
                1 => 0b00100,
                _ => 0
            };
        }

        if (wallCount == 3)
        {
            return doorCount switch
            {
                2 => 0b101,
                1 => 0b010,
                _ => 0
            };
        }

        return 0;
    }
}