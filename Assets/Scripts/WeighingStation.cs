using UnityEngine;

/*
 * WeighingStation: 这是一个 MonoBehaviour 类
 * 它将附加到我们场景中的 "WeighingStation" 游戏物体上。
 * 它利用 Unity 的物理引擎来检测物体进入。
 */
public class WeighingStation : MonoBehaviour
{
    // === 属性 ===
    // 我们将这些设置为 public，以便在 Inspector 中为每个站台单独设置

    /// <summary>
    /// 称重站的 ID (例如 1 到 5)
    /// </summary>
    public int stationId = 1;

    /// <summary>
    /// 这个称重站 *期望* 检测到的分组 ID
    /// (例如，1号站台期望检测 group_id 为 1 的包裹)
    /// </summary>
    public int targetGroupId = 1;

    /// <summary>
    /// (未来功能) 重量容差，单位：克
    /// 我们可以暂时先不用它，但先定义好
    /// </summary>
    public float tolerance_g = 2.0f;

    // === 统计数据 (私有) ===
    [SerializeField]
    private int packagesPassed = 0;

    [SerializeField]
    private int exceptionsDetected = 0;

    // === Unity 物理生命周期 ===

    /// <summary>
    /// 当另一个 Collider (碰撞体) 进入这个物体的 Trigger (触发器) 时，
    /// Unity 会自动调用这个方法。
    /// 
    /// **触发条件 (回答你的问题):**
    /// 1. 这个物体必须有一个 Collider 且 Is Trigger = true (我们上周已设置)。
    /// 2. 另一个物体 (other) 必须有一个 Collider。
    /// 3. 两个物体中 *至少一个* 必须有 Rigidbody (我们的 Package 有)。
    /// </summary>
    /// <param name="other">进入触发器的另一个物体的 Collider</param>
    void OnTriggerEnter(Collider other)
    {
        // 调试日志：检查是否检测到了 *任何* 物体
        // Debug.Log($"[Station {stationId}] Triggered by: {other.name}");

        // --- 如何获取 PackageController (回答你的问题) ---
        // 我们使用 GetComponent<T>() 方法。
        // 这会检查进入的物体 (other) 是否附加了 "PackageController" 脚本。
        PackageController package = other.GetComponent<PackageController>();

        // 如果 package 不为 null，说明进入的是一个我们关心的 "Package"
        if (package != null)
        {
            // 我们成功检测到了一个包裹！
            packagesPassed++; // 统计通过的包裹数量

            // --- 核心检测逻辑 ---
            // 获取包裹被分配的组ID
            int packageGroupId = package.GetGroupID();

            // 检查包裹的组ID是否与本站台期望的组ID匹配
            if (packageGroupId == this.targetGroupId)
            {
                // 正确的包裹到达了正确的站点
                Debug.Log($"[Station {stationId}] CORRECT Package detected. ID: {package.GetPackageID()}, Group: {packageGroupId}");
            }
            else
            {
                // 错误的包裹！
                exceptionsDetected++; // 统计异常数量
                Debug.LogWarning($"[Station {stationId}] WRONG Package detected! Expected Group {targetGroupId}, but got Group {packageGroupId}. Package ID: {package.GetPackageID()}");
            }

            // (可选) TODO: 可以在这里添加视觉反馈，比如让包裹或站台改变颜色
            
            // --- 如何防止重复检测 (回答你的问题) ---
            // OnTriggerEnter 在物体 *进入* 时只触发一次。
            // 如果包裹在触发器内来回移动，它会触发 OnTriggerStay。
            // 如果包裹 *离开*，它会触发 OnTriggerExit。
            // 对于我们的传送带场景 (包裹只会单向通过)，OnTriggerEnter 就是我们想要的，它天生就不会重复检测同一个包裹（除非它离开后又再次进入）。
        }
    }

}