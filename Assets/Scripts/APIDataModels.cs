using System.Collections.Generic;
using System; // 必须导入这个才能使用 [Serializable]

/* * 这个文件包含了所有用于 API 通信的数据模型。
 * 它们都是纯粹的 C# 类，不继承 MonoBehaviour。
 * 它们都标记为 [Serializable] 以便 Unity 的 JsonUtility 可以处理它们。
 * * 注意：字段名 (例如 package_id) 被有意地设置为了 snake_case (下划线命名法)，
 * 这是为了与 Python/JSON 的通用约定完全匹配，
 * 从而让 JsonUtility.ToJson() 和 JsonUtility.FromJson() 可以直接工作。
 */

// --- 请求 (Unity → Python) ---

[Serializable]
public class BrickData
{
    public int brick_id;
    public float weight;
}

[Serializable]
public class PackageData
{
    public int package_id;
    public List<BrickData> bricks;
}

/// <summary>
/// 这是我们发送给 Python 的最顶层请求对象
/// </summary>
[Serializable]
public class GroupingRequest
{
    public List<PackageData> packages;
    public int num_groups;

    // 构造函数，方便创建
    public GroupingRequest()
    {
        packages = new List<PackageData>();
    }
}


// --- 响应 (Python → Unity) ---

/// <summary>
/// 这是我们从 Python 接收的最顶层响应对象
/// </summary>
[Serializable]
public class GroupingResponse
{
    public bool success;
    
    // 这是一个与包裹列表一一对应的分组ID列表
    // 例如：[1, 2, 1, 3, 2, ...]
    public List<int> group_assignments;

    // 这是一个包含每组中心重量的列表
    // 例如：[9.8, 10.1, 10.5, 11.0, 11.2]
    public List<float> group_centers;

    // 如果 success = false，这里会包含错误信息
    public string error_message;
}