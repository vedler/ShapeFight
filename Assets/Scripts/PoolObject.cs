using UnityEngine;
using System.Collections;

public interface PoolObject {
    void OnObjectReuse();
    void FireMe(Vector2 direction);
    void Reset();
}
