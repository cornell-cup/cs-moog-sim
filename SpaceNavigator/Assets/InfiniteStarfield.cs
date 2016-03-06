using UnityEngine;
using System.Collections;

public class InfiniteStarfield : MonoBehaviour
{
    private ParticleSystem.Particle[] points;

    public int starsMax = 100;
    public float starSize = 1.0f;
    public float starDistance = 10;
    private float starDistSqr;

    // Use this for initialization
    void Start()
    {
        starDistSqr = starDistance * starDistance;
        points = new ParticleSystem.Particle[starsMax];

        for (int i = 0; i < starsMax; i++)
        {
            points[i].position = randomPosition();
            points[i].startColor = new Color(1, 1, 1, 1);
            points[i].startSize = starSize;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < starsMax; i++)
        {
            if ((points[i].position - transform.position).sqrMagnitude > starDistSqr)
            {
                points[i].position = randomPosition();
            }
        }
        GetComponent<ParticleSystem>().SetParticles(points, points.Length);
    }

    Vector3 randomPosition()
    {
        return Random.insideUnitSphere.normalized * starDistance + transform.position;
    }
}