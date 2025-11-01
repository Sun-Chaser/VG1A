using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;

    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    private void Start()
    {  

        Rect[] deployZones = {
        new Rect(12.5f, 26f, 12f, 8f), 
        new Rect( -11.9f, -4.6f, 10f, 5f) 
        };
        Vector2 randomPosition = RandomDeployPoint(deployZones);

        PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity);
    }

    public static Vector2 RandomDeployPoint(params Rect[] zones)
    {
        Rect z = zones[Random.Range(0, zones.Length)];
        return new Vector2(Random.Range(z.xMin, z.xMax), Random.Range(z.yMin, z.yMax));
    }

}
