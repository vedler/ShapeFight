using UnityEngine;
using System.Collections.Generic;
using System;

namespace TrueSync {

    /**
     *  @brief Manages the 3D physics simulation.
     **/
    public class PhysicsWorldManager : IPhysicsManager {

        private World world;

        Dictionary<IBody, GameObject> gameObjectMap;

        /**
         *  @brief Property access to simulated gravity.
         **/
        public TSVector Gravity {
            get;
            set;
        }

        /**
         *  @brief Property access to speculative contacts.
         **/
        public bool SpeculativeContacts {
            get;
            set;
        }

        /**
         *  @brief If true ignores Z axis updates.
         **/
        public bool is2D;

        // Use this for initialization
        public void Init() {
            ChecksumExtractor.Init(this);

            gameObjectMap = new Dictionary<IBody, GameObject>();

            CollisionSystemPersistentSAP collisionSystem = new CollisionSystemPersistentSAP();
            collisionSystem.EnableSpeculativeContacts = SpeculativeContacts;

            world = new World(collisionSystem);
            collisionSystem.world = world;

            world.physicsManager = this;
            world.Gravity = Gravity;

            world.Events.BodiesBeginCollide += CollisionEnter;
            world.Events.BodiesStayCollide += CollisionStay;
            world.Events.BodiesEndCollide += CollisionExit;

            world.Events.TriggerBeginCollide += TriggerEnter;
            world.Events.TriggerStayCollide += TriggerStay;
            world.Events.TriggerEndCollide += TriggerExit;

            world.Events.RemovedRigidBody += OnRemovedRigidBody;

            AddRigidBodies();
        }

        /**
         *  @brief Goes one step further on the physics simulation.
         **/
        public void UpdateStep() {
            world.Step((FP) Time.fixedDeltaTime, false);
        }

        /**
         *  @brief Instance of the current simulated world.
         **/
        public IWorld GetWorld() {
            return world;
        }

        void AddRigidBodies() {
            TSCollider[] bodies = GameObject.FindObjectsOfType<TSCollider>();
            List<TSCollider> sortedBodies = new List<TSCollider>(bodies);
            sortedBodies.Sort(UnityUtils.bodyComparer);

            for (int i = 0; i < sortedBodies.Count; i++) {
                AddBody(sortedBodies[i]);
            }
        }

        /**
         *  @brief Add a new RigidBody to the world.
         *  
         *  @param jRigidBody Instance of a {@link TSRigidBody}.
         **/
        public void AddBody(ICollider iCollider) {
            TSCollider tsCollider = (TSCollider) iCollider;

            if (tsCollider._body != null) {
                //already added
                return;
            }

            tsCollider.Initialize();
            world.AddBody(tsCollider._body);
            gameObjectMap[tsCollider._body] = tsCollider.gameObject;

            if (is2D && !tsCollider._body.IsStatic) {
                TSRigidBody tsRigidBody = tsCollider.GetComponent<TSRigidBody>();
                bool freezeAxis = tsRigidBody != null && tsRigidBody.freezeZAxis;
                world.AddConstraint(new Constraint2D(tsCollider._body, freezeAxis));
            }

            if (tsCollider.gameObject.transform.parent != null && tsCollider.gameObject.transform.parent.GetComponentInParent<TSCollider>() != null) {
                TSCollider parentCollider = tsCollider.gameObject.transform.parent.GetComponentInParent<TSCollider>();
				world.AddConstraint(new ConstraintHierarchy(parentCollider.Body, tsCollider._body, (tsCollider.GetComponent<TSTransform>().position + tsCollider.ScaledCenter) - (parentCollider.GetComponent<TSTransform>().position + parentCollider.ScaledCenter)));
            }
        }

        public void RemoveBody(IBody iBody) {
            world.RemoveBody((RigidBody) iBody);
        }

        public void OnRemoveBody(System.Action<IBody> OnRemoveBody){
            world.Events.RemovedRigidBody += delegate (RigidBody rb) {
                OnRemoveBody(rb);
            };
        }

