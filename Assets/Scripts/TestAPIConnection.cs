using UnityEngine;
using System.Collections.Generic; // 必须导入这个才能使用 List<>

/// <summary>
/// 这是一个测试脚本，用于在 Unity 编辑器中触发一次完整的
/// “生成数据 -> 发送 API -> 接收响应” 的端到端测试。
/// 
/// 它依赖 ProductionLineManager 和 APIClient 才能工作。
/// </summary>
public class TestAPIConnection : MonoBehaviour
{
    // === Inspector 按钮 ===
    
    /// <summary>
    /// [ContextMenu("...")] 会在 Inspector 窗口的组件上
    /// (点击三个点或右键) 添加一个菜单按钮，
    /// 让我们可以在 *非运行模式* 或 *运行模式* 下都能触发这个方法。
    /// 这对于测试来说非常方便！
    /// </summary>
    [ContextMenu("Run Full Test: Generate, Send, and Receive")]
    public void RunFullTest()
    {
        Debug.Log("======================================");
        Debug.Log("[TestAPI] --- 启动完整 API 测试 ---");
        Debug.Log("======================================");

        // --- 1. 检查依赖 ---
        if (ProductionLineManager.Instance == null)
        {
            Debug.LogError("[TestAPI] 失败：找不到 ProductionLineManager.Instance！" +
                           "请确保 ProductionLineManager 脚本已附加到 _Managers 物体上。");
            return;
        }

        if (APIClient.Instance == null)
        {
            Debug.LogError("[TestAPI] 失败：找不到 APIClient.Instance！" +
                           "请确保 APIClient 脚本已附加到 _Managers 物体上。");
            return;
        }

        // --- 2. 生成数据 ---
        Debug.Log("[TestAPI] 步骤 1: 命令 ProductionLineManager 生成新批次...");
        ProductionLineManager.Instance.GenerateNewBatch();

        // --- 3. 获取数据 ---
        List<PackageController> batchToTest = ProductionLineManager.Instance.GetCurrentBatch();
        
        if (batchToTest == null || batchToTest.Count == 0)
        {
            Debug.LogError("[TestAPI] 失败：Manager 生成的批次为空！");
            return;
        }
        Debug.Log($"[TestAPI] 步骤 2: 已获取 {batchToTest.Count} 个包裹的数据。");

        // --- 4. 发送 API 请求 ---
        Debug.Log("[TestAPI] 步骤 3: 调用 APIClient 发送请求...");
        APIClient.Instance.RequestPackageGrouping(
            batchToTest,
            OnTestSuccess, // 成功时调用这个方法
            OnTestError    // 失败时调用这个方法
        );
    }

    // === 5. 回调方法 ===

    /// <summary>
    /// 当 APIClient 报告成功时，此方法会被调用
    /// </summary>
    private void OnTestSuccess(GroupingResponse response)
    {
        Debug.Log("======================================");
        Debug.Log("[TestAPI] --- 测试成功 ---");
        Debug.Log("======================================");
        Debug.Log("[TestAPI] 成功从服务器收到响应。");

        // 如何漂亮地打印 JSON 数据 (回答你的问题):
        // JsonUtility.ToJson() 的第二个参数 `true` 会开启 "prettyPrint" (格式化输出)。
        string prettyJson = JsonUtility.ToJson(response, true);
        
        Debug.Log($"[TestAPI] 响应内容:\n{prettyJson}");
    }

    /// <summary>
    /// 当 APIClient 报告失败时，此方法会被调用
    /// </summary>
    private void OnTestError(string errorMessage)
    {
        Debug.Log("======================================");
        Debug.LogWarning("[TestAPI] --- 测试失败 ---");
        Debug.Log("======================================");
        Debug.LogWarning($"[TestAPI] 收到错误: {errorMessage}");
    }
}