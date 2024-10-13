namespace Slack_GPT_Socket.GptApi;

/// <summary>
///     Using source generator create a definition for OpenAI Structured output to produce proper function call via
///     typed method.
///     Implement method called CallExpression that will be called by the generated code.
///     Implement interface IExpressionGptTool to allow the generator to find the tool.
///     The generator will create a partial class that will implement definition based on CallExpression method.
/// </summary>
public interface IExpressionGptTool
{
    string Name { get; }
    Task<CallExpressionResult> CallExpressionInternal(string jsonParameters, Func<string, Type, object> deserialize);
}

public interface IExpressionGptTool<in T> : IExpressionGptTool
{
    Task<CallExpressionResult> CallExpression(T parameters);
}

public interface IExpressionGptTool<in T1, in T2> : IExpressionGptTool
{
    Task<CallExpressionResult> CallExpression(T1 parameters1, T2 parameters2);
}

public interface IExpressionGptTool<in T1, in T2, in T3> : IExpressionGptTool
{
    Task<CallExpressionResult> CallExpression(T1 parameters1, T2 parameters2, T3 parameters3);
}

public interface IExpressionGptTool<in T1, in T2, in T3, in T4> : IExpressionGptTool
{
    Task<CallExpressionResult> CallExpression(T1 parameters1, T2 parameters2, T3 parameters3, T4 parameters4);
}

public interface IExpressionGptTool<in T1, in T2, in T3, in T4, in T5> : IExpressionGptTool
{
    Task<CallExpressionResult> CallExpression(T1 parameters1, T2 parameters2, T3 parameters3, T4 parameters4, T5 parameters5);
}

public interface IExpressionGptTool<in T1, in T2, in T3, in T4, in T5, in T6> : IExpressionGptTool
{
    Task<CallExpressionResult> CallExpression(T1 parameters1, T2 parameters2, T3 parameters3, T4 parameters4, T5 parameters5,
        T6 parameters6);
}

public interface IExpressionGptTool<in T1, in T2, in T3, in T4, in T5, in T6, in T7> : IExpressionGptTool
{
    Task<CallExpressionResult> CallExpression(T1 parameters1, T2 parameters2, T3 parameters3, T4 parameters4, T5 parameters5,
        T6 parameters6, T7 parameters7);
}

public interface IExpressionGptTool<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8> : IExpressionGptTool
{
    Task<CallExpressionResult> CallExpression(T1 parameters1, T2 parameters2, T3 parameters3, T4 parameters4, T5 parameters5,
        T6 parameters6, T7 parameters7, T8 parameters8);
}

public interface IExpressionGptTool<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9> : IExpressionGptTool
{
    Task<CallExpressionResult> CallExpression(T1 parameters1, T2 parameters2, T3 parameters3, T4 parameters4, T5 parameters5,
        T6 parameters6, T7 parameters7, T8 parameters8, T9 parameters9);
}

public interface
    IExpressionGptTool<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10> : IExpressionGptTool
{
    Task<CallExpressionResult> CallExpression(T1 parameters1, T2 parameters2, T3 parameters3, T4 parameters4, T5 parameters5,
        T6 parameters6, T7 parameters7, T8 parameters8, T9 parameters9, T10 parameters10);
}

public interface
    IExpressionGptTool<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10,
        in T11> : IExpressionGptTool
{
    Task<CallExpressionResult> CallExpression(T1 parameters1, T2 parameters2, T3 parameters3, T4 parameters4, T5 parameters5,
        T6 parameters6, T7 parameters7, T8 parameters8, T9 parameters9, T10 parameters10, T11 parameters11);
}

public interface IExpressionGptTool<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11,
    in T12> : IExpressionGptTool
{
    Task<CallExpressionResult> CallExpression(T1 parameters1, T2 parameters2, T3 parameters3, T4 parameters4, T5 parameters5,
        T6 parameters6, T7 parameters7, T8 parameters8, T9 parameters9, T10 parameters10, T11 parameters11,
        T12 parameters12);
}

public interface IExpressionGptTool<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11,
    in T12, in T13> : IExpressionGptTool
{
    Task<CallExpressionResult> CallExpression(T1 parameters1, T2 parameters2, T3 parameters3, T4 parameters4, T5 parameters5,
        T6 parameters6, T7 parameters7, T8 parameters8, T9 parameters9, T10 parameters10, T11 parameters11,
        T12 parameters12, T13 parameters13);
}

public interface IExpressionGptTool<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11,
    in T12, in T13, in T14> : IExpressionGptTool
{
    Task<CallExpressionResult> CallExpression(T1 parameters1, T2 parameters2, T3 parameters3, T4 parameters4, T5 parameters5,
        T6 parameters6, T7 parameters7, T8 parameters8, T9 parameters9, T10 parameters10, T11 parameters11,
        T12 parameters12, T13 parameters13, T14 parameters14);
}

public interface
    IExpressionGptTool<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13,
        in T14, in T15> : IExpressionGptTool
{
    Task<CallExpressionResult> CallExpression(T1 parameters1, T2 parameters2, T3 parameters3, T4 parameters4, T5 parameters5,
        T6 parameters6, T7 parameters7, T8 parameters8, T9 parameters9, T10 parameters10, T11 parameters11,
        T12 parameters12, T13 parameters13, T14 parameters14, T15 parameters15);
}