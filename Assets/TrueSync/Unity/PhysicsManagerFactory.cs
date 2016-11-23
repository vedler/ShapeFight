namespace TrueSync {

    /**
     *  @brief Manages physics simulation.
     **/
    public class PhysicsManager {

        /**
         *  @brief Indicates the type of physics simulations: 2D or 3D.
         **/
        public enum PhysicsType {W_2D, W_3D};

        /**
         *  @brief Returns a proper implementation of {@link IPhysicsManager}.
         **/
        public static IPhysicsManager instance;

        /**
         *  @brief Instantiates a new {@link IPhysicsManager}.
         *  
         *  @param physicsType Indicates if is a 2D or 3D world.
         **/
        public static IPhysicsManager New(PhysicsType physicsType) {
            if (physicsType == PhysicsType.W_2D) {
                instance = new Physics2DWorldManager();
            } else if (physicsType == PhysicsType.W_3D) {
                instance = new PhysicsWorldManager();
            }

            return instance;
        }

        /**
         *  @brief Instantiates a 3D physics for tests purpose.
         **/
        internal static void InitTest3D() {
            New(PhysicsManager.PhysicsType.W_3D);
            instance.Gravity = new TSVector(0, -10, 0);
            instance.Init();
        }

        /**
         *  @brief Instantiates a 2D physics for tests purpose.
         **/
        internal static void InitTest2D() {
            New(PhysicsManager.PhysicsType.W_2D);
            instance.Gravity = new TSVector(0, -10, 0);
            instance.Init();
        }

    }

}