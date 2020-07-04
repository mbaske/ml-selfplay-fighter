using UnityEngine;
using System;

namespace MBaske.Fighter
{
    public struct OpponentCollision
    {
        public Collision collision;
        public BodyPart bodyPartA;
        public BodyPart bodyPartB;
        public Vector3 velocity;

        public bool IsPunch()
        {
            return bodyPartA.Name == BodyPartName.Hand;
        }

        public bool IsKick()
        {
            return bodyPartA.Name == BodyPartName.Foot;
        }

        public bool IsUpperBodyHit()
        {
            return bodyPartB.Name == BodyPartName.Head || 
                   bodyPartB.Name == BodyPartName.Spine1 ||
                   bodyPartB.Name == BodyPartName.Spine2;
        }

        public bool OpponentBlocksPunch()
        {
            return bodyPartB.Name == BodyPartName.Hand || 
                   bodyPartB.Name == BodyPartName.ForeArm;
        }
    }

    [RequireComponent(typeof(Rigidbody))]
    public class CollisionDetection : MonoBehaviour
    {
        public event Action<OpponentCollision> OnCollision;

        private OpponentCollision collision;
        private Rigidbody rb;
        private int oppLayer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            oppLayer = gameObject.layer == 9 ? 10 : 9;
            collision = new OpponentCollision() { bodyPartA = GetComponent<BodyPart>() };
        }

        private void OnCollisionEnter(Collision other)
        {
            GameObject oppObj = other.collider.gameObject;
            if (oppObj.layer == oppLayer)
            {
                collision.bodyPartB = oppObj.GetComponent<BodyPart>();
                collision.collision = other;
                collision.velocity = rb.velocity;
                OnCollision?.Invoke(collision);
            }
        }
    }
}