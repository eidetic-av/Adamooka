using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    public static SnakeController Instance;

    public ParticleSystem ParticleSystem;

    public int PositionIndex = 0;
    int MovementPosition = 0;
    public List<int> MovementOrder = new List<int>();

    public bool Advance = false;
    public float MovementDamping = 5f;

    public float ZPosition = -0.6f;
    
    Vector3 Position = Vector3.zero;

    void Start()
    {
        Instance = this;
    }

    void Update()
    {
        if (Advance)
        {
            PositionIndex = (PositionIndex + 1) % MovementOrder.Count;
            MovementPosition = MovementOrder[PositionIndex];
            switch (MovementPosition)
            {
                case 0:
                    Position = new Vector3(-1, -1, ZPosition);
                    break;
                case 1:
                    Position = new Vector3(1, -1, ZPosition);
                    break;
                case 2:
                    Position = new Vector3(1, 1, ZPosition);
                    break;
                case 3:
                    Position = new Vector3(-1, 1, ZPosition);
                    break;
            }
            Advance = false;
        }

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[1];
        ParticleSystem.GetParticles(particles);
        var currentPosition = particles[0].position;

        if (Mathf.Abs(Position.x - currentPosition.x) > 0)
        {
            currentPosition.x = currentPosition.x + (Position.x - currentPosition.x) / MovementDamping;
        }
        if (Mathf.Abs(Position.y - currentPosition.y) > 0)
        {
            currentPosition.y = currentPosition.y + (Position.y - currentPosition.y) / MovementDamping;
        }
        if (Mathf.Abs(Position.z - currentPosition.z) > 0)
        {
            currentPosition.z = currentPosition.z + (Position.z - currentPosition.z) / MovementDamping;
        }

        particles[0].position = currentPosition;
        ParticleSystem.SetParticles(particles, 1);
    }
}