        public bool Raycast(TSVector rayOrigin, TSVector rayDirection, RaycastCallback raycast, out IBody body, out TSVector normal, out FP fraction) {
            RigidBody rb;
            bool result = world.CollisionSystem.Raycast(rayOrigin, rayDirection, raycast, out rb, out normal, out fraction);
            body = rb;

            return result;
        }

        public TSRaycastHit Raycast(TSRay ray, FP maxDistance, RaycastCallback callback = null) {
            IBody hitBody;
            TSVector hitNormal;
            FP hitFraction;

            TSVector origin = ray.origin;
            TSVector direction = ray.direction;

            if (PhysicsManager.instance.Raycast(origin, direction, callback, out hitBody, out hitNormal, out hitFraction)) {
                if (hitFraction <= maxDistance) {
                    GameObject other = PhysicsManager.instance.GetGameObject(hitBody);
                    TSRigidBody bodyComponent = other.GetComponent<TSRigidBody>();
                    TSCollider colliderComponent = other.GetComponent<TSCollider>();
                    TSTransform transformComponent = other.GetComponent<TSTransform>();
                    return new TSRaycastHit(bodyComponent, colliderComponent, transformComponent, hitNormal, ray.origin, ray.direction, hitFraction);
                }
            } else {
                direction *= maxDistance;
                if (PhysicsManager.instance.Raycast(origin, direction, callback, out hitBody, out hitNormal, out hitFraction)) {
                    GameObject other = PhysicsManager.instance.GetGameObject(hitBody);
                    TSRigidBody bodyComponent = other.GetComponent<TSRigidBody>();
                    TSCollider colliderComponent = other.GetComponent<TSCollider>();
                    TSTransform transformComponent = other.GetComponent<TSTransform>();
                    return new TSRaycastHit(bodyComponent, colliderComponent, transformComponent, hitNormal, ray.origin, direction, hitFraction);
                }
            }
            return null;
        }

        private void OnRemovedRigidBody(RigidBody body) {
            GameObject go = gameObjectMap[body];

            if (go != null) {
                GameObject.Destroy(go);
            }
        }

        private void CollisionEnter(RigidBody body1, RigidBody body2) {
            CollisionDetected(body1, body2, "OnSyncedCollisionEnter");
        }

        private void CollisionStay(RigidBody body1, RigidBody body2) {
            CollisionDetected(body1, body2, "OnSyncedCollisionStay");
        }

        private void CollisionExit(RigidBody body1, RigidBody body2) {
            CollisionDetected(body1, body2, "OnSyncedCollisionExit");
        }

        private void TriggerEnter(RigidBody body1, RigidBody body2) {
            CollisionDetected(body1, body2, "OnSyncedTriggerEnter");
        }

        private void TriggerStay(RigidBody body1, RigidBody body2) {
            CollisionDetected(body1, body2, "OnSyncedTriggerStay");
        }

        private void TriggerExit(RigidBody body1, RigidBody body2) {
            CollisionDetected(body1, body2, "OnSyncedTriggerExit");
        }

        private void CollisionDetected(RigidBody body1, RigidBody body2, string callbackName) {
            GameObject b1 = gameObjectMap[body1];
            GameObject b2 = gameObjectMap[body2];

            if (b1 == null || b2 == null) {
                return;
            }

            b1.SendMessage(callbackName, b2, SendMessageOptions.DontRequireReceiver);
            b2.SendMessage(callbackName, b1, SendMessageOptions.DontRequireReceiver);

			TrueSyncManager.UpdateCoroutines ();
        }

        /**
         *  @brief Get the GameObject related to a specific RigidBody.
         *  
         *  @param rigidBody Instance of a {@link RigidBody}
         **/
        public GameObject GetGameObject(IBody rigidBody) {
            return gameObjectMap[rigidBody];
        }

        /**
         *  @brief Check if the collision between two RigidBodies is enabled.
         *  
         *  @param rigidBody1 First {@link RigidBody}
         *  @param rigidBody2 Second {@link RigidBody}
         **/
        public bool IsCollisionEnabled(IBody rigidBody1, IBody rigidBody2) {
            return LayerCollisionMatrix.CollisionEnabled(gameObjectMap[rigidBody1], gameObjectMap[rigidBody2]);
        }

        public IWorldClone GetWorldClone() {
            return new WorldClone();
        }

    }

}