using UnityEngine;
using System.Collections.Generic;
using System;

namespace TrueSync {

    /**
     *  @brief Manages the 2D physics simulation.
     **/
    public class Physics2DWorldManager : IPhysicsManager {

        /**
         *  @brief Public access to a manager instance.
         **/
        public static Physics2DWorldManager instance;
        
        private Physics2D.World world;

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

        // Use this for initialization
        public void Init() {
            ChecksumExtractor.Init(this);

            world = new TrueSync.Physics2D.World(new TSVector2(Gravity.x, Gravity.y));
            Physics2D.ContactManager.physicsManager = this;

            world.BodyRemoved += OnRemovedRigidBody;
            world.ContactManager.BeginContact += CollisionEnter;
            world.ContactManager.StayContact += CollisionStay;
            world.ContactManager.EndContact += CollisionExit;

            gameObjectMap = new Dictionary<IBody, GameObject>();

            instance = this;
            AddRigidBodies();
        }

        /**
         *  @brief Goes one step further on the physics simulation.
         **/
        public void UpdateStep() {
            world.Step((FP) Time.fixedDeltaTime);
        }

        public void FixedUpdate() {
            UpdateStep();
        }

        public IWorld GetWorld() {
            return world;
        }

        void AddRigidBodies() {
            TSCollider2D[] bodies = GameObject.FindObjectsOfType<TSCollider2D>();
            List<TSCollider2D> sortedBodies = new List<TSCollider2D>(bodies);
            sortedBodies.Sort(UnityUtils.body2DComparer);

            for (int i = 0; i < sortedBodies.Count; i++) {
                AddBody(sortedBodies[i]);
            }
        }

        public void AddBody(ICollider iCollider) {
            TSCollider2D tsCollider = (TSCollider2D) iCollider;

            if (tsCollider._body != null) {
                //already added
                return;
            }

            tsCollider.Initialize(world);
            gameObjectMap[tsCollider._body] = tsCollider.gameObject;

            if (tsCollider.gameObject.transform.parent != null && tsCollider.gameObject.transform.parent.GetComponentInParent<TSCollider2D>() != null) {
                TSCollider2D parentCollider = tsCollider.gameObject.transform.parent.GetComponentInParent<TSCollider2D>();
                Physics2D.Body childBody = tsCollider._body;

                childBody.bodyConstraints.Add(new ConstraintHierarchy2D(((Physics2D.Body)parentCollider.Body), tsCollider._body, (tsCollider.GetComponent<TSTransform2D>().position + tsCollider.ScaledCenter) - (parentCollider.GetComponent<TSTransform2D>().position + parentCollider.ScaledCenter)));
            }

            world.ProcessAddedBodies();
        }

        internal string GetChecksum(bool plain) {
            return ChecksumExtractor.GetEncodedChecksum();
        }

        public void RemoveBody(IBody iBody) {
            world.RemoveBody((TrueSync.Physics2D.Body) iBody);
            world.ProcessRemovedBodies();
        }

        public void OnRemoveBody(Action<IBody> OnRemoveBody) {
            world.BodyRemoved += delegate (TrueSync.Physics2D.Body body) {
                OnRemoveBody(body);
            };
        }

        private void OnRemovedRigidBody(TrueSync.Physics2D.Body body) {
            GameObject go = gameObjectMap[body];

            if (go != null) {
                GameObject.Destroy(go);
            }
        }

        private bool CollisionEnter(TrueSync.Physics2D.Contact contact) {
            if (contact.FixtureA.IsSensor || contact.FixtureB.IsSensor) {
                TriggerEnter(contact);

                return true;
            }

            CollisionDetected(contact.FixtureA.Body, contact.FixtureB.Body, "OnSyncedCollisionEnter");
            return true;
        }

        private void CollisionStay(TrueSync.Physics2D.Contact contact) {
            if (contact.FixtureA.IsSensor || contact.FixtureB.IsSensor) {
                TriggerStay(contact);
                return;
            }

            CollisionDetected(contact.FixtureA.Body, contact.FixtureB.Body, "OnSyncedCollisionStay");
        }

        private void CollisionExit(TrueSync.Physics2D.Contact contact) {
            if (contact.FixtureA.IsSensor || contact.FixtureB.IsSensor) {
                TriggerExit(contact);
                return;
            }

            CollisionDetected(contact.FixtureA.Body, contact.FixtureB.Body, "OnSyncedCollisionExit");
        }

        private void TriggerEnter(TrueSync.Physics2D.Contact contact) {
            CollisionDetected(contact.FixtureA.Body, contact.FixtureB.Body, "OnSyncedTriggerEnter");
        }

        private void TriggerStay(TrueSync.Physics2D.Contact contact) {
            CollisionDetected(contact.FixtureA.Body, contact.FixtureB.Body, "OnSyncedTriggerStay");
        }

        private void TriggerExit(TrueSync.Physics2D.Contact contact) {
            CollisionDetected(contact.FixtureA.Body, contact.FixtureB.Body, "OnSyncedTriggerExit");
        }

        private void CollisionDetected(TrueSync.Physics2D.Body body1, TrueSync.Physics2D.Body body2, string callbackName) {
            GameObject b1 = gameObjectMap[body1];
            GameObject b2 = gameObjectMap[body2];

            if (b1 == null || b2 == null) {
                return;
            }

            b1.SendMessage(callbackName, b2, SendMessageOptions.DontRequireReceiver);
            b2.SendMessage(callbackName, b1, SendMessageOptions.DontRequireReceiver);

			TrueSyncManager.UpdateCoroutines ();
        }

        public GameObject GetGameObject(IBody body) {
            return gameObjectMap[body];
        }

        public bool Raycast(TSVector rayOrigin, TSVector rayDirection, RaycastCallback raycast, out IBody body, out TSVector normal, out FP fraction) {
            throw new NotImplementedException();
        }

        public TSRaycastHit Raycast(TSRay ray, FP maxDistance, RaycastCallback callback = null) {
            throw new NotImplementedException();
        }

        public bool IsCollisionEnabled(IBody rigidBody1, IBody rigidBody2) {
            if (gameObjectMap.ContainsKey(rigidBody1) && gameObjectMap.ContainsKey(rigidBody2)) {
                return LayerCollisionMatrix.CollisionEnabled(gameObjectMap[rigidBody1], gameObjectMap[rigidBody2]);
            }

            return true;
        }

        public IWorldClone GetWorldClone() {
            return new WorldClone2D();
        }

    }

}