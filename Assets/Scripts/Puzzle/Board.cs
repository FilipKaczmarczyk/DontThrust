using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Puzzle
{
    public class Board : MonoBehaviour
    {
        public int width;
        public int height;

        [SerializeField] private ActionTile[] actionTilesPrefabs;

        public ActionTile[,] ActionTiles;
        public float actionTileSpeed;

        public MatchFinder matchFinder;

        private void Awake()
        {
            matchFinder = GetComponent<MatchFinder>();
        }

        private void Start()
        {
            ActionTiles = new ActionTile[width, height];
        
            SpawnTiles();
        }

        private void Update()
        {
            matchFinder.FindAllMatches();
        }

        private void SpawnTiles()
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var randomActionTile = Random.Range(0, actionTilesPrefabs.Length);
                    
                    var iterationLimiter = 0;
                    while (CheckMatchAt(new Vector2Int(x, y), actionTilesPrefabs[randomActionTile]) && iterationLimiter < 100)
                    {
                        randomActionTile = Random.Range(0, actionTilesPrefabs.Length);
                        iterationLimiter ++;
                    }

                    if (iterationLimiter >= 100)
                    {
                        Debug.LogError("Iterator is out of range");
                    }
                
                    SpawnTile(new Vector2Int(x, y), actionTilesPrefabs[randomActionTile]);
                }
            }
        }

        private void SpawnTile(Vector2Int positionToSpawn, ActionTile actionTileToSpawn)
        {
            var spawnedActionTile = Instantiate(actionTileToSpawn, new Vector3(positionToSpawn.x, positionToSpawn.y, 0.0f), Quaternion.identity, transform);
            spawnedActionTile.name = "Tile - " + positionToSpawn.x + ", " + positionToSpawn.y;
        
            ActionTiles[positionToSpawn.x, positionToSpawn.y] = spawnedActionTile;
            spawnedActionTile.SetupGem(positionToSpawn, this);
        }

        private bool CheckMatchAt(Vector2Int positionToCheck, ActionTile tileToCheck)
        {
            if (positionToCheck.x > 1)
            {
                if (ActionTiles[positionToCheck.x - 1, positionToCheck.y].actionType == tileToCheck.actionType && 
                    ActionTiles[positionToCheck.x - 2, positionToCheck.y].actionType == tileToCheck.actionType)
                {
                    return true;
                }
            }
            
            if (positionToCheck.y > 1)
            {
                if (ActionTiles[positionToCheck.x, positionToCheck.y - 1].actionType == tileToCheck.actionType && 
                    ActionTiles[positionToCheck.x, positionToCheck.y - 2].actionType == tileToCheck.actionType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}