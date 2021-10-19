public interface IMetrics
{
    IDisposable Duration(string name, string description = null, double[] buckets = null);
    void Duration(string name, string description, int value, double[] buckets = null);
    void Value(string name, double value, params (string labelName, string labelValue)[] instances);
    void Value(string name, double value);
    void RemoveValue(string name);
    void Count(string name, string description = null, int countNumber = 1);
    void Count(string name, string description = null, params (string labelName, string labelValue)[] instances);
    void Count(string name, params (string labelName, string labelValue)[] instances);
    void Count(string name, string description = null, int countNumber = 1, params (string labelName, string labelValue)[] instances);
}