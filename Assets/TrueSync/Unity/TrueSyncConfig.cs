using UnityEngine;

namespace TrueSync {

    public class TrueSyncConfig : ScriptableObject {

        /**
         * @brief Synchronization window size.
         **/
        public int syncWindow = 10;

        /**
         * @brief Rollback window size.
         **/
        public int rollbackWindow = 0;

        /**
         * @brief Maximum number of ticks to wait until all other players inputs arrive.
         **/
        public int panicWindow = 100;

        public TrueSyncConfig() {
        }

        public TrueSyncConfig(int syncWindow, int rollbackWindow, int panicWindow) {
            this.syncWindow = syncWindow;
            this.rollbackWindow = rollbackWindow;
            this.panicWindow = panicWindow;
        }

    }

}