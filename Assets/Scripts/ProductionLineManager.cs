using UnityEngine;
using System.Collections.Generic; // 必须导入这个才能使用 List<>

/// <summary>
/// 生产线的“总指挥”。
/// 这是一个单例 (Singleton)，负责生成和管理所有包裹。
/// </summary>
public class ProductionLineManager : MonoBehaviour
{
    // === 单例模式实现 ===
    // 这能确保场景中只有一个 Manager，并让其他脚本 (如 APIClient)
    // 可以通过 ProductionLineManager.Instance 轻松访问它。
    public static ProductionLineManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
        }
        else
        {
            Instance = this;
        }
    }

    // === Inspector 中可配置的属性 ===

    [Header("Prefab & Spawn Settings")]
    [SerializeField]
    private GameObject packagePrefab; // 包裹的 Prefab

    [SerializeField]
    private Transform spawnPoint; // 包裹生成位置

    [SerializeField]
    private int batchSize = 50; // 批次大小

    [SerializeField]
    private int bricksPerPackage = 10; // 每个包裹的积木数量

    [Header("Brick Weight Distribution (Normal)")]
    [SerializeField]
    private float meanWeight_g = 10.0f; // 均值

    [SerializeField]
    private float stdDev_g = 0.5f; // 标准差

    [SerializeField]
    private float minWeight_g = 8.0f; // 最小重量

    [SerializeField]
    private float maxWeight_g = 12.0f; // 最大重量


    // === 内部状态 ===
    private List<PackageController> currentBatch = new List<PackageController>();

    // 用于 Box-Muller 变换的静态变量
    private static bool hasSpareGaussian = false;
    private static float spareGaussian;


    // === 公开方法 ===

    /// <summary>
    /// 生成一个新批次的包裹。
    /// [ContextMenu("...")] 允许我们在 Inspector 中右键点击组件来运行此方法，非常适合测试！
    /// </summary>
    [ContextMenu("Generate New Batch")]
    public void GenerateNewBatch()
    {
        ClearBatch(); // 清理上一批

        for (int i = 0; i < batchSize; i++)
        {
            // 1. 克隆 Prefab 来创建包裹实例
            GameObject packageGO = Instantiate(packagePrefab, spawnPoint.position, Quaternion.identity);

            // 2. 获取包裹的控制器脚本
            PackageController package = packageGO.GetComponent<PackageController>();
            
            if (package != null)
            {
                int packageId = i + 1; // ID 从 1 开始
                package.name = $"Package_{packageId}"; // 在 Hierarchy 中重命名，方便调试
                package.SetPackageID(packageId);

                // 3. 为这个包裹生成积木列表
                List<Brick> bricksForThisPackage = new List<Brick>();
                int startBrickId = packageId * 100; // 确保积木 ID 也是唯一的
                for(int j = 0; j < bricksPerPackage; j++)
                {
                    // 调用正态分布生成器
                    float weight = GenerateNormalRandom(meanWeight_g, stdDev_g, minWeight_g, maxWeight_g);
                    bricksForThisPackage.Add(new Brick(startBrickId + j, weight));
                }

                // 4. 将生成好的列表设置给包裹
                package.SetBricksList(bricksForThisPackage);

                // 5. 添加到列表进行管理
                currentBatch.Add(package);
            }
        }

        Debug.Log($"[ProductionLineManager] 生成了 {currentBatch.Count} 个包裹的新批次。");
    }

    /// <summary>
    /// 清理（销毁）场景中所有已生成的包裹
    /// </summary>
    [ContextMenu("Clear Batch")]
    public void ClearBatch()
    {
        foreach (PackageController package in currentBatch)
        {
            if (package != null)
            {
                Destroy(package.gameObject);
            }
        }
        currentBatch.Clear(); // 清空列表
        Debug.Log("[ProductionLineManager] 已清理上一批包裹。");
    }

    /// <summary>
    /// 获取当前批次中所有包裹的列表 (只读)
    /// </summary>
    public List<PackageController> GetCurrentBatch()
    {
        return currentBatch;
    }


    // === 辅助方法 (正态分布生成器) ===

    /// <summary>
    /// 生成一个符合正态分布的随机数 (使用 Box-Muller 变换)。
    /// </summary>
    private float GenerateNormalRandom(float mean, float stdDev, float min, float max)
    {
        float result;
        if (hasSpareGaussian)
        {
            hasSpareGaussian = false;
            result = spareGaussian * stdDev + mean;
        }
        else
        {
            float u, v, s;
            do {
                u = Random.Range(-1.0f, 1.0f);
                v = Random.Range(-1.0f, 1.0f);
                s = u * u + v * v;
            } while (s >= 1.0f || s == 0f); // 确保在单位圆内

            s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);

            spareGaussian = v * s;
            hasSpareGaussian = true;

            result = u * s * stdDev + mean;
        }
        
        // 将结果限制在 [min, max] 范围内
        return Mathf.Clamp(result, min, max);
    }
}