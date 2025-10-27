using UnityEngine;
using UnityEngine.Networking; // 必须导入这个才能使用 UnityWebRequest
using System.Collections;    // 必须导入这个才能使用 IEnumerator (Coroutine)
using System.Collections.Generic;
using System.Text;          // 必须导入这个才能使用 Encoding.UTF8
using System;                // 必须导入这个才能使用 Action (回调)

/// <summary>
/// API 客户端 (单例)
/// 负责处理所有与 Python FastAPI 服务器的通信。
/// </summary>
public class APIClient : MonoBehaviour
{
    // === 单例模式实现 ===
    // 让我们可以在其他任何脚本中通过 APIClient.Instance 来调用它
    public static APIClient Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 可选：如果跨场景，取消此行注释
        }
    }

    // === Inspector 配置 ===
    [Header("API Configuration")]
    public string apiUrl = "http://localhost:8000/api/grouping";
    
    [SerializeField]
    private int timeoutSeconds = 10; // 超时设置

    // === 核心公开方法 ===

    /// <summary>
    /// 异步发送分组请求到 Python 服务器
    /// </summary>
    /// <param name="packages">包含 PackageController 的列表</param>
    /// <param name="onSuccess">成功时调用的回调函数 (返回 GroupingResponse)</param>
    /// <param name="onError">失败时调用的回调函数 (返回 错误信息)</param>
    public void RequestPackageGrouping(List<PackageController> packages, 
                                       Action<GroupingResponse> onSuccess, 
                                       Action<string> onError)
    {
        // 启动 Coroutine 来执行异步的网络请求
        StartCoroutine(SendGroupingRequest(packages, onSuccess, onError));
    }

    /// <summary>
    /// [协程] 实际执行网络请求的方法
    /// </summary>
    private IEnumerator SendGroupingRequest(List<PackageController> packages, 
                                            Action<GroupingResponse> onSuccess, 
                                            Action<string> onError)
    {
        Debug.Log($"[APIClient] 正在准备发送 {packages.Count} 个包裹的数据到 {apiUrl}...");

        // --- 1. 构建请求数据 (JSON) ---
        // 我们不直接发送 PackageController，而是发送干净的 DTO (Data Transfer Object)
        GroupingRequest requestData = BuildRequestData(packages);
        
        // 使用 JsonUtility 将 C# 对象序列化为 JSON 字符串
        string jsonData = JsonUtility.ToJson(requestData);
        Debug.Log("[APIClient] Sending JSON: " + jsonData);

        // 将 JSON 字符串转换为 UTF-8 字节数组
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        // --- 2. 创建和配置 POST 请求 ---
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");

        // 设置 Upload Handler (告诉 UWR 要发送什么)
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        
        // 设置 Download Handler (告诉 UWR 如何存储响应)
        request.downloadHandler = new DownloadHandlerBuffer();

        // 设置必要的请求头
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");

        // 设置超时
        request.timeout = timeoutSeconds;

        // --- 3. 发送并等待 (yield) ---
        // yield return 会暂停这个协程，直到网络请求完成
        yield return request.SendWebRequest();

        // --- 4. 处理响应 ---
        // 检查是否有网络错误或 HTTP 协议错误
        if (request.result != UnityWebRequest.Result.Success)
        {
            // === 5. 错误处理 ===
            string errorMsg = $"[APIClient] Error: {request.error} | HTTP Code: {request.responseCode}";
            Debug.LogError(errorMsg);
            
            // 调用失败回调
            onError?.Invoke(errorMsg); // ?. 是一种安全的调用方式，如果 onError 为 null 则不执行
        }
        else
        {
            // === 6. 成功处理 ===
            Debug.Log($"[APIClient] Success! HTTP Code: {request.responseCode}");
            
            // 从 Download Handler 获取响应文本
            string responseJson = request.downloadHandler.text;
            Debug.Log("[APIClient] Received JSON: " + responseJson);

            // --- 7. 解析响应的 JSON ---
            try
            {
                // 使用 JsonUtility 将 JSON 字符串反序列化为 C# 对象
                GroupingResponse response = JsonUtility.FromJson<GroupingResponse>(responseJson);

                if (response.success)
                {
                    // API 逻辑成功
                    Debug.Log("[APIClient] API logic successful.");
                    onSuccess?.Invoke(response); // 调用成功回调
                }
                else
                {
                    // API 逻辑失败 (例如，Python 返回了 "success": false)
                    string errorMsg = $"[APIClient] API logic error: {response.error_message}";
                    Debug.LogWarning(errorMsg);
                    onError?.Invoke(errorMsg); // 调用失败回调
                }
            }
            catch (Exception ex)
            {
                // JSON 解析错误
                string errorMsg = $"[APIClient] JSON Parse Error: {ex.Message}. Received: {responseJson}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg); // 调用失败回调
            }
        }
        
        // 无论成功与否，最后都要释放网络请求的资源
        request.Dispose();
    }


    /// <summary>
    /// 辅助方法：将 PackageController 列表转换为 GroupingRequest DTO
    /// </summary>
    private GroupingRequest BuildRequestData(List<PackageController> packages)
    {
        GroupingRequest requestData = new GroupingRequest();
        requestData.num_groups = 5; // TODO: 让这个可配置

        foreach (PackageController controller in packages)
        {
            // 1. 创建 PackageData (DTO)
            PackageData packageDto = new PackageData();
            packageDto.package_id = controller.GetPackageID();
            packageDto.bricks = new List<BrickData>();

            // 2. 遍历包裹中的所有 Brick (Data)
            foreach (Brick brick in controller.GetBricks())
            {
                // 3. 创建 BrickData (DTO)
                BrickData brickDto = new BrickData
                {
                    brick_id = brick.id,
                    weight = brick.weight
                };
                packageDto.bricks.Add(brickDto);
            }
            
            // 4. 将 PackageData 添加到请求列表
            requestData.packages.Add(packageDto);
        }

        return requestData;
    }
}