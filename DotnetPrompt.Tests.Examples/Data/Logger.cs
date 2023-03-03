using Microsoft.Extensions.Logging;

namespace DotnetPrompt.Tests.Examples.Data;

public class Logger
{
    public void WorkerStart(string id, Guid pipelineId)
    {
        WriteInfo(17001, new
        {
            id,
            pipelineId
        }, () => "Start worker");
    }

    public void StartProcessingDocument(Guid pipelineId)
    {
        WriteInfo(17003, new
        {
            pipelineId
        }, () => "Start processing document");
    }

    public void EndProcessingDocument(Guid id, int leftCount, int rightCount,
        long elapsedMillisecond)
    {
        WriteInfo(17004, new
        {
            id,
            leftCount,
            rightCount,
            elapsedMillisecond
        }, () => $"End processing document. ElapsedMillisecond: {elapsedMillisecond}");
    }

    public void ProcessDocumentHandledException(Exception exception)
    {
        WriteError(17100, null, () => "Processing Handled Exception", exception);
    }

    public void ChangeTypeArgumentOutOfRangeWarning(object payload, ArgumentOutOfRangeException exception)
    {
        WriteWarning(171001, payload, () => $"Wrong change type. Exception message: {exception.Message}");
    }

    #region Fakes
    private void WriteWarning(int i, object payload, Func<string> func)
    {
        throw new NotImplementedException();
    }
    private void WriteInfo(int i, object o, Func<string> func)
    {
        throw new NotImplementedException();
    }
    private void WriteError(int i, object o, Func<string> func, Exception exception)
    {
        throw new NotImplementedException();
    }
    #endregion

}