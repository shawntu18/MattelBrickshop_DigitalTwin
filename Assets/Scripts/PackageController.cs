using UnityEngine;
using System.Collections.Generic; // 必须导入这个才能使用 List<>
using System;

/// <summary>
/// 包裹的“大脑”，附加到 Package Prefab 上。
/// 它主要是一个数据容器，负责存储自己的状态（ID, 重量, 积木, 分组）。
/// </summary>
public class PackageController : MonoBehaviour
{
    // === 内部状态 ===
    // [SerializeField] 让我们可以在 Inspector 中看到这些私有变量，方便调试
    
    [SerializeField]
    private int package_id;

    [SerializeField]
    private float actualTotalWeight_g;

    [SerializeField]
    private int group_id = 0; // 0 = 未分组

    [SerializeField]
    private List<Brick> bricks = new List<Brick>();

    /// <summary>
    /// Awake 在对象加载时执行一次 (在 Start 之前)
    /// </summary>
    void Awake()
    {
        // 确保列表被正确初始化
        if (bricks == null)
        {
            bricks = new List<Brick>();
        }
    }

    // === 公开的 Setters (由 Manager 调用) ===

    /// <summary>
    /// 设置此包裹的唯一 ID
    /// </summary>
    public void SetPackageID(int id)
    {
        this.package_id = id;
    }

    /// <summary>
    /// 由 ProductionLineManager 调用，设置这个包裹的完整积木列表
    /// </summary>
    public void SetBricksList(List<Brick> newBricksList)
    {
        this.bricks = newBricksList;
        // 收到积木列表后，立即计算总重量
        CalculateTotalWeight();
    }

    /// <summary>
    /// 设置由 Python API 返回的分组 ID
    /// </summary>
    public void SetGroupID(int id)
    {
        this.group_id = id;
    }

    // === 核心逻辑 ===

    /// <summary>
    /// 遍历所有积木并计算总重量，更新 actualTotalWeight_g 字段
    /// </summary>
    public float CalculateTotalWeight()
    {
        actualTotalWeight_g = 0f;
        foreach (Brick brick in bricks)
        {
            actualTotalWeight_g += brick.weight;
        }
        return actualTotalWeight_g;
    }

    // === 公开的 Getters (供其他脚本访问) ===
    
    public int GetPackageID()
    {
        return package_id;
    }

    public float GetActualTotalWeight()
    {
        return actualTotalWeight_g;
    }

    public List<Brick> GetBricks()
    {
        return bricks;
    }

    public int GetGroupID()
    {
        return group_id;
    }
}