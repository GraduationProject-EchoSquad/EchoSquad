using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Addressables 기반의 오브젝트 풀링을 관리하는 싱글턴 클래스입니다.
/// </summary>
public class ResourceController : Singleton<ResourceController>
{
    // 1. 로드된 원본 프리팹을 캐싱하는 딕셔너리
    private Dictionary<string, AsyncOperationHandle<GameObject>> _prefabHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();
    
    // 2. 비활성화된 인스턴스들을 관리하는 풀
    private Dictionary<string, Queue<GameObject>> _pool = new Dictionary<string, Queue<GameObject>>();

    /// <summary>
    /// 풀에서 오브젝트를 가져오거나, 없으면 새로 생성합니다.
    /// </summary>
    /// <param name="assetPath">에셋의 Addressable 경로</param>
    /// <param name="parent">생성될 오브젝트의 부모 Transform</param>
    /// <returns>요청한 컴포넌트</returns>
    public async UniTask<T> GetAsync<T>(string assetPath, Transform parent = null) where T : Component
    {
        // 1. 풀에 사용 가능한 오브젝트가 있는지 확인
        if (_pool.TryGetValue(assetPath, out var queue) && queue.Count > 0)
        {
            GameObject instance = queue.Dequeue();
            instance.transform.SetParent(parent);
            instance.SetActive(true);
            return instance.GetComponent<T>();
        }

        // 2. 풀이 비어있으면, 원본 프리팹이 로드되었는지 확인
        if (!_prefabHandles.TryGetValue(assetPath, out var handle))
        {
            handle = Addressables.LoadAssetAsync<GameObject>(assetPath);
            await handle.ToUniTask();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[Pool] 프리팹 로드 실패: {assetPath}");
                return null;
            }
            _prefabHandles[assetPath] = handle;
        }

        GameObject prefab = handle.Result;
        if (prefab == null)
        {
            Debug.LogError($"[UIManager] UI 로드 실패! '{assetPath}'에 해당하는 프리팹을 경로 '{assetPath}'에서 찾을 수 없거나, 해당 프리팹에 '{typeof(T).Name}' 컴포넌트가 없습니다. 경로와 프리팹 설정을 확인하세요.");

            return null;
        }

        // 3. 프리팹으로 새 인스턴스 생성
        GameObject newInstance = Instantiate(prefab, parent);
        // 어떤 풀로 돌아가야 하는지 알려주기 위해 Poolable 컴포넌트 추가
        newInstance.AddComponent<Poolable>().AssetPath = assetPath;
        
        Debug.Log($"[Pool] 새 오브젝트 생성: {assetPath}");
        return newInstance.GetComponent<T>();
    }

    /// <summary>
    /// 사용이 끝난 오브젝트를 풀에 반환합니다.
    /// </summary>
    /// <param name="instanceToReturn">반환할 게임 오브젝트</param>
    public void Return(GameObject instanceToReturn)
    {
        Poolable poolable = instanceToReturn.GetComponent<Poolable>();
        if (poolable == null)
        {
            Debug.LogWarning($"[Pool] 풀링되지 않은 오브젝트({instanceToReturn.name}) 반환 시도. 오브젝트를 파괴합니다.");
            Destroy(instanceToReturn);
            return;
        }

        string assetPath = poolable.AssetPath;
        if (!_pool.TryGetValue(assetPath, out var queue))
        {
            queue = new Queue<GameObject>();
            _pool[assetPath] = queue;
        }
        
        instanceToReturn.SetActive(false);
        instanceToReturn.transform.SetParent(this.transform); // Hierarchy를 깔끔하게 유지
        queue.Enqueue(instanceToReturn);
    }

    // 풀링된 오브젝트의 출처(AssetPath)를 추적하기 위한 헬퍼 컴포넌트
    private class Poolable : MonoBehaviour
    {
        public string AssetPath;
    }

    private void OnDestroy()
    {
        // 모든 풀링된 인스턴스 파괴
        foreach (var queue in _pool.Values)
            foreach (var instance in queue)
                Destroy(instance);

        // 로드된 모든 프리팹 핸들 해제
        foreach (var handle in _prefabHandles.Values)
            Addressables.Release(handle);

        _pool.Clear();
        _prefabHandles.Clear();
    }
}