using UnityEngine;
using System.Collections;

public class InfiniteStarfield : MonoBehaviour
{
    private ParticleSystem.Particle[] points;
    private ParticleSystem ps;

    public int starsMax = 100;
    public float starDistance = 10;
    public float starSize = 1.0f;
    private float starDistSqr;

    // Use this for initialization
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        starDistSqr = starDistance * starDistance;
        points = new ParticleSystem.Particle[starsMax];

        for (int i = 0; i < starsMax; i++)
        {
            points[i].position = randomPosition();
            float rand = Random.value;
            points[i].startSize = starSize * Mathf.Pow(0.1292f * rand, -0.602f);
            points[i].startColor = randomColor(rand);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < starsMax; i++)
        {
            if ((points[i].position - transform.position).sqrMagnitude > starDistSqr*points[i].startSize/starSize)
            {
                points[i].position = randomPosition();
            }
        }
        ps.SetParticles(points, points.Length);
    }

    Vector3 randomPosition()
    {
        return Random.insideUnitSphere.normalized * starDistance + transform.position;
    }

    Color randomColor(float rand)
    {
        //Using stellar size and apparent color distribution from
        //https://en.wikipedia.org/wiki/Stellar_classification

        if (rand < 0.0013)
        {
            return new Color(0.67f, 0.75f, 1);
        }
        else if (rand < 0.0073)
        {
            return new Color(0.8f, 0.85f, 1);
        }
        else if (rand < 0.0373)
        {
            return new Color(1, 1, 1);
        }
        else if (rand < 0.1133)
        {
            return new Color(1, 0.95f, 0.92f);
        }
        else if (rand < 0.2343)
        {
            return new Color(1, 0.82f, 0.63f);
        }
        else
        {
            return new Color(1, 0.8f, 0.44f);
        }
    }
}