using UnityEngine;
using System.Collections;

public class InfiniteStarfield : MonoBehaviour
{
    private ParticleSystem.Particle[] points;
    private ParticleSystem ps;

    public int starsMax = 100;
    public float starDistance = 10;
    public float starSizeMin = 1.0f;
    public float starSizeMax = 5.0f;
    private float starDistSqr;

    // Use this for initialization
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        starDistSqr = starDistance * starDistance;
        points = new ParticleSystem.Particle[starsMax];

        for (int i = 0; i < starsMax; i++)
        {
            points[i].position = randomPosition(true);
            points[i].startSize = Random.Range(starSizeMin,starSizeMax);
            points[i].startColor = Color.white;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Color c = Color.white;
        for (int i = 0; i < starsMax; i++)
        {
            float dist = (points[i].position - transform.position).sqrMagnitude;
            if (dist > starDistSqr)
            {
                points[i].position = randomPosition(false);
            }
            else
            {
                c.a = 1.0f - Mathf.Sqrt(dist / starDistSqr);
                points[i].color = c;
            }
        }
        ps.SetParticles(points, points.Length);
    }

    Vector3 randomPosition(bool init)
    {
        Vector3 rand = init ? Random.insideUnitSphere : Random.onUnitSphere;
        return rand * starDistance + transform.position;
    }
}