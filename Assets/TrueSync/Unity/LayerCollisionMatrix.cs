using UnityEngine;

namespace TrueSync {

    /**
     * @brief Manages the collision matrix of physics simulation.
     **/
    public class LayerCollisionMatrix {

        private static bool[,] matrix = new bool[32, 32];

        static void Init() {
            for (int i = 0; i < 32; i++) {
                for (int j = 0; j < 31 - i; j++) {
                    matrix[j, 31 - i] = matrix[i, 31 - j];
                }
            }
            //LayerMask.LayerToName (int name);
        }

        /**
         * @brief Returns true if the given layers can collide.
         * 
         * @param layerA Layer of the first object
         * @param layerB Layer of the second object
         **/
        public static bool CollisionEnabled(int layerA, int layerB) {
            return !UnityEngine.Physics2D.GetIgnoreLayerCollision(layerA, layerB);            
        }

        /**
         * @brief Returns true if the given GameObjects can collide (based on its layers).
         * 
         * @param goA First GameObject
         * @param goB Second GameObject
         **/
        public static bool CollisionEnabled(GameObject goA, GameObject goB) {
            return CollisionEnabled(goA.layer, goB.layer);
        }

    }

}