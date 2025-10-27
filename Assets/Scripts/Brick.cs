using System;
using UnityEngine; // 我们需要这个来进行 [Serializable] 

/*
 * [Serializable] 是一个“标签” (Attribute)，它告诉 Unity 两件事：
 * 1. 这个类的实例可以被 Unity 的 JsonUtility 序列化(打包)成 JSON 字符串，以便发送给 API。
 * 2. 如果这个类的对象在一个 MonoBehaviour (比如 PackageController) 中作为 public 或 [SerializeField] 字段存在，
 * Unity 的 Inspector 窗口应该显示它的内容，这对于调试非常有用。
 */
[Serializable]
public class Brick
{
    // 注意：这个类 *不* 继承自 MonoBehaviour
    // 它是一个纯粹的数据容器 (POCO - Plain Old C# Object)
    // 这样的类更轻量，适合用来定义数据结构

    /* * 字段 (Fields)
     * 我们使用 public 字段而不是属性 (properties)，因为
     * Unity 的 JsonUtility 默认只序列化 public 字段。
     */

    /// <summary>
    /// 积木的唯一标识符
    /// </summary>
    public int id;

    /// <summary>
    /// 积木的重量，单位：克
    /// </summary>
    public float weight;

    /// <summary>
    /// 构造函数 (Constructor)
    /// 让我们可以在创建时方便地设置 ID 和重量
    /// </summary>
    /// <param name="id">积木 ID</param>
    /// <param name="weight">积木重量</param>
    public Brick(int id, float weight)
    {
        this.id = id;
        this.weight = weight;
    }

    /// <summary>
    /// 重写 ToString() 方法，这是一个非常好的习惯
    /// 当你调用 Debug.Log(myBrickObject) 时，它会打印出这个格式化的字符串，而不是 "Brick"
    /// 极大地提升了调试效率
    /// </summary>
    /// <returns>积木信息的格式化字符串</returns>
    public override string ToString()
    {
        // 使用 $"" 语法 (字符串插值) 来格式化字符串
        return $"Brick[ID: {id}, Weight: {weight:F2}g]"; 
        // :F2 表示将 float 格式化为保留两位小数
    }
}